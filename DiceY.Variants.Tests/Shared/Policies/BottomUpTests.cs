using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.Variants.Shared.Policies;

namespace DiceY.Variants.Tests.Shared.Policies;

public sealed class BottomUpTests
{
    private sealed class ZeroRule : IScoringRule { public int GetScore(IReadOnlyList<Die> dice) => 0; }

    private static Category Cat(string key, int? score = null) =>
        new(new CategoryDefinition(new CategoryKey(key), new ZeroRule()), score);

    [Fact]
    public void Allows_Last_Unfilled_Only_When_None_Filled()
    {
        var cats = new[] { Cat("a"), Cat("b"), Cat("c") };
        var policy = new BottomUp();
        Assert.True(policy.CanFill(cats, new CategoryKey("c")));
        Assert.False(policy.CanFill(cats, new CategoryKey("a")));
        Assert.False(policy.CanFill(cats, new CategoryKey("b")));
    }

    [Fact]
    public void Allows_Previous_Above_Last_After_Last_Filled()
    {
        var cats = new[] { Cat("a"), Cat("b"), Cat("c", 1) };
        var policy = new BottomUp();
        Assert.True(policy.CanFill(cats, new CategoryKey("b")));
        Assert.False(policy.CanFill(cats, new CategoryKey("a")));
    }

    [Fact]
    public void Denies_Already_Filled()
    {
        var cats = new[] { Cat("a"), Cat("b", 1), Cat("c") };
        var policy = new BottomUp();
        Assert.False(policy.CanFill(cats, new CategoryKey("b")));
    }

    [Fact]
    public void Denies_Unknown()
    {
        var cats = new[] { Cat("a"), Cat("b") };
        var policy = new BottomUp();
        Assert.False(policy.CanFill(cats, new CategoryKey("x")));
    }

    [Fact]
    public void Denies_When_List_Empty()
    {
        var policy = new BottomUp();
        Assert.False(policy.CanFill(Array.Empty<Category>(), new CategoryKey("a")));
    }
}
