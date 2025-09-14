using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed record YahtzeeState(
    ImmutableArray<Die> DiceArray,
    Column Column,
    int RollCount = 0
) : IGameState
{
    public IReadOnlyList<Die> Dice => DiceArray;
    public IReadOnlyList<Column> Columns => [Column];
}
