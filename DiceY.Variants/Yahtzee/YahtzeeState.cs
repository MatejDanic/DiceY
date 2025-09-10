using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Yahtzee;

public sealed record YahtzeeState(IReadOnlyList<Die> Dice, IReadOnlyList<Column> Columns, int RollCount = 0) : IGameState
{
    public bool IsCompleted => Columns.All(c => c.IsCompleted);
    public int TotalScore => Columns.Sum(c => c.Score);

}