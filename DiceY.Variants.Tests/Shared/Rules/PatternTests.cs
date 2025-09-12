using DiceY.Variants.Shared.Rules;
using DiceY.TestUtil;

namespace DiceY.Variants.Tests.Shared.Rules;

public sealed class PatternTests
{
    private static Pattern StraightRule() => new(
        new Dictionary<IReadOnlySet<int>, int>
        {
            { new HashSet<int> { 1, 2, 3, 4, 5 }, 35 },
            { new HashSet<int> { 2, 3, 4, 5, 6 }, 45 }
        });

    [Fact]
    public void Matches_SmallStraight()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4, 5));
        Assert.Equal(35, score);
    }

    [Fact]
    public void Matches_LargeStraight()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D6(2, 3, 4, 5, 6));
        Assert.Equal(45, score);
    }

    [Fact]
    public void PicksBest_WhenBothPatternsPresent()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4, 5, 6));
        Assert.Equal(45, score);
    }

    [Fact]
    public void IgnoresDuplicates_InDicePool()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D6(1, 1, 2, 3, 4, 5));
        Assert.Equal(35, score);
    }

    [Fact]
    public void ReturnsZero_WhenNoPatternMatches()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D6(1, 1, 3, 4, 6));
        Assert.Equal(0, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D(8, 2, 3, 4, 5, 6, 8));
        Assert.Equal(45, score);
    }

    [Fact]
    public void GetScore_WhenDiceIsNull_Throws()
    {
        var rule = StraightRule();
        Assert.Throws<ArgumentNullException>(() => rule.GetScore(null!));
    }

    [Fact]
    public void ReturnsZero_WhenDiceIsEmpty()
    {
        var rule = StraightRule();
        var score = rule.GetScore(DiceFactory.D6());
        Assert.Equal(0, score);
    }
}
