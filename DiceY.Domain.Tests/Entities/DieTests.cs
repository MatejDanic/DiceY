using DiceY.Domain.Entities;
using DiceY.TestUtil;

namespace DiceY.Domain.Tests.Entities;

public sealed class DieTests
{
    [Fact]
    public void DefaultCtor_ProducesValidDie()
    {
        var d = new Die();
        Assert.InRange(d.Value, 1, 6);
    }

    [Theory]
    [InlineData(6, 1)]
    [InlineData(6, 6)]
    [InlineData(10, 7)]
    public void Ctor_AssignsValue_AndSidesAreStored(int sides, int value)
    {
        var d = new Die(sides, value);
        Assert.Equal(value, d.Value);
        var after = d.Roll(new FixedRollService([]));
        Assert.Equal(sides, after.Value);
    }

    [Fact]
    public void Roll_UsesRngValue_AndReturnsNewDie()
    {
        var d = new Die(6, 1);
        var rng = new FixedRollService([5]);
        var after = d.Roll(rng);
        Assert.Equal(1, d.Value);
        Assert.Equal(5, after.Value);
    }

    [Fact]
    public void Roll_MultipleTimes_ConsumesSequence()
    {
        var d = new Die(6, 1);
        var rng = new FixedRollService([2, 4, 6]);
        var d1 = d.Roll(rng);
        Assert.Equal(2, d1.Value);
        var d2 = d1.Roll(rng);
        Assert.Equal(4, d2.Value);
        var d3 = d2.Roll(rng);
        Assert.Equal(6, d3.Value);
    }

    [Fact]
    public void Roll_WhenSequenceEmpty_FallsBackToSides()
    {
        var d = new Die(8, 3);
        var rng = new FixedRollService([]);
        var after = d.Roll(rng);
        Assert.Equal(8, after.Value);
    }

    [Fact]
    public void Roll_PreservesSidesAcrossRolls()
    {
        var d = new Die(10, 3);
        var first = d.Roll(new FixedRollService([7]));
        var second = first.Roll(new FixedRollService([]));
        Assert.Equal(10, second.Value);
    }
}
