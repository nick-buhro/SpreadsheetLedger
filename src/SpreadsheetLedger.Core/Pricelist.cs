using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpreadsheetLedger.Core
{
    public sealed class Pricelist
    {
        private readonly Dictionary<string, List<PriceRecord>> _index;

        public string BaseCommodity { get; }

        public Pricelist(string baseCommodity, IEnumerable<PriceRecord> prices)
        {
            Trace.Assert(!string.IsNullOrEmpty(baseCommodity));
            Trace.Assert(prices != null);
            BaseCommodity = baseCommodity;
            _index = prices
                .Where(p => p.Date.HasValue && !string.IsNullOrEmpty(p.Commodity) && p.Price.HasValue)
                .GroupBy(p => p.Commodity)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(p => p.Date).ToList());
        }

        public void Add(PriceRecord price)
        {
            throw new NotImplementedException();
        }


        public decimal CalculateAmountBC(DateTime date, decimal amount, string commodity)
        {
            if (amount == 0) return 0;
            if (commodity == BaseCommodity) return amount;

            var price = GetPrice(date, commodity);
            return Math.Round(amount * price, 2);
        }

        private decimal GetPrice(DateTime date, string commodity)
        {
            //TODO: optimize performance
            if (_index.TryGetValue(commodity, out var list))
            {
                if (date < list[0].Date.Value)
                    throw new Exception($"'{commodity}' prices on {date:d} not found.");

                if (date >= list[list.Count - 1].Date.Value)
                    return list[list.Count - 1].Price.Value;

                for (var i = 0; i < list.Count; i++)
                {
                    if (date == list[i].Date.Value)
                        return list[i].Price.Value;

                    if (date < list[i].Date.Value)
                        return list[i - 1].Price.Value;
                }

                throw new InvalidOperationException();
            }
            else
            {
                throw new Exception($"'{commodity}' prices not found.");
            }
        }
    }
}
