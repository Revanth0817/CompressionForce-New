using CompressionForce.SignalRules.Enums;
using CompressionForce.SignalRules.Rules;
using Xunit;

namespace CompressionForce.SignalRules.Tests
{
    public sealed class SignalTypeRulesTests
    {
        [Theory]
        [InlineData(SignalDataType.Int16, 1)]
        [InlineData(SignalDataType.Int32, 2)]
        [InlineData(SignalDataType.Float, 2)]
        [InlineData(SignalDataType.Double, 4)]
        public void ExpectedRegisterLength_IsCorrect(
            SignalDataType dataType,
            int expected)
        {
            var result = SignalTypeRules.ExpectedRegisterLength(dataType);

            Assert.NotNull(result);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Bool_ReturnsNullLength()
        {
            var result = SignalTypeRules.ExpectedRegisterLength(SignalDataType.Bool);

            Assert.Null(result);
        }
    }
}
