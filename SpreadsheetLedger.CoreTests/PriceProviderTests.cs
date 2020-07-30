using SpreadsheetLedger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SpreadsheetLedger.CoreTests
{
    public class PriceProviderTests
    {
        [Fact]
        public void ParseConfigurationValueTest()
        {
            var input = "(USD/BYN:2016-07-01:FX_DAILY:DEMO)";
            var actual = PriceProvider.ParseConfigurationValue(input);

            Assert.Equal(1, actual.Count);
            Assert.Equal("USD", actual[0].symbol);
            Assert.Equal("BYN", actual[0].currency);
            Assert.Equal(new DateTime(2016, 7, 1), actual[0].from);
            Assert.Equal(2, actual[0].options.Count);
            Assert.Equal("FX_DAILY", actual[0].options[0]);
            Assert.Equal("DEMO", actual[0].options[1]);
        }

        [Fact]
        public void ParseConfigurationValueTest2()
        {
            var input = "(USD/BYN:2016-07-01:FX_DAILY:DEMO),\r\n( USD / BYN : 2016-07-01 : FX DAILY 23 : DEMO ),";
            var actual = PriceProvider.ParseConfigurationValue(input);

            Assert.Equal(2, actual.Count);
            Assert.Equal("USD", actual[1].symbol);
            Assert.Equal("BYN", actual[1].currency);
            Assert.Equal(new DateTime(2016, 7, 1), actual[1].from);
            Assert.Equal(2, actual[1].options.Count);
            Assert.Equal("FX DAILY 23", actual[1].options[0]);
            Assert.Equal("DEMO", actual[1].options[1]);
        }
    }
}
