using DiceY.Domain.Entities;

namespace DiceY.Domain.Interfaces;

public interface IGameState
{
    int RollCount { get; }
    IReadOnlyList<Die> Dice { get; }
    IReadOnlyList<Column> Columns { get; }
}
