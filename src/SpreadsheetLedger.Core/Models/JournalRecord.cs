using SpreadsheetLedger.Core.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class JournalRecord
    {
        public DateTime? Date { get; private set; }

        public string R { get; private set; }

        public string Num { get; private set; }

        public string Description { get; private set; }
                
        public decimal? Amount { get; private set; }

        [Name("To Balance")]
        public decimal? ToBalance { get; private set; }

        [Name("Comm")]
        public string Commodity { get; private set; }

        [Name("Account Id")]
        public string AccountId { get; private set; }

        [Name("P/L Category Id")]
        public string PLCategoryId { get; private set; }

        [Name("P/L Tag")]
        public string PLTag { get; private set; }

        [Name("Source Ref")]
        public string SourceRef { get; private set; }

        [Name("Source Ln")]
        public string SourceLn { get; private set; }

        [Name("Source Memo")]
        public string SourceMemo { get; private set; }


        public override string ToString()
        {
            return $"{Date:d} {R} {Num} {Description}\t {Amount,6}{ToBalance,6} {Commodity} [{AccountId} <= {PLCategoryId}]";
        }
    }
}
