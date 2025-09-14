using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.TestUtil;
using DiceY.Variants.Shared.Commands;
using DiceY.Variants.Yamb;
using DiceY.Variants.Yamb.Commands;

namespace DiceY.Variants.Tests.Yamb;

public sealed class YambTests
{
    private static YambEngine CreateEngine(IRollService? rng = null, GameDefinition? def = null)
        => new(rng ?? new FixedRollService([]), def);

    private static YambState WithDice(YambState s, params int[] values)
        => new(DiceFactory.D6(values), [.. s.Columns], s.RollCount, s.Announcement);

    private static Column Col(YambState s, string keyValue) =>
        s.Columns.First(c => c.Key.Value == keyValue);

    private static ColumnKey ColKey(YambEngine rs, string keyValue) =>
        rs.Definition.ColumnDefinitions.First(cd => cd.Key.Value == keyValue).Key;

    private static CategoryKey CatKey(YambEngine rs, string keyValue) =>
        rs.Definition.CategoryDefinitions.First(cd => cd.Key.Value == keyValue).Key;

    [Fact]
    public void Create_InitializesDiceAndColumns()
    {
        var rs = CreateEngine(def: YambConfig.Build());
        var s = rs.Create();
        Assert.Equal(5, s.Dice.Count);
        Assert.All(s.Dice, d => Assert.InRange(d.Value, 1, 6));
        Assert.Equal(0, s.RollCount);
        var keys = s.Columns.Select(c => c.Key.Value).ToArray();
        Assert.Contains("down", keys);
        Assert.Contains("up", keys);
        Assert.Contains("free", keys);
        Assert.Contains("announcement", keys);
    }

    [Fact]
    public void Roll_RollsUnmaskedDice_AndIncrementsRollCount()
    {
        var rng = new FixedRollService([2, 4]);
        var rs = CreateEngine(rng, YambConfig.Build());
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
        var rs = CreateEngine(rng, YambConfig.Build());
        var s = rs.Create();
        var s2 = rs.Reduce(s, new Roll(0));
        Assert.All(s2.Dice, d => Assert.Equal(1, d.Value));
        Assert.Equal(1, s2.RollCount);
    }

    [Fact]
    public void Roll_MaxRollsReached_Throws()
    {
        var rs = CreateEngine(def: YambConfig.Build());
        var created = rs.Create();
        var s = new YambState(created.DiceArray, created.ColumnsArray, RollCount: rs.Definition.MaxRollsPerTurn, Announcement: null);
        Assert.Throws<InvalidOperationException>(() => rs.Reduce(s, new Roll(0b11111)));
    }

    [Fact]
    public void Roll_OutOfRangeMask_Throws()
    {
        var rs = CreateEngine(new FixedRollService([1]), YambConfig.Build());
        var s = rs.Create();
        Assert.Throws<ArgumentOutOfRangeException>(() => rs.Reduce(s, new Roll(1 << 5)));
    }

    [Fact]
    public void Announce_WhenRollCountZero_SetsAndEnablesOnlyAnnouncementMatch()
    {
        var rs = CreateEngine(def: YambConfig.Build());
        var s = rs.Create();
        var trips = CatKey(rs, "trips");
        var s2 = rs.Reduce(s, new Announce(trips));
        s2 = WithDice(s2, 2, 2, 2, 3, 4);
        Assert.Throws<InvalidOperationException>(() => rs.Reduce(s2, new Fill(ColKey(rs, "free"), trips)));
        Assert.Throws<InvalidOperationException>(() => rs.Reduce(s2, new Fill(ColKey(rs, "announcement"), CatKey(rs, "ones"))));
        var scored = rs.Reduce(s2, new Fill(ColKey(rs, "announcement"), trips));
        var cat = Col(scored, "announcement").Categories.First(c => c.Key.Value == "trips");
        Assert.True(cat.Score.HasValue);
        Assert.Equal(3 * 2 + 10, cat.Score);
        Assert.Equal(0, scored.RollCount);
    }

    [Fact]
    public void Announce_WhenRollCountNotZero_Throws()
    {
        var rs = CreateEngine(def: YambConfig.Build());
        var s = rs.Create();
        var rolled = rs.Reduce(s, new Roll(0b00001));
        Assert.Throws<InvalidOperationException>(() => rs.Reduce(rolled, new Announce(CatKey(rs, "trips"))));
    }

    [Fact]
    public void ColumnScore_UsesYambSpecialFormula_InFreeColumn()
    {
        var rs = CreateEngine(def: YambConfig.Build());
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 4, 5);
        s = rs.Reduce(s, new Fill(ColKey(rs, "free"), CatKey(rs, "ones")));
        s = WithDice(s, 1, 1, 1, 1, 1);
        s = rs.Reduce(s, new Fill(ColKey(rs, "free"), CatKey(rs, "min")));
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new Fill(ColKey(rs, "free"), CatKey(rs, "max")));
        var col = Col(s, "free");
        var ones = col.Categories.First(c => c.Key.Value == "ones").Score ?? 0;
        var min = col.Categories.First(c => c.Key.Value == "min").Score ?? 0;
        var max = col.Categories.First(c => c.Key.Value == "max").Score ?? 0;
        var expected = ones + (max - min) * ones;
        Assert.Equal(expected, col.Score);
    }

    [Fact]
    public void GameCompletion_BlocksFurtherRollsAndScoring_WithExceptions()
    {
        var rs = CreateEngine(def: YambConfig.Build());
        var s = rs.Create();

        foreach (var col in s.Columns)
        {
            var cats = col.Key.Value == "up" ? col.Categories.Reverse() : col.Categories;
            foreach (var cat in cats)
            {
                s = WithDice(s, 1, 1, 1, 1, 1);
                if (col.Key.Value == "announcement")
                    s = rs.Reduce(s, new Announce(cat.Key));
                s = rs.Reduce(s, new Fill(col.Key, cat.Key));
            }
        }

        Assert.Throws<InvalidOperationException>(() => rs.Reduce(s, new Roll(0b11111)));
        var firstCol = s.Columns[0].Key;
        Assert.Throws<InvalidOperationException>(() => rs.Reduce(s, new Fill(firstCol, s.Columns[0].Categories[0].Key)));
    }

    [Fact]
    public void Roll_Blocked_WhenOnlyAnnouncementOptionsLeft_AndNoAnnouncement_Throws()
    {
        var def = YambConfig.Build();
        var rng = new FixedRollService([5]);
        var rs = CreateEngine(rng, def);
        var s = rs.Create();

        foreach (var col in s.Columns.Where(c => c.Key.Value != "announcement"))
        {
            var order = col.Key.Value == "up" ? col.Categories.Reverse() : col.Categories;
            foreach (var k in order.Select(c => c.Key))
            {
                s = WithDice(s, 1, 1, 1, 1, 1);
                s = rs.Reduce(s, new Fill(col.Key, k));
            }
        }

        Assert.Throws<InvalidOperationException>(() => rs.Reduce(s, new Roll(0b00001)));
    }

    [Fact]
    public void Roll_Allowed_WhenAnnouncementIsSet_EvenIfOnlyAnnouncementOptionsLeft()
    {
        var def = YambConfig.Build();
        var rng = new FixedRollService([2]);
        var rs = CreateEngine(rng, def);
        var s = rs.Create();

        foreach (var col in s.Columns.Where(c => c.Key.Value != "announcement"))
        {
            var order = col.Key.Value == "up" ? col.Categories.Reverse() : col.Categories;
            foreach (var k in order.Select(c => c.Key))
            {
                s = WithDice(s, 1, 1, 1, 1, 1);
                s = rs.Reduce(s, new Fill(col.Key, k));
            }
        }

        var trips = CatKey(rs, "trips");
        s = rs.Reduce(s, new Announce(trips));
        var after = rs.Reduce(s, new Roll(0b11110));
        Assert.Equal(2, after.Dice[0].Value);
        Assert.Equal(1, after.RollCount);
    }
}
