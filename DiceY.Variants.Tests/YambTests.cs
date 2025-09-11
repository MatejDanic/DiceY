using Yamb.Domain.Entities;
using Yamb.Domain.Interfaces;
using Yamb.Domain.Primitives;
using Yamb.TestUtil;
using Yamb.Variants.Yamb;
using Yamb.Variants.Yamb.Commands;

namespace Yamb.Variants.UnitTests;

public sealed class YambTests
{
    private static YambRuleset CreateRuleset(IRollService rng = null, YambConfig cfg = null) => new(rng ?? new FixedRollService([]), cfg ?? YambConfig.Default());

    private static YambState WithDice(YambState s, params int[] values) => s with { Dice = DiceFactory.D6(values) };

    private static Column Col(YambState s, ColumnKey key) =>
        s.Columns.First(c => c.ColumnKey.Equals(key));

    [Fact]
    public void Create_InitializesDiceAndColumns()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        Assert.Equal(5, s.Dice.Count);
        Assert.All(s.Dice, d => Assert.Equal(6, d.Sides));
        Assert.Equal(0, s.RollCount);
        Assert.Null(s.Announcement);
        Assert.False(s.IsCompleted);
        Assert.Contains(YambConstants.Down, s.Columns.Select(c => c.ColumnKey));
        Assert.Contains(YambConstants.Up, s.Columns.Select(c => c.ColumnKey));
        Assert.Contains(YambConstants.Free, s.Columns.Select(c => c.ColumnKey));
        Assert.Contains(YambConstants.Announcement, s.Columns.Select(c => c.ColumnKey));
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
    public void Roll_MaxRollsReached_Ignored()
    {
        var rs = CreateRuleset(new FixedRollService([3, 3, 3]));
        var s = rs.Create() with { RollCount = YambConfig.Default().MaxRollsPerTurn };
        var s2 = rs.Reduce(s, new RollCommand(0b11111));
        Assert.Equal(s, s2);
    }

    [Fact]
    public void Roll_OutOfRangeMask_Throws()
    {
        var rs = CreateRuleset(new FixedRollService([1]));
        var s = rs.Create();
        Assert.Throws<ArgumentOutOfRangeException>(() => rs.Reduce(s, new RollCommand(1 << 5)));
    }

    [Fact]
    public void Announce_WhenRollCountZero_SetsAnnouncement()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        var s2 = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        Assert.Equal(YambConstants.Trips, s2.Announcement);
    }

    [Fact]
    public void Announce_WhenRollCountNotZero_Ignored()
    {
        var rs = CreateRuleset();
        var s = rs.Create() with { RollCount = 1 };
        var s2 = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        Assert.Equal(s, s2);
    }

    [Fact]
    public void Score_AnnouncementColumn_RequiresMatchingAnnouncement()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        s = WithDice(s, 2, 2, 2, 3, 4);
        var s2 = rs.Reduce(s, new ScoreCommand(YambConstants.Announcement, YambConstants.Trips));
        var cat = Col(s2, YambConstants.Announcement).Map[YambConstants.Trips];
        Assert.Equal(3 * 2 + YambConfig.Default().TripsBonus, cat.Score);
        Assert.Equal(0, s2.RollCount);
        Assert.Null(s2.Announcement);
    }

    [Fact]
    public void Announce_SameCategoryAfterScoring_Ignored()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        s = WithDice(s, 2, 2, 2, 3, 4);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Announcement, YambConstants.Trips));
        var after = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        Assert.Equal(s, after);
    }

    [Fact]
    public void Score_NonAnnouncementColumn_DownDirectionEnforced()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = WithDice(s, 2, 2, 4, 5, 6);
        var s1 = rs.Reduce(s, new ScoreCommand(YambConstants.Down, YambConstants.Twos));
        Assert.Equal(s, s1);
        s = WithDice(s, 1, 1, 1, 6, 6);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Down, YambConstants.Ones));
        Assert.Equal(3, Col(s, YambConstants.Down).Map[YambConstants.Ones].Score);
        s = WithDice(s, 2, 2, 5, 5, 6);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Down, YambConstants.Twos));
        Assert.Equal(4, Col(s, YambConstants.Down).Map[YambConstants.Twos].Score);
    }

    [Fact]
    public void Score_UpDirection_Enforced()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 4, 5);
        var s1 = rs.Reduce(s, new ScoreCommand(YambConstants.Up, YambConstants.Ones));
        Assert.Equal(s, s1);
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Up, YambConstants.Yamb));
        Assert.Equal(5 * 6 + YambConfig.Default().YambBonus, Col(s, YambConstants.Up).Map[YambConstants.Yamb].Score);
    }

    [Fact]
    public void Score_AnnouncementColumn_WrongCategory_Ignored()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        s = WithDice(s, 2, 2, 2, 3, 4);
        var s2 = rs.Reduce(s, new ScoreCommand(YambConstants.Announcement, YambConstants.Ones));
        Assert.Equal(s, s2);
    }

    [Fact]
    public void ColumnScore_UsesYambSpecialFormula()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = WithDice(s, 1, 1, 1, 4, 5);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Free, YambConstants.Ones));
        s = WithDice(s, 1, 1, 1, 1, 1);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Free, YambConstants.Min));
        s = WithDice(s, 6, 6, 6, 6, 6);
        s = rs.Reduce(s, new ScoreCommand(YambConstants.Free, YambConstants.Max));
        var col = Col(s, YambConstants.Free);
        var ones = col.Map[YambConstants.Ones].Score ?? 0;
        var min = col.Map[YambConstants.Min].Score ?? 0;
        var max = col.Map[YambConstants.Max].Score ?? 0;
        var expected = ones + min + max + (max - min) * ones;
        Assert.Equal(expected, col.Score);
        Assert.Equal(expected + Col(s, YambConstants.Down).Score + Col(s, YambConstants.Up).Score + Col(s, YambConstants.Announcement).Score, s.TotalScore);
    }

    [Fact]
    public void GameCompletion_BlocksFurtherActions()
    {
        var rs = CreateRuleset();
        var s = rs.Create();

        foreach (var col in s.Columns)
        {
            var cats = col.Direction == Column.FillDirection.Up ? col.Categories.Reverse() : col.Categories;
            foreach (var cat in cats)
            {
                s = WithDice(s, 1, 1, 1, 1, 1);
                if (col.ColumnKey.Equals(YambConstants.Announcement))
                    s = rs.Reduce(s, new AnnounceCommand(cat.CategoryKey));
                s = rs.Reduce(s, new ScoreCommand(col.ColumnKey, cat.CategoryKey));
            }
        }

        Assert.True(s.IsCompleted);
        var afterRoll = rs.Reduce(s, new RollCommand(0b11111));
        Assert.Same(s, afterRoll);
        var afterAnnounce = rs.Reduce(s, new AnnounceCommand(YambConstants.Ones));
        Assert.Same(s, afterAnnounce);
        var firstCol = s.Columns[0].ColumnKey;
        var afterScore = rs.Reduce(s, new ScoreCommand(firstCol, YambConstants.Ones));
        Assert.Same(s, afterScore);
    }

    [Fact]
    public void CannotAnnounceAfterRolling()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new RollCommand(0b00001));
        var after = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        Assert.Equal(s, after);
    }

    [Fact]
    public void AnnouncementLocksScoringToAnnouncementColumn()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        s = WithDice(s, 2, 2, 2, 3, 4);
        var blocked = rs.Reduce(s, new ScoreCommand(YambConstants.Free, YambConstants.Trips));
        Assert.Equal(s, blocked);
        var ok = rs.Reduce(s, new ScoreCommand(YambConstants.Announcement, YambConstants.Trips));
        var col = ok.Columns.First(c => c.ColumnKey.Equals(YambConstants.Announcement));
        Assert.Equal(3 * 2 + YambConfig.Default().TripsBonus, col.Map[YambConstants.Trips].Score);
        Assert.Null(ok.Announcement);
        Assert.Equal(0, ok.RollCount);
    }

    [Fact]
    public void MustMatchAnnouncedCategoryInAnnouncementColumn()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        s = WithDice(s, 2, 2, 2, 3, 4);
        var blocked = rs.Reduce(s, new ScoreCommand(YambConstants.Announcement, YambConstants.Ones));
        Assert.Equal(s, blocked);
    }

    [Fact]
    public void FailingToMeetRuleStillCrossesOutWithZero()
    {
        var rs = CreateRuleset();
        var s = rs.Create();
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        s = WithDice(s, 1, 1, 2, 5, 6);
        var scored = rs.Reduce(s, new ScoreCommand(YambConstants.Announcement, YambConstants.Trips));
        var cat = scored.Columns.First(c => c.ColumnKey.Equals(YambConstants.Announcement)).Map[YambConstants.Trips];
        Assert.Equal(0, cat.Score);
        Assert.Null(scored.Announcement);
    }

    private static YambState CompleteNonAnnouncementColumns(YambRuleset rs, YambState s, YambConfig cfg)
    {
        var order = cfg.Categories;
        foreach (var col in s.Columns.Where(c => !c.ColumnKey.Equals(YambConstants.Announcement)))
        {
            if (col.Direction == Column.FillDirection.Up)
            {
                foreach (var k in order.Reverse())
                {
                    s = WithDice(s, 1, 1, 1, 1, 1);
                    s = rs.Reduce(s, new ScoreCommand(col.ColumnKey, k));
                }
            }
            else
            {
                foreach (var k in order)
                {
                    s = WithDice(s, 1, 1, 1, 1, 1);
                    s = rs.Reduce(s, new ScoreCommand(col.ColumnKey, k));
                }
            }
        }
        return s;
    }

    [Fact]
    public void Roll_Blocked_WhenOnlyAnnouncementOptionsLeft_AndNoAnnouncement()
    {
        var cfg = YambConfig.Default();
        var rng = new FixedRollService([5]);
        var rs = CreateRuleset(rng, cfg);
        var s = rs.Create();
        s = CompleteNonAnnouncementColumns(rs, s, cfg);
        var nonAnnAvail = s.Columns.Where(c => !c.ColumnKey.Equals(YambConstants.Announcement)).Any(c => c.GetAvailableCategories().Length > 0);
        var annAvail = s.Columns.First(c => c.ColumnKey.Equals(YambConstants.Announcement)).GetAvailableCategories().Length > 0;
        Assert.False(nonAnnAvail);
        Assert.True(annAvail);
        var before = s;
        var after = rs.Reduce(s, new RollCommand(0b00001));
        Assert.Equal(before, after);
        Assert.Equal(6, after.Dice[0].Value);
        Assert.Equal(0, after.RollCount);
    }

    [Fact]
    public void Roll_Allowed_WhenAnnouncementIsSet_EvenIfOnlyAnnouncementOptionsLeft()
    {
        var cfg = YambConfig.Default();
        var rng = new FixedRollService([2]);
        var rs = CreateRuleset(rng, cfg);
        var s = rs.Create();
        s = CompleteNonAnnouncementColumns(rs, s, cfg);
        s = rs.Reduce(s, new AnnounceCommand(YambConstants.Trips));
        var after = rs.Reduce(s, new RollCommand(0b00001));
        Assert.Equal(2, after.Dice[0].Value);
        Assert.Equal(1, after.RollCount);
    }
}