using Yamb.Domain.Entities;
using Yamb.TestUtil;

namespace Yamb.Domain.UnitTests.Entities;

public sealed class DieTests
{
    [Fact]
    public void DefaultCtor_ProducesValidDie()
    {
        var d = new Die();
        Assert.True(d.Sides >= 1);
        Assert.InRange(d.Value, 1, d.Sides);
    }

    [Theory]
    [InlineData(6, 1)]
    [InlineData(6, 6)]
    [InlineData(10, 7)]
    public void Ctor_AssignsSidesAndValue(int sides, int value)
    {
        var d = new Die(sides, value);
        Assert.Equal(sides, d.Sides);
        Assert.Equal(value, d.Value);
    }

    [Fact]
    public void Roll_UsesRngValue()
    {
        var d = new Die(6, 1);
        var rng = new FixedRollService([ 5 ]);
        d.Roll(rng);
        Assert.Equal(5, d.Value);
        Assert.Equal(6, d.Sides);
    }

    [Fact]
    public void Roll_MultipleTimes_ConsumesSequence()
    {
        var d = new Die(6, 1);
        var rng = new FixedRollService([ 2, 4, 6 ]);
        d.Roll(rng);
        Assert.Equal(2, d.Value);
        d.Roll(rng);
        Assert.Equal(4, d.Value);
        d.Roll(rng);
        Assert.Equal(6, d.Value);
    }

    [Fact]
    public void Roll_WhenSequenceEmpty_FallsBackToSides()
    {
        var d = new Die(8, 3);
        var rng = new FixedRollService([]);
        d.Roll(rng);
        Assert.Equal(8, d.Value);
        Assert.Equal(8, d.Sides);
    }

    [Fact]
    public void Roll_PreservesSides()
    {
        var d = new Die(10, 3);
        var rng = new FixedRollService([ 7 ]);
        d.Roll(rng);
        Assert.Equal(10, d.Sides);
    }
}
