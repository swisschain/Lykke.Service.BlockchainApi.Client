using Xunit;

namespace Lykke.Service.BlockchainApi.Contract.Tests
{
    public class ConversionsTests
    {
        [Fact]
        public void Test_coins_from_contract_conversion()
        {
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("", 1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("100000000000000000000000000000", 1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("80000000000000000000000000000", 1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("0", -1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("0", 29));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("-1", 1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("asd", 1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsFromContract("0.1", 1));

            Assert.Equal(0m, Conversions.CoinsFromContract("0", 0));
            Assert.Equal(0m, Conversions.CoinsFromContract("0", 28));

            Assert.Equal(1m, Conversions.CoinsFromContract("1", 0));
            Assert.Equal(1e-28m, Conversions.CoinsFromContract("1", 28));

            Assert.Equal(7e+28m, Conversions.CoinsFromContract("70000000000000000000000000000", 0));

            Assert.Equal(100000000000000000000000000.1m, Conversions.CoinsFromContract("1000000000000000000000000001", 1));
            Assert.Equal(0.1000000000000000000000000001m, Conversions.CoinsFromContract("1000000000000000000000000001", 28));
        }

        [Fact]
        public void Test_coins_to_contract_conversion()
        {
            Assert.Throws<ConversionException>(() => Conversions.CoinsToContract(-1, 1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsToContract(0, -1));
            Assert.Throws<ConversionException>(() => Conversions.CoinsToContract(0, 29));
            Assert.Throws<ConversionException>(() => Conversions.CoinsToContract(1e+28m, 1));

            Assert.Equal("0", Conversions.CoinsToContract(0, 0));
            Assert.Equal("0", Conversions.CoinsToContract(0, 28));

            Assert.Equal("1", Conversions.CoinsToContract(1, 0));
            Assert.Equal("1", Conversions.CoinsToContract(1e-28m, 28));
            Assert.Equal("0", Conversions.CoinsToContract(1e-28m, 27));

            Assert.Equal("70000000000000000000000000000", Conversions.CoinsToContract(7e+28m, 0));

            Assert.Equal("1000000000000000000000000001", Conversions.CoinsToContract(100000000000000000000000000.1m, 1));
            Assert.Equal("1000000000000000000000000001", Conversions.CoinsToContract(0.1000000000000000000000000001m, 28));
        }
    }
}
