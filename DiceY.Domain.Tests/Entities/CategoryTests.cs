using DiceY.Domain.Entities;
using DiceY.Domain.Exceptions;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.TestUtil;

namespace DiceY.Domain.Tests.Entities;

public sealed class CategoryTests
{
    private sealed class StubRule(int score) : IScoringRule
    {
        private readonly int _score = score;
        public int GetScore(IReadOnlyList<Die> dice) => _score;
    }

    [Fact]
    public void Ctor_AssignsKey_AndScoreIsNull()
    {
        var key = new CategoryKey("ones");
        var cat = new Category(new CategoryDefinition(key, new StubRule(3)));
        Assert.Equal(key, cat.Key);
        Assert.Null(cat.Score);
    }

    [Fact]
    public void Ctor_NullDefinition_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Category(def: null!));
    }

    [Fact]
    public void Fill_NullDice_Throws()
    {
        var cat = new Category(new CategoryDefinition(new CategoryKey("x"), new StubRule(10)));
        Assert.Throws<ArgumentNullException>(() => cat.Fill(null!));
    }

    [Fact]
    public void Fill_WhenUnscored_ComputesScore_AndReturnsNewInstance()
    {
        var cat = new Category(new CategoryDefinition(new CategoryKey("x"), new StubRule(12)));
        var filled = cat.Fill(DiceFactory.D6(1, 2, 3));
        Assert.Null(cat.Score);
        Assert.Equal(12, filled.Score);
        Assert.Equal(cat.Key, filled.Key);
    }

    [Fact]
    public void Fill_WhenRuleReturnsZero_SetsZero()
    {
        var cat = new Category(new CategoryDefinition(new CategoryKey("x"), new StubRule(0)));
        var filled = cat.Fill(DiceFactory.D6(1, 2, 3));
        Assert.Equal(0, filled.Score);
    }

    [Fact]
    public void Fill_WhenAlreadyScored_Throws()
    {
        var cat = new Category(new CategoryDefinition(new CategoryKey("x"), new StubRule(7)), score: 7);
        Assert.Throws<CategoryAlreadyScoredException>(() => cat.Fill(DiceFactory.D6(4, 5, 6)));
    }
}
