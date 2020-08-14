using SpreadsheetLedger.Core.Models.Attributes;
using System;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class PriceRecord
    {
        public DateTime? Date { get; set; }
                
        public string Symbol { get; set; }
                
        public decimal? Price { get; set; }

        public string Currency { get; set; }

        public string Provider { get; set; }
    }
}
