using SpreadsheetLedger.Core.Models.Attributes;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class PLCategoryRecord: Record
    {
        [Mandatory(true)]
        [Name("P/L Category Id")]
        public string PLCategoryId { get; private set; }

        [Mandatory]
        [Name("P/L Category Name 1")]
        public string Name1 { get; private set; }

        [Mandatory]
        [Name("P/L Category Name 2")]
        public string Name2 { get; private set; }

        [Mandatory]
        [Name("P/L Category Name 3")]
        public string Name3 { get; private set; }

        [Mandatory]
        [Name("Closing Account Id")]
        public string ClosingAccount { get; private set; }

        public string Project { get; private set; }
    }
}
