using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Variants.Yahtzee;
using System.Collections.Immutable;

namespace DiceY.Variants.Yamb;

public sealed record YambState : IGameState
{
    public int RollCount { get; init; }
    public IReadOnlyList<Die> Dice => _dice;
    public IReadOnlyList<Column> Columns => _columns;
    public CategoryKey? Announcement => _announcement;

    private readonly IReadOnlyList<Die> _dice;
    private readonly IReadOnlyList<Column> _columns;
    private readonly CategoryKey? _announcement;

    public YambState(IReadOnlyList<Die> dice, IReadOnlyList<Column> columns, int rollCount = 0, CategoryKey? announcement = null)
    {
        ArgumentNullException.ThrowIfNull(dice, nameof(dice));
        ArgumentNullException.ThrowIfNull(columns, nameof(columns));
        _dice = dice;
        _columns = columns;
        RollCount = rollCount;
        _announcement = announcement;
    }
}
