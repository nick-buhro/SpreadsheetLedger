using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpreadsheetLedger.Core
{
    public interface ICurrencyConverter
    {
        decimal Convert(DateTime date, decimal amount, string symbol);
    }
}
