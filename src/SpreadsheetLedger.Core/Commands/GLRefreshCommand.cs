using SpreadsheetLedger.Core.Helpers;
using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpreadsheetLedger.Core.Commands
{
    public sealed class GLRefreshCommand : ICommand
    {
        private readonly IContext _context;

        private IDictionary<string, AccountRecord> _accountIndex;        
        private Pricelist _pricelist;
        private IList<JournalRecord> _journalRecords;
        private IList<GLRecord> _result;        

        private int _nextJournalRecord;
        private Dictionary<(string account, string comm), (decimal amount, decimal amountbc)> _balanceTable;

        public GLRefreshCommand(IContext context) 
        {
            _context = context;
        }

        public void Execute()
        {
            try
            {
                // Read data

                _accountIndex = _context.CoA.Read()
                    .Where(a => !string.IsNullOrEmpty(a.AccountId))
                    .ToDictionary(a => a.AccountId);
                                
                _pricelist = new Pricelist(
                    _context.BaseCommodity,
                    _context.Prices.Read());

                _journalRecords = _context.Journal.Read()
                    .Where(j => j.Date.HasValue)
                    .OrderBy(j => j.Date.Value)
                    .ThenBy(j => j.ToBalance.HasValue)
                    .ToList();

                _result = new List<GLRecord>(_journalRecords.Count * 4);

                // Buld GL

                if (_journalRecords.Count > 0)
                {
                    _nextJournalRecord = 0;
                    _balanceTable = new Dictionary<(string account, string comm), (decimal amount, decimal amountbc)>();

                    var minDate = _journalRecords[0].Date.Value;
                    var maxDate = _journalRecords[_journalRecords.Count - 1].Date.Value;
                    maxDate = maxDate.AddMonths(1);
                    maxDate = new DateTime(maxDate.Year, maxDate.Month, 1);

                    for (var dt = minDate; dt <= maxDate; dt = dt.AddDays(1))
                    {
                        AppendJournalRecords(dt);
                        AppendRevaluations(dt);
                    }
                }

                // Write result

                _context.GL.Write(_result, true);
            }
            finally
            {
                _accountIndex = null;
                _pricelist = null;
                _journalRecords = null;
                _result = null;
                _nextJournalRecord = 0;
                _balanceTable = null;
            }
        }

        private void AppendJournalRecords(DateTime dt)
        {
            var toBalanceSection = false;
            while (true)
            {
                if (_nextJournalRecord >= _journalRecords.Count) return;
                var j = _journalRecords[_nextJournalRecord];
                Trace.Assert(j.Date >= dt);
                if (j.Date > dt) return;
                Trace.Assert(j.Date == dt);
                _nextJournalRecord++;

                try
                {                   
                    // Calculate prices

                    decimal amount;
                    if (j.ToBalance != null)
                    {
                        if (j.Amount != null)
                            throw new Exception("Amount and To Balance is specified. Leave only one value.");
                        amount = j.ToBalance.Value - UpdateBalanceTable(j.AccountId, j.Commodity, 0, 0).amount;
                        toBalanceSection = true;
                    }
                    else
                    {                        
                        if (j.Amount == null) continue;
                        amount = j.Amount.Value;
                        Trace.Assert(!toBalanceSection);        // "To Balance" records should be at the end during the day
                    }

                    if (amount == 0) continue;
                    var amountbc = _pricelist.CalculateAmountBC(dt, amount, j.Commodity);

                    // Find accounts

                    var account = TryExecute(
                        () => _accountIndex[j.AccountId],
                        $"Account '{j.AccountId}' not found.");

                    var offsetAccount = TryExecute(
                        () => _accountIndex[j.OffsetAccountId],
                        $"Offset account '{j.OffsetAccountId}' not found.");                                        

                    // Validate project

                    string project = null;
                    if (!string.IsNullOrEmpty(account.Project))
                    {
                        project = account.Project;
                        if (!string.IsNullOrEmpty(offsetAccount.Project) && (offsetAccount.Project != project))
                            throw new Exception($"'{j.AccountId} account project doesn't equal to '{j.OffsetAccountId}' offset account project.");
                    }
                    if (!string.IsNullOrEmpty(offsetAccount.Project))
                    {
                        project = offsetAccount.Project;
                    }

                    // Add GL record

                    AddGLTransaction(
                        dt, j.R, j.Num, j.Text, amount, j.Commodity, amountbc,
                        account, offsetAccount,
                        j.Tag, project,
                        j.SourceRef, j.SourceLn, j.SourceText);
                }
                catch (Exception ex)
                {
                    throw new Exception("Journal procesing error: " + j.ToString(), ex);
                }                
            }
        }
                
        private void AppendRevaluations(DateTime dt)
        {
            if (dt.AddDays(1).Day != 1) return;

            var keys = _balanceTable.Keys.ToList();
            foreach (var key in keys)
            {
                if (key.comm == _context.BaseCommodity) continue;
                
                var account = _accountIndex[key.account];
                if (IsEquityAccount(account)) continue;

                // Calculate correction

                var balance = _balanceTable[key];
                var correction = _pricelist.CalculateAmountBC(dt, balance.amount, key.comm) - balance.amountbc;
                if (correction == 0) continue;

                // Find revaluation account

                var revaluationAccount = TryExecute(
                    () => _accountIndex[account.RevaluationAccountId],
                    $"Revaluation account '{account.RevaluationAccountId}' for '{account.AccountId}' not found.");
                
                if (!IsEquityAccount(revaluationAccount))
                    throw new Exception($"Revaluation account '{account.RevaluationAccountId}' for '{account.AccountId}' is not of type 'E' (Equity).");

                // Validate project

                string project = null;
                if (!string.IsNullOrEmpty(account.Project))
                {
                    project = account.Project;
                    if (!string.IsNullOrEmpty(revaluationAccount.Project) && (revaluationAccount.Project != project))
                        throw new Exception($"'{account.AccountId} account project doesn't equal to '{revaluationAccount.AccountId}' revaluation account project.");
                }
                if (!string.IsNullOrEmpty(revaluationAccount.Project))
                {
                    project = revaluationAccount.Project;
                }

                // Add GL record                    

                AddGLTransaction(dt, "r", null, null, null, key.comm, correction, account, revaluationAccount, null, project, null, null, null);
            }
        }


        private void AddGLTransaction(
            DateTime date, string r, string num, string text, decimal? amount, string comm, decimal amountdc,
            AccountRecord account, AccountRecord offset,
            string tag, string project, string sourceRef, string sourceLn, string sourceText)
        {
            UpdateBalanceTable(account.AccountId, comm, amount ?? 0, amountdc);
            UpdateBalanceTable(offset.AccountId, comm, -amount ?? 0, -amountdc);

            _result.Add(new GLRecord
            {
                Date = date,
                R = r,
                Num = num,
                Text = text,
                Amount = amount,
                Commodity = comm,
                AmountDC = amountdc,
                AccountId = account.AccountId,
                AccountName = account.Name,
                AccountName1 = account.Name1,
                AccountName2 = account.Name2,
                AccountName3 = account.Name3,
                AccountName4 = account.Name4,
                AccountType = account.Type,
                OffsetAccountId = offset.AccountId,
                OffsetAccountName = offset.Name,
                Tag = tag,
                Project = project,
                SourceRef = sourceRef,
                SourceLn = sourceLn,
                SourceText = sourceText
            });

            _result.Add(new GLRecord
            {
                Date = date,
                R = r,
                Num = num,
                Text = text,
                Amount = -amount,
                Commodity = comm,
                AmountDC = -amountdc,
                AccountId = offset.AccountId,
                AccountName = offset.Name,
                AccountName1 = offset.Name1,
                AccountName2 = offset.Name2,
                AccountName3 = offset.Name3,
                AccountName4 = offset.Name4,
                AccountType = offset.Type,
                OffsetAccountId = account.AccountId,
                OffsetAccountName = account.Name,
                Tag = tag,
                Project = project,
                SourceRef = sourceRef,
                SourceLn = sourceLn,
                SourceText = sourceText
            });
        }


        private (decimal amount, decimal amountbc) UpdateBalanceTable(string accountId, string commodity, decimal amount, decimal amountbc)
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(accountId));
            Trace.Assert(!string.IsNullOrWhiteSpace(commodity));

            var key = (accountId, commodity);
            if (_balanceTable.TryGetValue(key, out var value))
            {
                value = (value.amount + amount, value.amountbc + amountbc);
                _balanceTable[key] = value;
                return value;
            }
            else
            {
                _balanceTable.Add(key, (amount, amountbc));
                return (amount, amountbc);
            }
        }

        private static bool IsEquityAccount(AccountRecord account)
        {
            switch (account.Type)
            {
                case "A":
                case "L":
                    return false;
                case "E":
                    return true;
                default:
                    throw new Exception($"Account '{account.AccountId}' has unsupported type: '{account.Type}'. Supported: 'A' (Assets), 'L' (Liabilities) and 'E' (Equity).");
            }
        }

        private static T TryExecute<T>(Func<T> action, string errorMessage)
        {
            try
            {
                return action();
            }
            catch
            {
                throw new Exception(errorMessage);
            }
        }
    }
}
