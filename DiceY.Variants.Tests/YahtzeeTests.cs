using DiceY.Domain.Interfaces;
using DiceY.TestUtil;
using DiceY.Variants.Yahtzee;

namespace DiceY.Variants.Tests;

public sealed class YahtzeeTests
{
    private static YahtzeeEngine CreateRuleset(IRollService rng = null, YahtzeeConfig cfg = null) => new(rng ?? new FixedRollService([]), cfg ?? YahtzeeConfig.Default());

    private static YahtzeeState WithDice(YahtzeeState s, params int[] values) =>
        s with { Dice = DiceFactory.D6(values) };

    [Fact]
    public void Create_InitializesState()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        Assert.Equal(5, s.Dice.Count);
        Assert.All(s.Dice, d => Assert.Equal(6, d.Sides));
        Assert.Equal(0, s.RollCount);
        Assert.False(s.IsCompleted);
        Assert.Equal(0, s.TotalScore);
        Assert.True(s.Column.Map.ContainsKey(YahtzeeConstants.Ones));
        Assert.True(s.Column.Map.ContainsKey(YahtzeeConstants.Chance));
    }

    [Fact]
    public void Roll_RollsSelectedDice_AndIncrementsRollCount()
    {
        var rng = new FixedRollService([2, 4]);
        var rs = CreateRuleset(rng);
        var s = rs.Create();
        var s2 = rs.Reduce(s, new RollCommand(0b00101));
        Assert.Equal(2, s2.Dice[0].Value);
        Assert.Equal(6, s2.Dice[1].Value);
        Assert.Equal(4, s2.Dice[2].Value);
        Assert.Equal(1, s2.RollCount);
    }

    [Fact]
    public void Roll_WithZeroMask_DoesNothing()
    {
        var rng = new FixedRollService([3]);
        var rs = CreateRuleset(rng);
        var s = rs.Create();
        var s2 = rs.Reduce(s, new RollCommand(0));
        Assert.Equal(s, s2);
    }

    [Fact]
    public void Roll_WhenMaxRollsReached_IsIgnored()
    {
        var rng = new FixedRollService([3, 3, 3]);
        var rs = CreateRuleset(rng);
        var s = rs.Create() with { RollCount = YahtzeeConfig.Default().MaxRollsPerTurn };
        var s2 = rs.Reduce(s, new RollCommand(0b11111));
        Assert.Equal(s, s2);
    }

    [Fact]
    public void Roll_WithOutOfRangeMask_Throws()
    {
        var rs = CreateRuleset(new FixedRollService([1]));
        var s = rs.Create();
        Assert.Throws<ArgumentOutOfRangeException>(() => rs.Reduce(s, new RollCommand(1 << 5)));
    }

    [Fact]
    public void Score_SetsCategoryAndResetsRollCount()
    {
        var rs = CreateRuleset();
        var s = rs.Create() with { RollCount = 2 };
        s = WithDice(s, 1, 1, 1, 4, 6);
        var s2 = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Ones));
        var cat = s2.Column.Map[YahtzeeConstants.Ones];
        Assert.Equal(3, cat.Score);
        Assert.Equal(0, s2.RollCount);
    }

    [Fact]
    public void Score_CannotScoreSameCategoryTwice()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 3, 4);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Ones));
        var after = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Ones));
        Assert.Same(s, after);
    }

    [Fact]
    public void TotalScore_AppliesTopBonusAtThreshold()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 5, 6);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Ones));
        s = WithDice(s, 2, 2, 2, 1, 4);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Twos));
        s = WithDice(s, 3, 3, 3, 1, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Threes));
        s = WithDice(s, 4, 4, 4, 1, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Fours));
        s = WithDice(s, 5, 5, 5, 1, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Fives));
        s = WithDice(s, 6, 6, 6, 1, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Sixes));
        Assert.Equal(63 + YahtzeeConfig.Default().TopSectionBonus, s.TotalScore);
    }

    [Fact]
    public void IsCompleted_BlocksFurtherActions()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 1, 1);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Ones));
        s = WithDice(s, 2, 2, 2, 2, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Twos));
        s = WithDice(s, 3, 3, 3, 3, 3);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Threes));
        s = WithDice(s, 4, 4, 4, 4, 4);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Fours));
        s = WithDice(s, 5, 5, 5, 5, 5);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Fives));
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Sixes));
        s = WithDice(s, 3, 3, 3, 5, 6);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.ThreeOfAKind));
        s = WithDice(s, 4, 4, 4, 4, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.FourOfAKind));
        s = WithDice(s, 3, 3, 3, 2, 2);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.FullHouse));
        s = WithDice(s, 1, 2, 3, 4, 6);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.SmallStraight));
        s = WithDice(s, 2, 3, 4, 5, 6);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.LargeStraight));
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Yahtzee));
        s = WithDice(s, 1, 2, 3, 4, 5);
        s = rs.Reduce(s, new ScoreCommand(YahtzeeConstants.Chance));
        Assert.True(s.IsCompleted);
        var afterRoll = rs.Reduce(s, new RollCommand(0b11111));
        Assert.Same(s, afterRoll);
    }
}