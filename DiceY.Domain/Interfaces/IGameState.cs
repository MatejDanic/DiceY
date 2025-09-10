using DiceY.Domain.Entities;

namespace DiceY.Domain.Interfaces;

public interface IGameState
{
    int RollCount { get; }
    bool IsCompleted { get; }
    IReadOnlyList<Die> Dice { get; }
    IReadOnlyList<Column> Columns { get; }
    int TotalScore { get; }
}
