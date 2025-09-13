
using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.TestUtil;

namespace DiceY.Domain.Tests.Entities;

public sealed class ColumnTests
{
    private static int SumScores(IReadOnlyDictionary<CategoryKey, Category> map) =>
        map.Values.Sum(c => c.Score.GetValueOrDefault());

    private sealed class StubRule(bool result, int scored) : IScoringRule
    {
        private readonly bool _result = result;
        private readonly int _scored = scored;
        public bool TryScore(IReadOnlyList<Die> dice, out int score) { score = _scored; return _result; }
    }

    [Fact]
    public void Ctor_AssignsProperties_AndBuildsCollections()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        var b = new Category(new CategoryKey("b"), new StubRule(true, 2));
        var col = new Column(new ColumnKey("main"), Column.FillDirection.Free, [a, b], SumScores);
        Assert.Equal("main", col.ColumnKey.Key);
        Assert.Equal(Column.FillDirection.Free, col.Direction);
        Assert.Equal(2, col.Categories.Length);
        Assert.Equal(2, col.Map.Count);
        Assert.False(col.IsCompleted);
        Assert.Equal(0, col.Score);
    }

    [Fact]
    public void Ctor_Throws_OnEmptyCategories()
    {
        Assert.Throws<ArgumentException>(() =>
            new Column(new ColumnKey("k"), Column.FillDirection.Free, [], SumScores));
    }

    [Fact]
    public void Ctor_Throws_OnDuplicateKeys()
    {
        var key = new CategoryKey("dup");
        var a = new Category(key, new StubRule(true, 1));
        var b = new Category(key, new StubRule(true, 2));
        Assert.Throws<ArgumentException>(() =>
            new Column(new ColumnKey("k"), Column.FillDirection.Free, [a, b], SumScores));
    }

    [Fact]
    public void Ctor_Throws_OnNullComputeScore()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        Assert.Throws<ArgumentNullException>(() =>
            new Column(new ColumnKey("k"), Column.FillDirection.Free, [a], null));
    }

    [Fact]
    public void TryScore_ReturnsFalse_OnNullDice()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        var col = new Column(new ColumnKey("k"), Column.FillDirection.Free, [a], SumScores);
        var ok = col.TryScore(null, a.CategoryKey);
        Assert.False(ok);
        Assert.Null(a.Score);
    }

    [Fact]
    public void TryScore_ReturnsFalse_ForUnknownCategory()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        var col = new Column(new ColumnKey("k"), Column.FillDirection.Free, [a], SumScores);
        var ok = col.TryScore(DiceFactory.D6(1), new CategoryKey("missing"));
        Assert.False(ok);
        Assert.Null(a.Score);
    }

    [Fact]
    public void FreeDirection_AllowsAnyOrder_UpdatesScore_AndCompletion()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        var b = new Category(new CategoryKey("b"), new StubRule(true, 2));
        var c = new Category(new CategoryKey("c"), new StubRule(true, 3));
        var col = new Column(new ColumnKey("k"), Column.FillDirection.Free, [ a, b, c ], SumScores);

        Assert.True(col.CanScore(c.CategoryKey));
        Assert.True(col.TryScore(DiceFactory.D6(1, 2, 3), c.CategoryKey));
        Assert.Equal(3, c.Score);
        Assert.False(col.IsCompleted);
        Assert.Equal(3, col.Score);

        Assert.True(col.CanScore(a.CategoryKey));
        Assert.True(col.TryScore(DiceFactory.D6(1), a.CategoryKey));
        Assert.Equal(4, col.Score);

        Assert.True(col.CanScore(b.CategoryKey));
        Assert.True(col.TryScore(DiceFactory.D6(6), b.CategoryKey));
        Assert.True(col.IsCompleted);
        Assert.Equal(6, col.Score);
    }

    [Fact]
    public void DownDirection_EnforcesTopDown()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        var b = new Category(new CategoryKey("b"), new StubRule(true, 2));
        var c = new Category(new CategoryKey("c"), new StubRule(true, 3));
        var col = new Column(new ColumnKey("k"), Column.FillDirection.Down, [a, b, c], SumScores);

        Assert.True(col.CanScore(a.CategoryKey));
        Assert.False(col.CanScore(b.CategoryKey));
        Assert.False(col.CanScore(c.CategoryKey));
        Assert.Equal([a.CategoryKey], col.GetAvailableCategories());

        Assert.True(col.TryScore(DiceFactory.D6(1), a.CategoryKey));
        Assert.True(col.CanScore(b.CategoryKey));
        Assert.False(col.CanScore(c.CategoryKey));
        Assert.Equal([b.CategoryKey], col.GetAvailableCategories());

        Assert.True(col.TryScore(DiceFactory.D6(2), b.CategoryKey));
        Assert.True(col.CanScore(c.CategoryKey));
        Assert.Equal([c.CategoryKey], col.GetAvailableCategories());

        Assert.True(col.TryScore(DiceFactory.D6(3), c.CategoryKey));
        Assert.True(col.IsCompleted);
        Assert.Equal(6, col.Score);
    }

    [Fact]
    public void UpDirection_EnforcesBottomUp()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(true, 1));
        var b = new Category(new CategoryKey("b"), new StubRule(true, 2));
        var c = new Category(new CategoryKey("c"), new StubRule(true, 3));
        var col = new Column(new ColumnKey("k"), Column.FillDirection.Up, [a, b, c], SumScores);

        Assert.False(col.CanScore(a.CategoryKey));
        Assert.False(col.CanScore(b.CategoryKey));
        Assert.True(col.CanScore(c.CategoryKey));
        Assert.Equal([c.CategoryKey], col.GetAvailableCategories());

        Assert.True(col.TryScore(DiceFactory.D6(3), c.CategoryKey));
        Assert.True(col.CanScore(b.CategoryKey));
        Assert.Equal([b.CategoryKey], col.GetAvailableCategories());

        Assert.True(col.TryScore(DiceFactory.D6(2), b.CategoryKey));
        Assert.True(col.CanScore(a.CategoryKey));
        Assert.Equal([a.CategoryKey], col.GetAvailableCategories());

        Assert.True(col.TryScore(DiceFactory.D6(1), a.CategoryKey));
        Assert.True(col.IsCompleted);
        Assert.Equal(6, col.Score);
    }

    [Fact]
    public void TryScore_WhenRuleFails_CrossesOutWithZero_AndBlocksFurtherScoring()
    {
        var a = new Category(new CategoryKey("a"), new StubRule(false, 99));
        var b = new Category(new CategoryKey("b"), new StubRule(true, 5));
        var col = new Column(new ColumnKey("k"), Column.FillDirection.Free, [a, b], SumScores);

        var ok1 = col.TryScore(DiceFactory.D6(1, 2, 3), a.CategoryKey);
        var ok2 = col.TryScore(DiceFactory.D6(6, 6, 6), a.CategoryKey);

        Assert.True(ok1);
        Assert.False(ok2);
        Assert.Equal(0, a.Score);
        Assert.True(col.TryScore(DiceFactory.D6(6, 6, 6), b.CategoryKey));
        Assert.True(col.IsCompleted);
        Assert.Equal(5, col.Score);
    }
}