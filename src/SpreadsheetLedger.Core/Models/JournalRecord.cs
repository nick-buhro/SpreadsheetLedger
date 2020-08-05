using SpreadsheetLedger.Core.Models.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class JournalRecord
    {
        public DateTime? Date { get; private set; }

        public string R { get; private set; }

        public string Num { get; private set; }

        public string Text { get; private set; }

        [Name("Doc Text")]
        public string DocText { get; private set; }
                
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


        public override string ToString()
        {
            return $"{Date:d} {R} {Num} {Text}\t {Amount,6}{ToBalance,6} {Commodity} [{AccountId} <= {OffsetAccountId}]";
        }
    }
}
