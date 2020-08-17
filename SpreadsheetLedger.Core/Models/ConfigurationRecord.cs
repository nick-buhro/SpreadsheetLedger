namespace SpreadsheetLedger.Core.Models
{
    public sealed class ConfigurationRecord
    {
        public const string KEY_BASE_CURRENCY = "BASE_CURRENCY";

        public string Key { get; private set; }

        public string Value { get; private set; }
    }
}
