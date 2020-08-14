using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SpreadsheetLedger.Core.Impl
{
    public sealed class PriceImportStrategy : IPriceImportStrategy
    {
        private Dictionary<string, IPriceProvider> _providers;

        public PriceImportStrategy()
        {
            _providers = new Dictionary<string, IPriceProvider>
            {
                {
                    AlphaVantageProvider.ProviderCode,
                    new AlphaVantageProvider()
                }
            };
        }

        public async Task<IList<PriceRecord>> Import(
            IEnumerable<ConfigurationRecord> configuration,
            IEnumerable<PriceRecord> pricelist = null,
            CancellationToken ct = default)
        {
            var configs = configuration
                .Where(c => c.Key.StartsWith("IMPORT_PR:"))
                .ToList();

            if (configs.Count == 0)
                return new PriceRecord[0];

            var lastImportIndex = pricelist
                .Where(p => p.Date.HasValue && p.Price.HasValue && !string.IsNullOrEmpty(p.Symbol) && !string.IsNullOrEmpty(p.Currency) && !string.IsNullOrEmpty(p.Provider))
                .GroupBy(p => (p.Symbol, p.Currency, p.Provider))
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(p => p.Date.Value));
                        
            var tasks = new Queue<Task<IList<PriceRecord>>>(configs.Count);
            try
            {
                // Schedule import

                foreach (var config in configs)
                {
                    var cfg = ParseConfiguration(config.Key, config.Value);

                    if (!_providers.TryGetValue(cfg.provider, out var provider))
                        throw new LedgerException($"Provider '{cfg.provider}' not found. Supported only 'AV' (AlphaVantage).");

                    var from = cfg.from;
                    if (lastImportIndex.TryGetValue((cfg.symbol, cfg.currency, cfg.provider), out var imported))
                    {
                        if (from < imported.AddDays(1))
                            from = imported.AddDays(1);
                    }

                    ct.ThrowIfCancellationRequested();
                    tasks.Enqueue(provider.GetPrices(cfg.symbol, cfg.currency, from, cfg.options, ct));
                }

                // Add result

                var result = new List<PriceRecord>();
                while (tasks.Count > 0)
                {
                    ct.ThrowIfCancellationRequested();
                    result.AddRange(await tasks.Dequeue().ConfigureAwait(false));
                }
                return result;
            }
            finally
            {
                foreach (var t in tasks)                
                    t.Dispose();                
            }            
        }

        internal static (string provider, string symbol, string currency, DateTime from, IList<string> options) ParseConfiguration(string key, string value)
        {
            var keyMatch = Regex.Match(
                key,
                @"^IMPORT_PR:([^\s:/]*):([^\s:/]+)/([^\s:/]+)\s*$",
                RegexOptions.CultureInvariant);

            if (!keyMatch.Success)
                throw new LedgerException($"Can't parse '{key}' price import configuration.\r\nKey format: 'IMPORT_PR:[PROVIDER]:[SYMBOL]/[CUR]'.\r\nExample: 'IMPORT_PR:AV:EUR/USD'.");

            var provider = keyMatch.Groups[1].Value;
            var symbol = keyMatch.Groups[2].Value;
            var cur = keyMatch.Groups[3].Value;
            
            var valueMatch = Regex.Match(
                    value,
                    @"^\s*(\d\d\d\d-\d\d-\d\d)\s*(:\s*([^\s:]+)\s*)*$",
                    RegexOptions.CultureInvariant);

            if (!valueMatch.Success)
                throw new LedgerException($"Can't parse '{key}':'{value}' price import configuration.\r\nValue format: 'yyyy-MM-dd[:option1][:option2][:optionN]'.");

            var date = DateTime.ParseExact(valueMatch.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var options = new string[valueMatch.Groups[3].Captures.Count];
            for (var i = 0; i < options.Length; i++)
            {
                options[i] = valueMatch.Groups[3].Captures[i].Value;
            }

            return (provider, symbol, cur, date, options);

        }
    }
}
