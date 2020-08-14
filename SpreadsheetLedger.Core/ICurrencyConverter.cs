using System;

namespace SpreadsheetLedger.Core
{
    public interface ICurrencyConverter
    {
        decimal Convert(decimal amount, string symbol, DateTime date, string currency = null);        
    }
}
