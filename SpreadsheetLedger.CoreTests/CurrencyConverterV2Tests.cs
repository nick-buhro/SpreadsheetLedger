using SpreadsheetLedger.Core;
using Xunit;

namespace SpreadsheetLedger.CoreTests
{
    public sealed class CurrencyConverterV2Tests
    {
        [Fact]
        public void ParseRuleTest0()
        {
            var input = @"";
            var result = CurrencyConverterV2.ParseRule(input);
            Assert.Empty(result);
        }

        [Fact]
        public void ParseRuleTest1()
        {
            var input = @"* (EUR/USD:AV)";
            var result = CurrencyConverterV2.ParseRule(input);

            Assert.Single(result);
            Assert.Equal('*', result[0].op);
            Assert.Equal("EUR/USD:AV", result[0].source);
        }

        [Fact]
        public void ParseRuleTest2()
        {
            var input = @"* (MSFT/USD:AV) / ( EUR / USD : AV )";    // MSFT => USD => EUR
            var result = CurrencyConverterV2.ParseRule(input);

            Assert.Equal(2, result.Length);
            Assert.Equal('*', result[0].op);
            Assert.Equal("MSFT/USD:AV", result[0].source);
            Assert.Equal('/', result[1].op);
            Assert.Equal("EUR/USD:AV", result[1].source);
        }
    }
}
