using SpreadsheetLedger.Core.Models.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class GLRecord
    {
        public DateTime Date { get; set; }

        public string R { get; set; }

        public string Num { get; set; }

        public string Text { get; set; }

        [Name("Doc Text")]
        public string DocText { get; set; }

        public decimal? Amount { get; set; }
                
        [Name("Comm")]
        public string Commodity { get; set; }
                
        [Name("Amount DC")]
        public decimal? AmountDC { get; set; }

        [Name("Account Id")]
        public string AccountId { get; set; }

        [Name("Account Name")]
        public string AccountName { get; set; }

        [Name("Account Name 1")]
        public string AccountName1 { get; set; }

        [Name("Account Name 2")]
        public string AccountName2 { get; set; }

        [Name("Account Name 3")]
        public string AccountName3 { get; set; }

        [Name("Account Name 4")]
        public string AccountName4 { get; set; }

        [Name("Account Type")]
        public string AccountType { get; set; }

        [Name("Offset Account Id")]
        public string OffsetAccountId { get; set; }

        [Name("Offset Account Name")]
        public string OffsetAccountName { get; set; }

        public string Tag { get; set; }

        public string Project { get; set; }
    }
}
