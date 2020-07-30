using SpreadsheetLedger.Core;
using System;
using System.Linq;
using Xunit;

namespace SpreadsheetLedger.CoreTests
{
    public class PriceProviderAlphaVantageTests
    {
        [Fact]
        public void FX_MONTHLY_DEMO_1()
        {
            var avProvider = new PriceProvider.AlphaVantage("AV");
            var actual = avProvider
                .Load("EUR", "USD", new DateTime(2020, 01, 01), new[] { "FX_MONTHLY", "DEMO" })
                .ToList();

            Assert.NotNull(actual);

            var price = actual.FirstOrDefault(p => p.Date == new DateTime(2020, 01, 31));

            Assert.NotNull(price);
            Assert.Equal("EUR", price.Symbol);
            Assert.Equal("USD", price.Currency);
            Assert.Equal("AV", price.Provider);
            Assert.True(price.Price.HasValue);
            Assert.Equal(11092, (int)(price.Price.Value * 10000));
        }

        [Fact]
        public void TIME_SERIES_MONTHLY_DEMO_1()
        {
            var avProvider = new PriceProvider.AlphaVantage("AV");
            var actual = avProvider
                .Load("IBM", "USD", new DateTime(2020, 01, 01), new[] { "TIME_SERIES_MONTHLY", "DEMO" })
                .ToList();

            Assert.NotNull(actual);

            var price = actual.FirstOrDefault(p => p.Date == new DateTime(2020, 01, 31));

            Assert.NotNull(price);
            Assert.Equal("IBM", price.Symbol);
            Assert.Equal("USD", price.Currency);
            Assert.Equal("AV", price.Provider);
            Assert.True(price.Price.HasValue);
            Assert.Equal(1437300, (int)(price.Price.Value * 10000));
        }
    }
}
