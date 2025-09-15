using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed record YahtzeeState : IGameState
{
    public ImmutableArray<Die> DiceArray { get; init; }
    public Column Column { get; init; }
    public int RollCount { get; init; }

    private readonly ImmutableArray<Column> _columns;

    public IReadOnlyList<Die> Dice => DiceArray;
    public IReadOnlyList<Column> Columns => _columns;

    public YahtzeeState(ImmutableArray<Die> diceArray, Column column, int rollCount = 0)
    {
        DiceArray = diceArray;
        Column = column;
        RollCount = rollCount;
        _columns = [column];
    }

    public YahtzeeState WithDice(ImmutableArray<Die> diceArray) =>
        new(diceArray, Column, RollCount);

    public YahtzeeState WithColumn(Column column) =>
        new(DiceArray, column, RollCount);

    public YahtzeeState WithRollCount(int rollCount) =>
        new(DiceArray, Column, rollCount);

    public YahtzeeState AfterRoll(ImmutableArray<Die> diceArray) =>
        new(diceArray, Column, RollCount + 1);

    public YahtzeeState AfterFill(Column updatedColumn) =>
        new(DiceArray, updatedColumn, 0);
}
