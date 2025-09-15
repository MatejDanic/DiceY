using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using System.Collections.Immutable;

namespace DiceY.Variants.Yamb;

public sealed record YambState : IGameState
{
    public ImmutableArray<Die> DiceArray { get; init; }
    public ImmutableArray<Column> ColumnsArray { get; init; }
    public int RollCount { get; init; }
    public CategoryKey? Announcement { get; init; }

    private readonly ImmutableDictionary<ColumnKey, int> _columnIndex;

    public IReadOnlyList<Die> Dice => DiceArray;
    public IReadOnlyList<Column> Columns => ColumnsArray;

    public YambState(
        ImmutableArray<Die> diceArray,
        ImmutableArray<Column> columnsArray,
        int rollCount = 0,
        CategoryKey? announcement = null)
    {
        DiceArray = diceArray;
        ColumnsArray = columnsArray;
        RollCount = rollCount;
        Announcement = announcement;
        _columnIndex = columnsArray.Select((c, i) => (c.Key, i)).ToImmutableDictionary(t => t.Key, t => t.i);
    }

    public Column GetColumn(ColumnKey key) => ColumnsArray[_columnIndex[key]];

    public YambState WithDice(ImmutableArray<Die> diceArray) =>
        new(diceArray, ColumnsArray, RollCount, Announcement);

    public YambState WithColumns(ImmutableArray<Column> columnsArray) =>
        new(DiceArray, columnsArray, RollCount, Announcement);

    public YambState WithRollCount(int rollCount) =>
        new(DiceArray, ColumnsArray, rollCount, Announcement);

    public YambState WithAnnouncement(CategoryKey? announcement) =>
        new(DiceArray, ColumnsArray, RollCount, announcement);

    public YambState AfterRoll(ImmutableArray<Die> diceArray) =>
        new(diceArray, ColumnsArray, RollCount + 1, Announcement);

    public YambState AfterAnnounce(CategoryKey categoryKey) =>
        new(DiceArray, ColumnsArray, RollCount, categoryKey);

    public YambState AfterFill(ColumnKey key, Column updated)
    {
        var i = _columnIndex[key];
        var newCols = ColumnsArray.SetItem(i, updated);
        return new(DiceArray, newCols, 0, null);
    }
}
