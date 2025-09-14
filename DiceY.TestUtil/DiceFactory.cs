using DiceY.Domain.Entities;
using System.Collections.Immutable;

namespace DiceY.TestUtil;

public sealed class DiceFactory
{
    public static ImmutableArray<Die> D6(params int[] values) => [.. values.Select(v => new Die(6, v))];
    public static ImmutableArray<Die> D(int sides, params int[] values) => [.. values.Select(v => new Die(sides, v))];
}
