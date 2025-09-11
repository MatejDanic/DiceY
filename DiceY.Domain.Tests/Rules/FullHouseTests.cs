using Yamb.Domain.Rules;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Rules;

public sealed class FullHouseTests
{
    [Fact]
    public void NotFullHouse_ReturnsFalse()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(3, 3, 3, 3, 2), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoresTriplePlusPair_WithBonus()
    {
        var rule = new FullHouse(bonus: 5, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(3, 3, 3, 2, 2), out var score);
        Assert.True(ok);
        Assert.Equal(3 * 3 + 2 * 2 + 5, score);
    }

    [Fact]
    public void UsesFixedScore_WhenProvided()
    {
        var rule = new FullHouse(bonus: 999, fixedScore: 25);
        var ok = rule.TryScore(DiceFactory.D6(5, 5, 5, 2, 2), out var score);
        Assert.True(ok);
        Assert.Equal(25, score);
    }

    [Fact]
    public void PicksBestFaces_WhenMultipleOptions()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(6, 6, 6, 4, 4, 3, 3), out var score);
        Assert.True(ok);
        Assert.Equal(3 * 6 + 2 * 4, score);
    }

    [Fact]
    public void FiveOfAKind_IsNotFullHouse()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(6, 6, 6, 6, 6), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D(8, 7, 7, 7, 2, 2), out var score);
        Assert.True(ok);
        Assert.Equal(3 * 7 + 2 * 2, score);
    }

    [Fact]
    public void TooFewDice_ReturnsFalse()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var ok = rule.TryScore(DiceFactory.D6(3, 3, 2, 2), out var score);
        Assert.False(ok);
        Assert.Equal(0, score);
    }
}