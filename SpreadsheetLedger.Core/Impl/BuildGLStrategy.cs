using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpreadsheetLedger.Core.Impl
{
    public sealed class BuildGLStrategy : IBuildGLStrategy
    {
        private Dictionary<string, AccountRecord> _coa;
        private ICurrencyConverter _converter;
        private List<JournalRecord> _journal;
        private List<GLRecord> _result;

        private int _journalNextIndex;
        private Dictionary<(string account, string comm), (string comm, decimal amount, decimal amountbc)> _runningBalance;

        public IList<GLRecord> Build(
            IEnumerable<AccountRecord> accountRecords,
            IEnumerable<JournalRecord> journalRecords,
            IEnumerable<CurrencyRecord> currencyRecords,
            IEnumerable<PriceRecord> priceRecords)
        {
            Trace.Assert(_coa == null);
            Trace.Assert(_converter == null);
            Trace.Assert(_journal == null);
            Trace.Assert(_result == null);
            Trace.Assert(_runningBalance == null);

            Trace.Assert(accountRecords != null);
            Trace.Assert(journalRecords != null);
            Trace.Assert(currencyRecords != null);
            Trace.Assert(priceRecords != null);

            try
            {

                _coa = accountRecords
                    .Where(a => !string.IsNullOrEmpty(a.AccountId))
                    .ToDictionary(a => a.AccountId);

                _converter = new CurrencyConverter(currencyRecords, priceRecords);

                _journal = journalRecords
                    .Where(j => j.Date.HasValue)
                    .OrderBy(j => j.Date.Value)
                    .ThenBy(j => j.ToBalance.HasValue)
                    .ToList();

                _result = new List<GLRecord>();
                if (_journal.Count > 0)
                {
                    _journalNextIndex = 0;
                    _runningBalance = new Dictionary<(string account, string comm), (string comm, decimal amount, decimal amountbc)>();

                    var minDate = _journal[0].Date.Value;
                    var maxDate = _journal[_journal.Count - 1].Date.Value;
                    if (maxDate < DateTime.Today)
                        maxDate = DateTime.Today;
                    maxDate = maxDate.AddMonths(1);
                    maxDate = new DateTime(maxDate.Year, maxDate.Month, 1);

                    for (var dt = minDate; dt <= maxDate; dt = dt.AddDays(1))
                    {
                        AppendJournalRecords(dt);
                        AppendRevaluations(dt);
                    }
                }
                return _result;
            }
            finally
            {
                _coa = null;
                _converter = null;
                _journal = null;
                _result = null;
                _journalNextIndex = 0;
                _runningBalance = null;
            }
        }



        private void AppendJournalRecords(DateTime dt)
        {
            var toBalanceSection = false;
            while (true)
            {
                if (_journalNextIndex >= _journal.Count) return;
                var j = _journal[_journalNextIndex];
                Trace.Assert(j.Date.HasValue);
                Trace.Assert(j.Date >= dt);
                if (j.Date > dt) return;
                Trace.Assert(j.Date == dt);
                _journalNextIndex++;

                try
                {
                    // Calculate prices

                    decimal amount;
                    if (j.ToBalance != null)
                    {
                        if (j.Amount != null)
                            throw new LedgerException("Amount and To Balance is specified. Leave only one value.");
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
                    var amountbc = _converter.Convert(amount, j.Commodity, dt);

                    // Find accounts

                    if (!_coa.TryGetValue(j.AccountId, out AccountRecord account))
                        throw new LedgerException($"Account '{j.AccountId}' not found.");
                    
                    if (!_coa.TryGetValue(j.OffsetAccountId, out AccountRecord offsetAccount))
                        throw new LedgerException($"Offset account '{j.OffsetAccountId}' not found.");

                    // Find project

                    string project = null;
                    if (!string.IsNullOrEmpty(account.Project))
                    {
                        project = account.Project;
                        if (!string.IsNullOrEmpty(offsetAccount.Project) && project != offsetAccount.Project)
                            throw new LedgerException($"'{j.AccountId} account project doesn't equal to '{j.OffsetAccountId}' offset account project.");
                    }
                    else if (!string.IsNullOrEmpty(offsetAccount.Project))
                    {
                        project = offsetAccount.Project;
                    }

                    // Add GL record

                    AddGLTransaction(
                        dt, j.R, j.Num, j.Text, amount, j.Commodity, amountbc,
                        account, offsetAccount,
                        j.Tag, project, j.DocText);
                }
                catch (Exception ex)
                {
                    throw new LedgerException("Journal procesing error: " + j.ToString(), ex);
                }
            }
        }

        private void AppendRevaluations(DateTime dt, string accountId = null)
        {
            // Revaluate all accounts only at the end of month
            if (accountId == null && dt.AddDays(1).Day != 1)
                return;

            var keys = _runningBalance.Keys
                .Where(k => accountId == null || k.account == accountId)
                .ToList();

            foreach (var key in keys)
            {
                try
                {
                    // if (key.comm == _converter.BaseCommodity) continue;

                    var account = _coa[key.account];
                    if (IsEquityAccount(account)) continue;

                    // Calculate correction

                    var balance = _runningBalance[key];
                    var newBalance = _converter.Convert(balance.amount, key.comm, dt);
                    var correction = newBalance - balance.amountbc;
                    if (correction == 0) continue;

                    var text = $"{balance.amount} {key.comm}: {balance.amountbc} => {newBalance}";

                    // Find revaluation account

                    if (!_coa.TryGetValue(account.RevaluationAccountId, out var revaluationAccount))
                        throw new LedgerException($"Revaluation account '{account.RevaluationAccountId}' for '{account.AccountId}' not found.");

                    if (!IsEquityAccount(revaluationAccount))
                        throw new LedgerException($"Revaluation account '{account.RevaluationAccountId}' for '{account.AccountId}' is not of type 'E' (Equity).");

                    // Validate project

                    string project = null;
                    if (!string.IsNullOrEmpty(account.Project))
                    {
                        project = account.Project;
                        if (!string.IsNullOrEmpty(revaluationAccount.Project) && revaluationAccount.Project != project)
                            throw new Exception($"'{account.AccountId} account project doesn't equal to '{revaluationAccount.AccountId}' revaluation account project.");
                    }
                    else if (!string.IsNullOrEmpty(revaluationAccount.Project))
                    {
                        project = revaluationAccount.Project;
                    }

                    // Add GL record

                    AddGLTransaction(dt, "r", null, text, null, key.comm, correction, account, revaluationAccount, null, project, null);
                }
                catch (Exception ex)
                {
                    throw new LedgerException($"Revaluation error for '{key.account}' {key.comm} on {dt.ToShortDateString()}.", ex);
                }
            }
        }

        private void AddGLTransaction(
            DateTime date, string r, string num, string text, decimal? amount, string comm, decimal amountdc,
            AccountRecord account, AccountRecord offset,
            string tag, string project, string docText)
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
                DocText = docText
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
                DocText = docText
            });
        }


        private (string comm, decimal amount, decimal amountbc) UpdateBalanceTable(string accountId, string commodity, decimal amount, decimal amountbc)
        {
            Trace.Assert(!string.IsNullOrWhiteSpace(accountId));
            Trace.Assert(!string.IsNullOrWhiteSpace(commodity));

            var key = (accountId, commodity);
            if (_runningBalance.TryGetValue(key, out var value))
            {
                value = (commodity, value.amount + amount, value.amountbc + amountbc);
                _runningBalance[key] = value;
                return value;
            }
            else
            {
                _runningBalance.Add(key, (commodity, amount, amountbc));
                return (commodity, amount, amountbc);
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
    }
}
