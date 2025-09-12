using DiceY.Variants.Shared.Rules;
using DiceY.TestUtil;

namespace DiceY.Variants.Tests.Shared.Rules;

public sealed class StraightTests
{
    [Fact]
    public void FixedScore_SucceedsOnRun()
    {
        var rule = new Straight(n: 4, fixedScore: 30);
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4, 6));
        Assert.Equal(30, score);
    }

    [Fact]
    public void FixedScore_IgnoresDuplicates()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var score = rule.GetScore(DiceFactory.D6(1, 1, 2, 3, 4, 5));
        Assert.Equal(40, score);
    }

    [Fact]
    public void FixedScore_FailsWithoutRun()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var score = rule.GetScore(DiceFactory.D6(1, 2, 2, 4, 6));
        Assert.Equal(0, score);
    }

    [Fact]
    public void Bonus_Applied_WhenNoFixedScore()
    {
        var rule = new Straight(n: 5, bonus: 7, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4, 5));
        Assert.Equal(22, score);
    }

    [Fact]
    public void UsesSumOfAllDice_WhenNoFixedScore_EvenWithDuplicates()
    {
        var rule = new Straight(n: 5, bonus: 0, fixedScore: 0);
        var score = rule.GetScore(DiceFactory.D6(1, 1, 2, 3, 4, 5));
        Assert.Equal(16, score);
    }

    [Fact]
    public void WorksWithDifferentSides()
    {
        var rule = new Straight(n: 5, fixedScore: 45);
        var score = rule.GetScore(DiceFactory.D(8, 2, 3, 4, 5, 6, 8));
        Assert.Equal(45, score);
    }

    [Fact]
    public void GetScore_WhenDiceIsNull_Throws()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        Assert.Throws<ArgumentNullException>(() => rule.GetScore(null!));
    }

    [Fact]
    public void ReturnsZero_WhenDiceIsEmpty()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var score = rule.GetScore(DiceFactory.D6());
        Assert.Equal(0, score);
    }

    [Fact]
    public void ReturnsZero_WhenDiceCountLessThanN()
    {
        var rule = new Straight(n: 5, fixedScore: 40);
        var score = rule.GetScore(DiceFactory.D6(1, 2, 3, 4));
        Assert.Equal(0, score);
    }
}
