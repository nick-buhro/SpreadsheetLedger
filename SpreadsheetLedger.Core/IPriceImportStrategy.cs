using SpreadsheetLedger.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SpreadsheetLedger.Core
{
    public interface IPriceImportStrategy
    {
        Task<IList<PriceRecord>> Import(
            IEnumerable<ConfigurationRecord> configuration,
            IEnumerable<PriceRecord> pricelist = null,
            CancellationToken ct = default);
    }
}
