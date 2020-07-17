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
        private IDictionary<string, PLCategoryRecord> _categoryIndex;
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

                _categoryIndex = _context.CoPL.Read()
                    .Where(c => !string.IsNullOrEmpty(c.PLCategoryId))
                    .ToDictionary(c => c.PLCategoryId);

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
                _categoryIndex = null;
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

                    var account = _accountIndex[j.AccountId];
                    var category = _categoryIndex[j.PLCategoryId];
                    var closingAccount = _accountIndex[category.ClosingAccount];

                    // Validate accounts

                    IsEquityAccount(account);
                    if (!IsEquityAccount(closingAccount))
                        throw new Exception($"Closing account for '{j.PLCategoryId}' is not of type 'E' (Equity).");

                    // Validate project

                    string project = null;
                    if (!string.IsNullOrEmpty(account.Project))
                    {
                        project = account.Project;
                        if (!string.IsNullOrEmpty(category.Project) && (category.Project != project))
                            throw new Exception($"'{j.AccountId} account project doesn't equal to '{j.PLCategoryId}' P/L Category project.");
                    }
                    if (!string.IsNullOrEmpty(category.Project))
                    {
                        project = category.Project;
                    }

                    // Add GL record                    

                    UpdateBalanceTable(j.AccountId, j.Commodity, amount, amountbc);
                    _result.Add(new GLRecord
                    {
                        Date = dt,
                        R = j.R,
                        Num = j.Num,
                        Description = j.Description,
                        Amount = amount,
                        Commodity = j.Commodity,
                        AmountDC = amountbc,
                        AccountId = account.AccountId,
                        AccountName1 = account.Name1,
                        AccountName2 = account.Name2,
                        AccountName3 = account.Name3,
                        AccountName4 = account.Name4,
                        AccountType = account.Type,
                        PLCategoryId = category.PLCategoryId,
                        PLCategoryName1 = category.Name1,
                        PLCategoryName2 = category.Name2,
                        PLCategoryName3 = category.Name3,
                        PLTag = j.PLTag,
                        Project = project,
                        Closing = false,
                        SourceRef = j.SourceRef,
                        SourceLn = j.SourceLn,
                        SourceMemo = j.SourceMemo
                    });

                    // Add closing GL record                    

                    UpdateBalanceTable(closingAccount.AccountId, j.Commodity, -amount, -amountbc);
                    _result.Add(new GLRecord
                    {
                        Date = dt,
                        R = j.R,
                        Num = j.Num,
                        Description = j.Description,
                        Amount = -amount,
                        Commodity = j.Commodity,
                        AmountDC = -amountbc,
                        AccountId = closingAccount.AccountId,
                        AccountName1 = closingAccount.Name1,
                        AccountName2 = closingAccount.Name2,
                        AccountName3 = closingAccount.Name3,
                        AccountName4 = closingAccount.Name4,
                        AccountType = closingAccount.Type,
                        PLCategoryId = category.PLCategoryId,
                        PLCategoryName1 = category.Name1,
                        PLCategoryName2 = category.Name2,
                        PLCategoryName3 = category.Name3,
                        PLTag = j.PLTag,
                        Project = project,
                        Closing = true,
                        SourceRef = j.SourceRef,
                        SourceLn = j.SourceLn,
                        SourceMemo = j.SourceMemo
                    });
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

            foreach (var key in _balanceTable.Keys)
            {
                if (key.comm == _context.BaseCommodity) continue;
                
                var account = _accountIndex[key.account];
                if (IsEquityAccount(account)) continue;

                // Calculate correction

                var balance = _balanceTable[key];
                var correction = _pricelist.CalculateAmountBC(dt, balance.amount, key.comm) - balance.amountbc;
                if (correction == 0) continue;

                // Find other accounts;

                var category = _categoryIndex[account.RevaluationPLCategory];
                var closingAccount = _accountIndex[category.ClosingAccount];

                // Validate accounts

                if (!IsEquityAccount(closingAccount))
                    throw new Exception($"Closing account for '{category.PLCategoryId}' is not of type 'E' (Equity).");

                // Validate project

                string project = null;
                if (!string.IsNullOrEmpty(account.Project))
                {
                    project = account.Project;
                    if (!string.IsNullOrEmpty(category.Project) && (category.Project != project))
                        throw new Exception($"'{account.AccountId} account project doesn't equal to '{category.PLCategoryId}' P/L Category project.");
                }
                if (!string.IsNullOrEmpty(category.Project))
                {
                    project = category.Project;
                }

                // Add GL record                    

                UpdateBalanceTable(key.account, key.comm, 0, correction);
                _result.Add(new GLRecord
                {
                    Date = dt,
                    R = "r",                                        
                    Commodity = key.comm,
                    AmountDC = correction,
                    AccountId = account.AccountId,
                    AccountName1 = account.Name1,
                    AccountName2 = account.Name2,
                    AccountName3 = account.Name3,
                    AccountName4 = account.Name4,
                    AccountType = account.Type,
                    PLCategoryId = category.PLCategoryId,
                    PLCategoryName1 = category.Name1,
                    PLCategoryName2 = category.Name2,
                    PLCategoryName3 = category.Name3,                    
                    Project = project,
                    Closing = false
                });

                // Add closing GL record                    

                UpdateBalanceTable(closingAccount.AccountId, key.comm, 0, -correction);
                _result.Add(new GLRecord
                {
                    Date = dt,
                    R = "r",
                    Commodity = key.comm,
                    AmountDC = -correction,
                    AccountId = closingAccount.AccountId,
                    AccountName1 = closingAccount.Name1,
                    AccountName2 = closingAccount.Name2,
                    AccountName3 = closingAccount.Name3,
                    AccountName4 = closingAccount.Name4,
                    AccountType = closingAccount.Type,
                    PLCategoryId = category.PLCategoryId,
                    PLCategoryName1 = category.Name1,
                    PLCategoryName2 = category.Name2,
                    PLCategoryName3 = category.Name3,
                    Project = project,
                    Closing = true
                });
            }
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
    }
}
