using System;
using Xunit;

namespace SpreadsheetLedger.Core.Impl
{
    public sealed class PriceImportStrategyTests
    {
        [Fact]
        public void ParseConfigurationTest()
        {
            var inputKey = "IMPORT_PR:AV:USD/BYN";
            var inputValue = "2016-07-01:FX_DAILY:DEMO";
            
            var actual = PriceImportStrategy.ParseConfiguration(inputKey, inputValue);

            Assert.Equal("AV", actual.provider);
            Assert.Equal("USD", actual.symbol);
            Assert.Equal("BYN", actual.currency);
            Assert.Equal(new DateTime(2016, 07, 01), actual.from);
            Assert.Equal(2, actual.options.Count);
            Assert.Equal("FX_DAILY", actual.options[0]);
            Assert.Equal("DEMO", actual.options[1]);
        }
    }
}
