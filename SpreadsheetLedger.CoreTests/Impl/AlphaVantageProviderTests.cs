using System;
using System.Linq;
using Xunit;

namespace SpreadsheetLedger.Core.Impl
{
    public class AlphaVantageProviderTests
    {
        [Fact]
        public async void FX_MONTHLY_DEMO_1()
        {
            var avProvider = new AlphaVantageProvider();
            var actual = await avProvider.GetPrices("EUR", "USD", new DateTime(2020, 01, 01), new[] { "FX_MONTHLY", "DEMO" }, default);

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
        public async void TIME_SERIES_MONTHLY_DEMO_1()
        {
            var avProvider = new AlphaVantageProvider();
            var actual = await avProvider.GetPrices("IBM", "USD", new DateTime(2020, 01, 01), new[] { "TIME_SERIES_MONTHLY", "DEMO" }, default);

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
