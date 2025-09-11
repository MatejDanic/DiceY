using Yamb.Domain.Rules;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Rules;

public sealed class NOfAKindTests
{
    [Fact]
    public void ReturnsFalse_WhenNoGroupMeetsN()
    {
        var rule = new NOfAKind(3);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 2, 5, 6), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void Scores_NTimesFace_OnMatch()
    {
        var rule = new NOfAKind(3);
        var ok = rule.TryScore(DiceFactory.D6(3, 3, 3, 2, 5), out var score);
        Assert.True(ok);
        Assert.Equal(9, score);
    }

    [Fact]
    public void PicksHighestFace_WhenMultipleQualify()
    {
        var rule = new NOfAKind(3);
        var ok = rule.TryScore(DiceFactory.D6(2, 2, 2, 5, 5, 5), out var score);
        Assert.True(ok);
        Assert.Equal(15, score);
    }

    [Fact]
    public void AppliesBonus_WhenNoFixedScore()
    {
        var rule = new NOfAKind(4, bonus: 10);
        var ok = rule.TryScore(DiceFactory.D6(4, 4, 4, 4, 1), out var score);
        Assert.True(ok);
        Assert.Equal(4 * 4 + 10, score);
    }

    [Fact]
    public void UsesFixedScore_WhenProvided()
    {
        var rule = new NOfAKind(5, bonus: 999, fixedScore: 50);
        var ok = rule.TryScore(DiceFactory.D6(6, 6, 6, 6, 6), out var score);
        Assert.True(ok);
        Assert.Equal(50, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new NOfAKind(2);
        var ok = rule.TryScore(DiceFactory.D(8, 7, 7, 2, 8), out var score);
        Assert.True(ok);
        Assert.Equal(2 * 7, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceIsNull()
    {
        var rule = new NOfAKind(3);
        var ok = rule.TryScore(null, out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceCountLessThanN()
    {
        var rule = new NOfAKind(4);
        var ok = rule.TryScore(DiceFactory.D6(3, 3, 3), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void FiveOfAKind_WithNThree_ScoresThreeTimesFace()
    {
        var rule = new NOfAKind(3);
        var ok = rule.TryScore(DiceFactory.D6(5, 5, 5, 5, 5), out var score);
        Assert.True(ok);
        Assert.Equal(3 * 5, score);
    }

    [Fact]
    public void MultiplePairs_WithNTwo_PicksHighestFace()
    {
        var rule = new NOfAKind(2);
        var ok = rule.TryScore(DiceFactory.D6(2, 2, 6, 6, 6), out var score);
        Assert.True(ok);
        Assert.Equal(2 * 6, score);
    }
}