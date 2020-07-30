using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpreadsheetLedger.Core
{
    public static partial class PriceProvider
    {        
        public static List<PriceRecord> Load(
            Dictionary<string, string> config,
            IEnumerable<PriceRecord> price = null)
        {
            var lastImportIndex = price
                .Where(p => p.Date.HasValue && p.Price.HasValue && !string.IsNullOrEmpty(p.Symbol) && !string.IsNullOrEmpty(p.Currency) && !string.IsNullOrEmpty(p.Provider))
                .GroupBy(p => (p.Symbol, p.Currency, p.Provider))
                .ToDictionary(
                    g => g.Key,
                    g => g.Max(p => p.Date.Value));

            var pairs = ReadConfiguration(config, "PRICE_PR_AV");
            var avProvider = new AlphaVantage("AV");

            var result = new List<PriceRecord>();
            foreach (var p in pairs)
            {
                if (!lastImportIndex.TryGetValue((p.symbol, p.currency, "AV"), out var lastImport))
                    lastImport = p.from.AddDays(-1);

                result.AddRange(avProvider.Load(p.symbol, p.currency, lastImport.AddDays(1), p.options));
            }
            return result;
        }



        private static IList<(string symbol, string currency, DateTime from, IList<string> options)> ReadConfiguration(
            Dictionary<string, string> config,
            string key)
        {
            if (!config.TryGetValue(key, out string value))
                throw new LedgerException($"Can't find '{key}' configuration.");            

            try
            {
                return ParseConfigurationValue(value);
            }
            catch (Exception ex)
            {
                throw new LedgerException($"Can't parse '{key}' configuration:\r\n{ex.Message}");
            }
        }

        internal static IList<(string symbol, string currency, DateTime from, IList<string> options)> ParseConfigurationValue(string value)
        {
            var result = new List<(string symbol, string currency, DateTime from, IList<string> options)>();
            if (!string.IsNullOrEmpty(value))
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in list)
                {
                    var match = Regex.Match(
                        s,
                        @"^\s*\(\s*([^\s\(\)/]+)\s*/\s*([^\s\(\)/]+)\s*:\s*(\d\d\d\d-\d\d-\d\d)\s*(:[^:\(\)]*)*\)\s*$",
                        RegexOptions.CultureInvariant);

                    if (!match.Success)
                        throw new LedgerException(s);

                    var optionsCount = (match.Groups.Count == 5)
                        ? match.Groups[4].Captures.Count
                        : 0;

                    var options = new string[optionsCount];
                    for (var i = 0; i < optionsCount; i++)
                    {
                        options[i] = match.Groups[4].Captures[i].Value.Substring(1).Trim();
                    }

                    result.Add((
                        match.Groups[1].Value,
                        match.Groups[2].Value,
                        DateTime.ParseExact(match.Groups[3].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                        options));
                }
            }
            return result;
        }
    }
}
