using SpreadsheetLedger.Core.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class JournalRecord
    {
        public DateTime? Date { get; private set; }

        public string R { get; private set; }

        public string Num { get; private set; }

        public string Text { get; private set; }
                
        public decimal? Amount { get; private set; }

        [Name("To Balance")]
        public decimal? ToBalance { get; private set; }

        [Name("Comm")]
        public string Commodity { get; private set; }

        [Name("Account Id")]
        public string AccountId { get; private set; }

        [Name("Offset Account Id")]
        public string OffsetAccountId { get; private set; }

        [Name("Tag")]
        public string Tag { get; private set; }

        [Name("Source Ref")]
        public string SourceRef { get; private set; }

        [Name("Source Ln")]
        public string SourceLn { get; private set; }

        [Name("Source Text")]
        public string SourceText { get; private set; }


        public override string ToString()
        {
            return $"{Date:d} {R} {Num} {Text}\t {Amount,6}{ToBalance,6} {Commodity} [{AccountId} <= {OffsetAccountId}]";
        }
    }
}
