using DiceY.TestUtil;
using DiceY.Variants.Shared.Rules;
using static System.Formats.Asn1.AsnWriter;

namespace DiceY.Variants.Tests.Shared.Rules;

public sealed class FaceSumTests
{
    [Theory]
    [InlineData(3, new[] { 1, 3, 3, 5, 6 }, 6)]
    [InlineData(1, new[] { 1, 1, 1, 1, 1 }, 5)]
    [InlineData(6, new[] { 2, 2, 2, 2, 2 }, 0)]
    public void ComputesSumForFace_D6(int face, int[] values, int expected)
    {
        var rule = new FaceSum(face);
        var score = rule.GetScore(DiceFactory.D6(values));
        Assert.Equal(expected, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new FaceSum(7);
        var score = rule.GetScore(DiceFactory.D(8, 7, 7, 1, 8));
        Assert.Equal(14, score);
    }

    [Fact]
    public void ReturnsZeroWhenNoMatches()
    {
        var rule = new FaceSum(5);
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4, 6));
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsZeroWithEmptyDice()
    {
        var rule = new FaceSum(4);
        var score = rule.GetScore(DiceFactory.D6());
        Assert.Equal(0, score);
    }

    [Fact]
    public void GetScore_WhenDiceIsNull_ReturnsZero()
    {
        var rule = new FaceSum(2);
        var score = rule.GetScore(null!);
        Assert.Equal(0, score);
    }

    [Fact]
    public void FaceGreaterThanSides_ReturnsZero()
    {
        var rule = new FaceSum(7);
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4, 5, 6));
        Assert.Equal(0, score);
    }
}
