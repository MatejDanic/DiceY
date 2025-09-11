using Yamb.Domain.Rules;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Rules;

public sealed class FaceSumTests
{
    [Theory]
    [InlineData(3, new[] { 1, 3, 3, 5, 6 }, 6)]
    [InlineData(1, new[] { 1, 1, 1, 1, 1 }, 5)]
    [InlineData(6, new[] { 2, 2, 2, 2, 2 }, 0)]
    public void ComputesSumForFace_D6(int face, int[] values, int expected)
    {
        var rule = new FaceSum(face);
        var ok = rule.TryScore(DiceFactory.D6(values), out var score);
        Assert.True(ok);
        Assert.Equal(expected, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new FaceSum(7);
        var ok = rule.TryScore(DiceFactory.D(8, 7, 7, 1, 8), out var score);
        Assert.True(ok);
        Assert.Equal(14, score);
    }

    [Fact]
    public void ReturnsZeroWhenNoMatches()
    {
        var rule = new FaceSum(5);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4, 6), out var score);
        Assert.True(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalseWithEmptyDice()
    {
        var rule = new FaceSum(4);
        var ok = rule.TryScore(DiceFactory.D6(), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalseWhenDiceIsNull()
    {
        var rule = new FaceSum(2);
        var ok = rule.TryScore(null, out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void FaceGreaterThanSides_ReturnsZero()
    {
        var rule = new FaceSum(7);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4, 5, 6), out var score);
        Assert.True(ok);
        Assert.Equal(0, score);
    }
}