using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed record YahtzeeState : IGameState
{
    public int RollCount { get; init; }
    public IReadOnlyList<Die> Dice => _dice;
    public IReadOnlyList<Column> Columns => _columns;

    private readonly IReadOnlyList<Die> _dice;
    private readonly IReadOnlyList<Column> _columns;

    public YahtzeeState(IReadOnlyList<Die> dice, IReadOnlyList<Column> columns, int rollCount = 0)
    {
        ArgumentNullException.ThrowIfNull(dice, nameof(dice));
        ArgumentNullException.ThrowIfNull(columns, nameof(columns));
        _dice = dice;
        _columns = columns;
        RollCount = rollCount;
    }
}
