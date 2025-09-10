using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Variants.Yamb;

public sealed record YambState(IReadOnlyList<Die> Dice, IReadOnlyList<Column> Columns, int RollCount = 0, CategoryKey? Announcement = null) : IGameState
{
    public bool IsCompleted => Columns.All(c => c.IsCompleted);
    public int TotalScore => Columns.Sum(c => c.Score);
}
