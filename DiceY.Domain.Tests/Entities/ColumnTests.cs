using DiceY.Domain.Entities;
using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.TestUtil;

namespace DiceY.Domain.Tests.Entities;

public sealed class ColumnTests
{
    private static int SumScores(IReadOnlyList<Category> categories) =>
        categories.Sum(c => c.Score.GetValueOrDefault());

    private sealed class StubRule(int score) : IScoringRule
    {
        private readonly int _score = score;
        public int GetScore(IReadOnlyList<Die> dice) => _score;
    }

    private sealed class AllowAllPolicy : IOrderPolicy
    {
        public bool CanFill(IReadOnlyList<Category> categories, CategoryKey categoryKey) => true;
    }

    private sealed class DenyPolicy : IOrderPolicy
    {
        public bool CanFill(IReadOnlyList<Category> categories, CategoryKey categoryKey) => false;
    }

    [Fact]
    public void Ctor_AssignsProperties_AndBuildsCollections()
    {
        var def = new ColumnDefinition(new ColumnKey("main"), new AllowAllPolicy(), SumScores);
        var a = new Category(new CategoryDefinition(new CategoryKey("a"), new StubRule(1)));
        var b = new Category(new CategoryDefinition(new CategoryKey("b"), new StubRule(2)));
        var col = new Column(def, [a, b]);
        Assert.Equal(new ColumnKey("main"), col.Key);
        Assert.Equal(2, col.Categories.Count);
        Assert.False(col.IsCompleted);
        Assert.Equal(0, col.Score);
    }

    [Fact]
    public void Ctor_Deduplicates_OnDuplicateKeys()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        var key = new CategoryKey("dup");
        var a = new Category(new CategoryDefinition(key, new StubRule(1)));
        var b = new Category(new CategoryDefinition(key, new StubRule(2)));
        var col = new Column(def, [a, b]);
        Assert.Single(col.Categories);
        Assert.Equal(key, col.Categories[0].Key);
    }

    [Fact]
    public void Ctor_Throws_OnNullArguments()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        Assert.Throws<ArgumentNullException>(() => new Column(def: null!, []));
        Assert.Throws<ArgumentNullException>(() => new Column(def, null!));
    }

    [Fact]
    public void Fill_NullDice_Throws()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        var a = new Category(new CategoryDefinition(new CategoryKey("a"), new StubRule(5)));
        var col = new Column(def, [a]);
        Assert.Throws<ArgumentNullException>(() => col.Fill(null!, new CategoryKey("a")));
    }

    [Fact]
    public void Fill_UnknownCategory_Throws()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        var a = new Category(new CategoryDefinition(new CategoryKey("a"), new StubRule(5)));
        var col = new Column(def, [a]);
        Assert.Throws<InvalidOperationException>(() => col.Fill(DiceFactory.D6(1), new CategoryKey("missing")));
    }

    [Fact]
    public void Fill_PolicyBlocks_ThrowsColumnFillPolicyException()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new DenyPolicy(), SumScores);
        var a = new Category(new CategoryDefinition(new CategoryKey("a"), new StubRule(5)));
        var col = new Column(def, [a]);
        Assert.Throws<ColumnFillPolicyException>(() => col.Fill(DiceFactory.D6(1), new CategoryKey("a")));
    }

    [Fact]
    public void Fill_WhenAllowed_UpdatesOnlyTargetCategory_AndReturnsNewColumn()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        var aDef = new CategoryDefinition(new CategoryKey("a"), new StubRule(1));
        var bDef = new CategoryDefinition(new CategoryKey("b"), new StubRule(2));
        var a = new Category(aDef);
        var b = new Category(bDef);
        var col = new Column(def, [a, b]);

        var col2 = col.Fill(DiceFactory.D6(1, 2, 3), new CategoryKey("b"));

        Assert.Null(col.Categories[1].Score);
        Assert.Equal(2, col2.Categories[1].Score);
        Assert.Equal(0, col.Score);
        Assert.Equal(2, col2.Score);
        Assert.False(col2.IsCompleted);
        Assert.NotSame(col, col2);
    }

    [Fact]
    public void IsCompleted_True_WhenAllCategoriesHaveScores()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        var a = new Category(new CategoryDefinition(new CategoryKey("a"), new StubRule(0)), score: 3);
        var b = new Category(new CategoryDefinition(new CategoryKey("b"), new StubRule(0)), score: 4);
        var col = new Column(def, [a, b]);
        Assert.True(col.IsCompleted);
        Assert.Equal(7, col.Score);
    }

    [Fact]
    public void Fill_Throws_WhenCategoryAlreadyScored()
    {
        var def = new ColumnDefinition(new ColumnKey("k"), new AllowAllPolicy(), SumScores);
        var a = new Category(new CategoryDefinition(new CategoryKey("a"), new StubRule(5)), score: 5);
        var col = new Column(def, [a]);
        Assert.Throws<CategoryAlreadyScoredException>(() => col.Fill(DiceFactory.D6(1), new CategoryKey("a")));
    }
}
