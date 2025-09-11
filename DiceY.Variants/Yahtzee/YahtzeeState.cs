using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed record YahtzeeState : IGameState
{
    public int RollCount { get; init; }

    public IReadOnlyList<Die> Dice => _dice;
    public IReadOnlyList<Column> Columns => _columns;

    private readonly ImmutableArray<Die> _dice;
    private readonly ImmutableArray<Column> _columns;

    public YahtzeeState(ImmutableArray<Die> dice, ImmutableArray<Column> columns, int rollCount = 0)
    {
        if (dice.IsDefault) throw new ArgumentException(nameof(dice));
        if (columns.IsDefault) throw new ArgumentException(nameof(columns));
        _dice = dice;
        _columns = columns;
        RollCount = rollCount;
    }

    public YahtzeeState WithDice(ImmutableArray<Die> dice) =>
        new(dice, _columns, RollCount);

    public YahtzeeState WithColumns(ImmutableArray<Column> columns) =>
        new(_dice, columns, RollCount);

}