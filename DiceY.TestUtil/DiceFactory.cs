using DiceY.Domain.Entities;

namespace DiceY.TestUtil;

public sealed class DiceFactory
{
    public static List<Die> D6(params int[] values) => [.. values.Select(v => new Die(6, v))];
    public static List<Die> D(int sides, params int[] values) => [.. values.Select(v => new Die(sides, v))];
}
