using SpreadsheetLedger.Core.Models.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class GLRecord: Record
    {
        [Mandatory(true)]
        public DateTime Date { get; set; }

        public string R { get; set; }

        public string Num { get; set; }

        public string Description { get; set; }

        public decimal? Amount { get; set; }
                
        [Mandatory]
        [Name("Comm")]
        public string Commodity { get; set; }

        [Mandatory]
        public decimal? AmountDC { get; set; }

        [Mandatory]
        [Name("Account Id")]
        public string AccountId { get; set; }

        [Mandatory]
        [Name("Account Name 1")]
        public string AccountName1 { get; set; }

        [Mandatory]
        [Name("Account Name 2")]
        public string AccountName2 { get; set; }

        [Mandatory]
        [Name("Account Name 3")]
        public string AccountName3 { get; set; }

        [Mandatory]
        [Name("Account Name 4")]
        public string AccountName4 { get; set; }

        [Mandatory]
        [Name("Account Type")]
        public string AccountType { get; set; }


        [Mandatory]
        [Name("P/L Category Id")]
        public string PLCategoryId { get; set; }

        [Mandatory]
        [Name("P/L Category Name 1")]
        public string PLCategoryName1 { get; set; }

        [Mandatory]
        [Name("P/L Category Name 2")]
        public string PLCategoryName2 { get; set; }

        [Mandatory]
        [Name("P/L Category Name 3")]
        public string PLCategoryName3 { get; set; }


        [Name("P/L Tag")]
        public string PLTag { get; set; }

        public string Project { get; set; }

        public bool IsClosing { get; set; }

        [Name("Source Ref")]
        public string SourceRef { get; set; }

        [Name("Source Ln")]
        public string SourceLn { get; set; }

        [Name("Source Memo")]
        public string SourceMemo { get; set; }
    }
}
