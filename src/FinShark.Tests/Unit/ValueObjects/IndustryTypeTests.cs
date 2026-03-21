using FinShark.Domain.ValueObjects;
using Xunit;

namespace FinShark.Tests.Unit.ValueObjects;

public class IndustryTypeTests
{
    [Theory]
    [InlineData("Real Estate", "Real Estate")]
    [InlineData("real estate", "Real Estate")]
    [InlineData("RealEstate", "Real Estate")]
    [InlineData("Technology", "Technology")]
    public void TryFrom_ShouldParseKnownIndustryNames(string input, string expectedValue)
    {
        var result = SectorType.TryFrom(input, out var industry);

        Assert.True(result);
        Assert.Equal(expectedValue, industry.Value);
    }

    [Fact]
    public void TryFrom_ShouldReturnFalseForUnknownIndustry()
    {
        var result = SectorType.TryFrom("Unknown Bucket", out var industry);

        Assert.False(result);
        Assert.Equal(SectorType.Other.Value, industry.Value);
    }
}
