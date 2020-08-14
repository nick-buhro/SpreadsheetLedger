using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SpreadsheetLedger.Core
{
    public interface IPriceProvider
    {
        Task<IList<PriceRecord>> GetPrices(
            string symbol,
            string currency,
            DateTime from,
            IList<string> options,
            CancellationToken ct);
    }
}
