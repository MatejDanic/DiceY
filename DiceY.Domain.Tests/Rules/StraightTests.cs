using Yamb.Domain.Rules;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Rules;

public sealed class StraightTests
{
    [Fact]
    public void FixedScore_SucceedsOnRun()
    {
        var rule = new Straight(n: 4, fixedScore: 30);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4, 6), out var score);
        Assert.True(ok);
        Assert.Equal(30, score);
    }

    [Fact]
    public void FixedScore_IgnoresDuplicates()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var ok = rule.TryScore(DiceFactory.D6(1, 1, 2, 3, 4, 5), out var score);
        Assert.True(ok);
        Assert.Equal(40, score);
    }

    [Fact]
    public void FixedScore_FailsWithoutRun()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 2, 4, 6), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void Bonus_Applied_WhenNoFixedScore()
    {
        var rule = new Straight(n: 5, bonus: 7, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4, 5), out var score);
        Assert.True(ok);
        Assert.Equal(15 + 7, score);
    }

    [Fact]
    public void UsesSumOfAllDice_WhenNoFixedScore_EvenWithDuplicates()
    {
        var rule = new Straight(n: 5, bonus: 0, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(1, 1, 2, 3, 4, 5), out var score);
        Assert.True(ok);
        Assert.Equal(1 + 1 + 2 + 3 + 4 + 5, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new Straight(n: 5, fixedScore: 45);
        var ok = rule.TryScore(DiceFactory.D(8, 2, 3, 4, 5, 6, 8), out var score);
        Assert.True(ok);
        Assert.Equal(45, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceIsNull()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var ok = rule.TryScore(null, out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceIsEmpty()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var ok = rule.TryScore(DiceFactory.D6(), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsFalse_WhenDiceCountLessThanN()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var ok = rule.TryScore(DiceFactory.D6(1, 2, 3, 4), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }
}