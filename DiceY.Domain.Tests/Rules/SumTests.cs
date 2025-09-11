using Yamb.Domain.Rules;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Rules;

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
        var ok = rule.TryScore(DiceFactory.D6(values), out var score);
        Assert.True(ok);
        Assert.Equal(expected, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new Sum();
        var ok = rule.TryScore(DiceFactory.D(8, 7, 7, 1, 8), out var score);
        Assert.True(ok);
        Assert.Equal(23, score);
    }

    [Fact]
    public void ReturnsFalseWithEmptyDice()
    {
        var rule = new Sum();
        var ok = rule.TryScore(DiceFactory.D6(), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalseWhenDiceIsNull()
    {
        var rule = new Sum();
        var ok = rule.TryScore(null, out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }
}