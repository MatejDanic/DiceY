using Yamb.Domain.Rules;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Rules;

public sealed class PatternTests
{
    private static Pattern StraightRule() => new (
        new Dictionary<ISet<int>, int>
        {
            { new HashSet<int> { 1, 2, 3, 4, 5 }, 35 },
            { new HashSet<int> { 2, 3, 4, 5, 6 }, 45 }
        });

    [Fact]
    public void Matches_SmallStraight()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4, 5), out var score);
        Assert.True(ok);
        Assert.Equal(35, score);
    }

    [Fact]
    public void Matches_LargeStraight()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D6(2, 3, 4, 5, 6), out var score);
        Assert.True(ok);
        Assert.Equal(45, score);
    }

    [Fact]
    public void PicksBest_WhenBothPatternsPresent()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4, 5, 6), out var score);
        Assert.True(ok);
        Assert.Equal(45, score);
    }

    [Fact]
    public void IgnoresDuplicates_InDicePool()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D6(1, 1, 2, 3, 4, 5), out var score);
        Assert.True(ok);
        Assert.Equal(35, score);
    }

    [Fact]
    public void Fails_WhenNoPatternMatches()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D6(1, 1, 3, 4, 6), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D(8, 2, 3, 4, 5, 6, 8), out var score);
        Assert.True(ok);
        Assert.Equal(45, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceIsNull()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(null, out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceIsEmpty()
    {
        var rule = StraightRule();
        var ok = rule.TryScore(DiceFactory.D6(), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }
}