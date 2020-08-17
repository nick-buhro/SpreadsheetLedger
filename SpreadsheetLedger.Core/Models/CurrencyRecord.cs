using SpreadsheetLedger.Core.Models.Attributes;

namespace SpreadsheetLedger.Core.Models
{
    public sealed class CurrencyRecord
    {
        public string Currency { get; private set; }

        [Name("Convertation Rule")]
        public string ConvertationRule { get; private set; }
    }
}
