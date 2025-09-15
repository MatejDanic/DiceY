using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.Domain.Interfaces;
using DiceY.TestUtil;
using DiceY.Variants.Shared.Commands;
using DiceY.Variants.Yahtzee;

namespace DiceY.Variants.Tests.Yahtzee;

public sealed class YahtzeeTests
{
    private static YahtzeeEngine CreateEngine(IRollService? rng = null, GameDefinition? def = null)
        => new(rng ?? new FixedRollService([]), def);

    private static YahtzeeState WithDice(YahtzeeState s, params int[] values)
        => new([.. DiceFactory.D6(values)], s.Column, s.RollCount);

    private static ColumnKey MainColumn(YahtzeeEngine rs)
        => rs.Definition.ColumnDefinitions[0].Key;

    private static CategoryKey Key(YahtzeeEngine rs, string value)
        => rs.Definition.CategoryDefinitions.First(cd => cd.Key.Value == value).Key;

    [Fact]
    public void Create_InitializesState()
    {
        var rs = CreateEngine(def: YahtzeeConfig.Build());
        var s = rs.Create();
        Assert.Equal(5, s.Dice.Count);
        Assert.All(s.Dice, d => Assert.InRange(d.Value, 1, 6));
        Assert.Equal(0, s.RollCount);
        var col = s.Columns.Single();
        Assert.False(col.IsCompleted);
        Assert.Equal(0, col.Score);
        var keys = col.Categories.Select(c => c.Key.Value).ToArray();
        Assert.Contains("ones", keys);
        Assert.Contains("chance", keys);
        Assert.Equal(13, keys.Length);
    }

    [Fact]
    public void Roll_RollsUnmaskedDice_AndIncrementsRollCount()
    {
        var rng = new FixedRollService([2, 4]);
        var rs = CreateEngine(rng, YahtzeeConfig.Build());
        var s = rs.Create();
        var s2 = rs.Reduce(s, new Roll(0b00101));
        Assert.Equal(s.Dice[0].Value, s2.Dice[0].Value);
        Assert.Equal(2, s2.Dice[1].Value);
        Assert.Equal(s.Dice[2].Value, s2.Dice[2].Value);
        Assert.Equal(4, s2.Dice[3].Value);
        Assert.Equal(6, s2.Dice[4].Value);
        Assert.Equal(1, s2.RollCount);
    }

    [Fact]
    public void Roll_WithZeroMask_RollsAllDice()
    {
        var rng = new FixedRollService([1, 1, 1, 1, 1]);
        var rs = CreateEngine(rng, YahtzeeConfig.Build());
        var s = rs.Create();
        var s2 = rs.Reduce(s, new Roll(0));
        Assert.All(s2.Dice, d => Assert.Equal(1, d.Value));
        Assert.Equal(1, s2.RollCount);
    }

    [Fact]
    public void Roll_WhenMaxRollsReached_Throws()
    {
        var rs = CreateEngine(def: YahtzeeConfig.Build());
        var state = rs.Create();
        var newState = new YahtzeeState(diceArray: state.DiceArray, column: state.Column, rollCount: rs.Definition.MaxRollsPerTurn);
        Assert.Throws<InvalidOperationException>(() => rs.Reduce(newState, new Roll(0b11111)));
    }

    [Fact]
    public void Score_SetsCategory_AndResetsRollCount()
    {
        var rs = CreateEngine(def: YahtzeeConfig.Build());
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 4, 6);
        s = new YahtzeeState(s.DiceArray, s.Column, rollCount: 2);
        var s2 = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "ones")));
        var col = s2.Columns.Single();
        var cat = col.Categories.First(c => c.Key.Value == "ones");
        Assert.Equal(3, cat.Score);
        Assert.Equal(0, s2.RollCount);
    }

    [Fact]
    public void Score_CannotScoreSameCategoryTwice_Throws()
    {
        var rs = CreateEngine(def: YahtzeeConfig.Build());
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 3, 4);
        var after = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "ones")));
        Assert.ThrowsAny<Exception>(() => rs.Reduce(after, new Fill(MainColumn(rs), Key(rs, "ones"))));
    }

    [Fact]
    public void TotalScore_AppliesTopBonusAtThreshold()
    {
        var rs = CreateEngine(def: YahtzeeConfig.Build());
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 5, 6);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "ones")));
        s = WithDice(s, 2, 2, 2, 1, 4);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "twos")));
        s = WithDice(s, 3, 3, 3, 1, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "threes")));
        s = WithDice(s, 4, 4, 4, 1, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "fours")));
        s = WithDice(s, 5, 5, 5, 1, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "fives")));
        s = WithDice(s, 6, 6, 6, 1, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "sixes")));
        var col = s.Columns.Single();
        var expected = 63 + 35;
        Assert.Equal(expected, col.Score);
    }

    [Fact]
    public void CompletingAllCategories_MarksColumnCompleted()
    {
        var rs = CreateEngine(def: YahtzeeConfig.Build());
        var s = rs.Create();

        s = WithDice(s, 1, 1, 1, 1, 1);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "ones")));
        s = WithDice(s, 2, 2, 2, 2, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "twos")));
        s = WithDice(s, 3, 3, 3, 3, 3);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "threes")));
        s = WithDice(s, 4, 4, 4, 4, 4);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "fours")));
        s = WithDice(s, 5, 5, 5, 5, 5);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "fives")));
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "sixes")));
        s = WithDice(s, 3, 3, 3, 5, 6);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "three_of_a_kind")));
        s = WithDice(s, 4, 4, 4, 4, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "four_of_a_kind")));
        s = WithDice(s, 3, 3, 3, 2, 2);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "full_house")));
        s = WithDice(s, 1, 2, 3, 4, 6);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "small_straight")));
        s = WithDice(s, 2, 3, 4, 5, 6);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "large_straight")));
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "yahtzee")));
        s = WithDice(s, 1, 2, 3, 4, 5);
        s = rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "chance")));

        var col = s.Columns.Single();
        Assert.True(col.IsCompleted);
        Assert.ThrowsAny<Exception>(() => rs.Reduce(s, new Fill(MainColumn(rs), Key(rs, "chance"))));
    }
}
