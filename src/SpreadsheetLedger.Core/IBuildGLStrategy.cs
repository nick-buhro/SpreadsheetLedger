using SpreadsheetLedger.Core.Models;
using System.Collections.Generic;

namespace SpreadsheetLedger.Core
{    
    public interface IBuildGLStrategy
    {
        IList<GLRecord> Build(
            IEnumerable<AccountRecord> accounts,
            IEnumerable<JournalRecord> journal,
            IEnumerable<CurrencyRecord> currencies,
            IEnumerable<PriceRecord> pricelist);
    }
}
