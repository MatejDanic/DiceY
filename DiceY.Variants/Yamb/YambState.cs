using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using System.Collections.Immutable;

namespace DiceY.Variants.Yamb;

public sealed record YambState(
    ImmutableArray<Die> DiceArray,
    ImmutableArray<Column> ColumnsArray,
    int RollCount = 0,
    CategoryKey? Announcement = null
) : IGameState
{
    public IReadOnlyList<Die> Dice => DiceArray;
    public IReadOnlyList<Column> Columns => ColumnsArray;
}
