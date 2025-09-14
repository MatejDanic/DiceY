using DiceY.TestUtil;
using DiceY.Variants.Shared.Rules;
using static System.Formats.Asn1.AsnWriter;

namespace DiceY.Variants.Tests.Shared.Rules;

public sealed class SumTests
{
    [Theory]
    [InlineData(new[] { 1, 2, 3, 4, 5 }, 15)]
    [InlineData(new[] { 6, 6, 6, 6, 6 }, 30)]
    [InlineData(new[] { 2, 3, 6 }, 11)]
    [InlineData(new[] { 1 }, 1)]
    public void ComputesTotal_D6(int[] values, int expected)
    {
        var rule = new Sum();
        var score = rule.GetScore(DiceFactory.D6(values));
        Assert.Equal(expected, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new Sum();
        var score = rule.GetScore(DiceFactory.D(8, 7, 7, 1, 8));
        Assert.Equal(23, score);
    }

    [Fact]
    public void ReturnsZeroWithEmptyDice()
    {
        var rule = new Sum();
        var score = rule.GetScore(DiceFactory.D6());
        Assert.Equal(0, score);
    }

    [Fact]
    public void GetScore_WhenDiceIsNull_ReturnsZero()
    {
        var rule = new Sum();
        var score = rule.GetScore(null!);
        Assert.Equal(0, score);
    }
}
