using SpreadsheetLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;

namespace SpreadsheetLedger.Core
{
    partial class PriceProvider
    {        
        internal class AlphaVantage
        {
            private readonly string _provider;

            public AlphaVantage(string provider)
            {
                _provider = provider;
            }

            public IEnumerable<PriceRecord> Load(string symbol, string currency, DateTime from, IList<string> options)
            {
                if ((options == null) || (options.Count != 2))
                    throw new LedgerException("AlphaVantage: 2 options should be declared.");

                string url;
                switch (options[0])                  
                {
                    case "FX_DAILY":
                    case "FX_MONTHLY":
                        url = "https://www.alphavantage.co/query?function=" + options[0] + "&from_symbol=" + symbol + "&to_symbol=" + currency + "&outputsize=full&datatype=csv&apikey=" + options[1];
                        break;
                    case "TIME_SERIES_DAILY":
                    case "TIME_SERIES_MONTHLY":
                        if (currency != "USD")
                            throw new LedgerException($"AlphaVantage '{options[0]}' method available only for USD currency.");
                        url = "https://www.alphavantage.co/query?function=" + options[0] + "&symbol=" + symbol + "&outputsize=full&datatype=csv&apikey=" + options[1];
                        break;
                    default:
                        throw new LedgerException($"AlphaVantage '{options[0]}' method is not supprted yet. Available: FX_DAILY, FX_MONTHLY, TIME_SERIES_DAILY, TIME_SERIES_MONTHLY");
                }

                //try
                {
                    string csv = null;
                    var tryCounter = 3;
                    while (tryCounter > 0)
                    {                        
                        csv = GetResponseText(url);
                        if (csv.StartsWith("timestamp,open,high,low,close"))
                            break;

                        if (--tryCounter == 0)
                            throw new LedgerException(csv);

                        Thread.Sleep(60000);
                    }
                                        
                    var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 1; i < lines.Length; i++)
                    {
                        var values = lines[i].Split(',');
                        if (values.Length < 5)
                            throw new LedgerException("Unexpected server response [2].");

                        var date = DateTime.Parse(values[0]);
                        if ((date < from) || (date >= DateTime.Today))
                            continue;

                        yield return new PriceRecord
                        {
                            Date = date,
                            Symbol = symbol,
                            Price = Decimal.Parse(values[4], CultureInfo.InvariantCulture),
                            Currency = currency,
                            Provider = _provider
                        };
                    }                    
                }
                //catch(Exception ex)
                //{
                //    throw new LedgerException($"AlphaVantage price import error ({url}).", ex);
                //}
            }

            private static string GetResponseText(string url)
            {
                var request = WebRequest.Create(url);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var sr = new StreamReader(stream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
