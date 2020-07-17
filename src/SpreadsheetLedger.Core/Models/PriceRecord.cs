using SpreadsheetLedger.Core.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class PriceRecord
    {
        public DateTime? Date { get; set; }

        [Name("Comm")]
        public string Commodity { get; set; }
                
        public decimal? Price { get; set; }
    }
}
