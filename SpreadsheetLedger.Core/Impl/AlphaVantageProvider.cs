using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SpreadsheetLedger.Core.Impl
{
    public sealed class AlphaVantageProvider : IPriceProvider
    {
        public const string ProviderCode = "AV";
        private const string Path = "https://www.alphavantage.co/query";


        public Task<IList<PriceRecord>> GetPrices(string symbol, string currency, DateTime from, IList<string> options, CancellationToken ct)
        {
            if ((options == null) || (options.Count != 2))
                throw new LedgerException("AlphaVantage: 2 options should be declared.");

            string url;
            switch (options[0])
            {
                case "FX_DAILY":
                case "FX_MONTHLY":
                    url = $"{Path}?function={options[0]}&from_symbol={symbol}&to_symbol={currency}&outputsize=full&datatype=csv&apikey={options[1]}";
                    break;
                case "TIME_SERIES_DAILY":
                case "TIME_SERIES_MONTHLY":
                    if (currency != "USD")
                        throw new LedgerException($"AlphaVantage '{options[0]}' method available only for USD currency.");
                    url = $"{Path}?function={options[0]}&symbol={symbol}&outputsize=full&datatype=csv&apikey={options[1]}";
                    break;
                default:
                    throw new LedgerException($"AlphaVantage '{options[0]}' method is not supprted yet. Available: FX_DAILY, FX_MONTHLY, TIME_SERIES_DAILY, TIME_SERIES_MONTHLY.");
            }

            return GetPrices(url, symbol, currency, from, ct);
        }


        private static async Task<IList<PriceRecord>> GetPrices(string url, string symbol, string currency, DateTime from, CancellationToken ct)
        {
            try
            {
                var csv = await GetCSV(url, "timestamp,open,high,low,close", ct: ct)
                    .ConfigureAwait(false);
                  
                var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var result = new List<PriceRecord>();
                for (var i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    if (values.Length < 5)
                        throw new LedgerException("Unexpected server response [2].");

                    var date = DateTime.Parse(values[0]);
                    if ((date < from) || (date >= DateTime.Today))
                        continue;

                    result.Add(new PriceRecord
                    {
                        Date = date,
                        Symbol = symbol,
                        Price = decimal.Parse(values[4], CultureInfo.InvariantCulture),
                        Currency = currency,
                        Provider = ProviderCode
                    });
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new LedgerException($"AlphaVantage price import error ({url}).", ex);
            }
        }

        private static async Task<string> GetCSV(string url, string csvStartsWith, int attemptCount = 3, int attemptPause = 60, CancellationToken ct = default)
        {            
            var tryCounter = attemptCount;
            for (; ; )
            {
                var text = await GetResponseText(url)
                    .ConfigureAwait(false);

                if (text.StartsWith(csvStartsWith))
                    return text;
                    
                if (--tryCounter <= 0)
                    throw new LedgerException(text);

                ct.ThrowIfCancellationRequested();

                await Task
                    .Delay(attemptPause * 1000)
                    .ConfigureAwait(false);                
            }
            throw new InvalidOperationException();
        }

        private static async Task<string> GetResponseText(string url)
        {
            var request = WebRequest.Create(url);                        
            using (var response = await request.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
