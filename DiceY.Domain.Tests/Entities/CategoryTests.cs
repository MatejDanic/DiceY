using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.TestUtil;

namespace DiceY.Domain.UnitTests.Entities;

public sealed class CategoryTests
{
    private sealed class StubRule(bool result, int scored) : IScoringRule
    {
        private readonly bool _result = result;
        private readonly int _scored = scored;
        public bool TryScore(IReadOnlyList<Die> dice, out int score) { score = _scored; return _result; }
    }

    [Fact]
    public void Ctor_AssignsKey()
    {
        var key = new CategoryKey("ones");
        var cat = new Category(key, new StubRule(true, 3));
        Assert.Equal(key, cat.CategoryKey);
        Assert.Null(cat.Score);
    }

    [Fact]
    public void Ctor_NullRule_Throws()
    {
        var key = new CategoryKey("any");
        Assert.Throws<ArgumentNullException>(() => new Category(key, null));
    }

    [Fact]
    public void TryScore_NullDice_ReturnsFalse_AndDoesNotSetScore()
    {
        var cat = new Category(new CategoryKey("x"), new StubRule(true, 10));
        var ok = cat.TryScore(null, out var applied);
        Assert.False(ok);
        Assert.Equal(0, applied);
        Assert.Null(cat.Score);
    }

    [Fact]
    public void TryScore_WhenRuleSucceeds_SetsScore_AndReturnsTrue()
    {
        var cat = new Category(new CategoryKey("x"), new StubRule(true, 12));
        var ok = cat.TryScore(DiceFactory.D6(1, 2, 3), out var applied);
        Assert.True(ok);
        Assert.Equal(12, applied);
        Assert.Equal(12, cat.Score);
    }

    [Fact]
    public void TryScore_WhenRuleFails_CrossesOutWithZero_AndReturnsTrue()
    {
        var cat = new Category(new CategoryKey("x"), new StubRule(false, 99));
        var ok = cat.TryScore(DiceFactory.D6(1, 2, 3), out var applied);
        Assert.True(ok);
        Assert.Equal(0, applied);
        Assert.Equal(0, cat.Score);
    }

    [Fact]
    public void TryScore_SecondTime_ReturnsFalse_AndDoesNotChangeScore()
    {
        var cat = new Category(new CategoryKey("x"), new StubRule(true, 7));
        var ok1 = cat.TryScore(DiceFactory.D6(1, 2, 3), out var applied1);
        var ok2 = cat.TryScore(DiceFactory.D6(4, 5, 6), out var applied2);
        Assert.True(ok1);
        Assert.Equal(7, applied1);
        Assert.False(ok2);
        Assert.Equal(0, applied2);
        Assert.Equal(7, cat.Score);
    }
}