using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.Variants.Shared.Policies;

namespace DiceY.Variants.Tests.Shared.Policies;

public sealed class FreeTests
{
    private sealed class ZeroRule : IScoringRule { public int GetScore(IReadOnlyList<Die> dice) => 0; }

    private static Category Cat(string key, int? score = null) =>
        new(new CategoryDefinition(new CategoryKey(key), new ZeroRule()), score);

    [Fact]
    public void Allows_Unfilled_Category()
    {
        var cats = new[] { Cat("a"), Cat("b"), Cat("c") };
        var policy = new Free();
        Assert.True(policy.CanFill(cats, new CategoryKey("b")));
    }

    [Fact]
    public void Denies_Already_Filled_Category()
    {
        var cats = new[] { Cat("a", 1), Cat("b"), Cat("c") };
        var policy = new Free();
        Assert.False(policy.CanFill(cats, new CategoryKey("a")));
    }

    [Fact]
    public void Denies_Unknown_Category()
    {
        var cats = new[] { Cat("a"), Cat("b") };
        var policy = new Free();
        Assert.False(policy.CanFill(cats, new CategoryKey("x")));
    }

    [Fact]
    public void Denies_When_List_Empty()
    {
        var policy = new Free();
        Assert.False(policy.CanFill(Array.Empty<Category>(), new CategoryKey("a")));
    }
}
