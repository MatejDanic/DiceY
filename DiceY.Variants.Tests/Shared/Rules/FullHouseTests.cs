using DiceY.Variants.Shared.Rules;
using DiceY.TestUtil;

namespace DiceY.Variants.Tests.Shared.Rules;

public sealed class FullHouseTests
{
    [Fact]
    public void NotFullHouse_ReturnsZero()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(3, 3, 3, 3, 2));
        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoresTriplePlusPair_WithBonus()
    {
        var rule = new FullHouse(bonus: 5, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(3, 3, 3, 2, 2));
        Assert.Equal(3 * 3 + 2 * 2 + 5, score);
    }

    [Fact]
    public void UsesFixedScore_WhenProvided()
    {
        var rule = new FullHouse(bonus: 999, fixedScore: 25);
        var score = rule.GetScore(DiceFactory.D6(5, 5, 5, 2, 2));
        Assert.Equal(25, score);
    }

    [Fact]
    public void PicksBestFaces_WhenMultipleOptions()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(6, 6, 6, 4, 4, 3, 3));
        Assert.Equal(3 * 6 + 2 * 4, score);
    }

    [Fact]
    public void FiveOfAKind_ReturnsZero()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(6, 6, 6, 6, 6));
        Assert.Equal(0, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D(8, 7, 7, 7, 2, 2));
        Assert.Equal(3 * 7 + 2 * 2, score);
    }

    [Fact]
    public void TooFewDice_ReturnsZero()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(3, 3, 2, 2));
        Assert.Equal(0, score);
    }

    [Fact]
    public void GetScore_WhenDiceIsNull_Throws()
    {
        var rule = new FullHouse(bonus: 0, fixedScore: 0);
        Assert.Throws<ArgumentNullException>(() => rule.GetScore(null!));
    }
}
