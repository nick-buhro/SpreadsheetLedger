using SpreadsheetLedger.Core.Models.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class PriceRecord: Record
    {
        public DateTime Date { get; set; }

        [Name("Comm")]
        public string Commodity { get; set; }
                
        public decimal Price { get; set; }
    }
}
