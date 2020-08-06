using System;

namespace SpreadsheetLedger.Core
{
    public interface ICurrencyConverter
    {
        decimal Convert(DateTime date, decimal amount, string symbol);

        decimal Round(decimal amount, string symbol = null);
    }
}
