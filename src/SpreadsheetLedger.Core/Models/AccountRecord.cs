﻿using SpreadsheetLedger.Core.Attributes;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class AccountRecord
    {
        [Name("Account Id")]
        public string AccountId { get; private set; }

        [Name("Account Name 1")]
        public string Name1 { get; private set; }

        [Name("Account Name 2")]
        public string Name2 { get; private set; }

        [Name("Account Name 3")]
        public string Name3 { get; private set; }

        [Name("Account Name 4")]
        public string Name4 { get; private set; }

        [Name("Account Type")]
        public string Type { get; private set; }

        [Name("Revaluation P/L Category")]
        public string RevaluationPLCategory { get; private set; }
                
        public string Project { get; private set; }
    }
}
