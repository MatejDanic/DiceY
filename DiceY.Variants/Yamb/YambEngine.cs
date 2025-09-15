using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.Variants.Shared.Commands;
using DiceY.Variants.Yamb.Commands;
using System.Collections.Immutable;

namespace DiceY.Variants.Yamb;

public sealed class YambEngine(IRollService rng, GameDefinition? definition = null) : IGameEngine<YambState>
{
    public GameDefinition Definition { get; } = definition ?? YambConfig.Build();

    public YambState Create()
    {
        var dice = Enumerable.Range(0, Definition.DiceCount)
            .Select(_ => new Die(Definition.DiceSides))
            .ToImmutableArray();

        var cols = Definition.ColumnDefinitions
            .Select(colDef =>
            {
                var cats = Definition.CategoryDefinitions
                    .Select(catDef => new Category(catDef))
                    .ToImmutableArray();
                return new Column(colDef, cats);
            })
            .ToImmutableArray();

        return new YambState(dice, cols);
    }

    public YambState Reduce(YambState state, IGameCommand<YambState> action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);
        if (state.Columns.All(c => c.IsCompleted))
            throw new InvalidOperationException("Game already completed.");
        return action switch
        {
            Roll roll => ReduceRoll(state, roll.Mask),
            Announce announce => ReduceAnnounce(state, announce.CategoryKey),
            Fill fill => ReduceFill(state, fill.ColumnKey, fill.CategoryKey),
            _ => throw new NotSupportedException(action.GetType().Name)
        };
    }

    private YambState ReduceRoll(YambState state, int mask)
    {
        if ((mask >> Definition.DiceCount) != 0)
            throw new ArgumentOutOfRangeException(nameof(mask));

        var announcementColKey = Definition.ColumnDefinitions.First(cd => cd.Key.Value == "announcement").Key;
        var nonAnnHasOptions = state.Columns.Any(c => !c.Key.Equals(announcementColKey) && !c.IsCompleted);
        var annCol = state.Columns.First(c => c.Key.Equals(announcementColKey));
        var annHasOptions = !annCol.IsCompleted;
        var announcementSet = state.Announcement.HasValue;
        if (!nonAnnHasOptions && annHasOptions && !announcementSet)
            throw new InvalidOperationException("Announcement required before rolling.");

        if (state.RollCount >= Definition.MaxRollsPerTurn)
            throw new InvalidOperationException("Max rolls per turn reached.");

        var dice = state.DiceArray
            .Select((d, i) => ((mask >> i) & 1) == 1 ? d : d.Roll(rng))
            .ToImmutableArray();

        return state.AfterRoll(dice);
    }

    private YambState ReduceAnnounce(YambState state, CategoryKey categoryKey)
    {
        if (state.RollCount != 0)
            throw new InvalidOperationException("Cannot announce after rolling.");

        var announcementColKey = Definition.ColumnDefinitions.First(cd => cd.Key.Value == "announcement").Key;
        var annCol = state.Columns.First(c => c.Key.Equals(announcementColKey));
        var alreadyScored = annCol.Categories.Any(c => c.Key.Equals(categoryKey) && c.Score.HasValue);
        if (alreadyScored)
            throw new InvalidOperationException("Category already scored in announcement column.");

        return state.AfterAnnounce(categoryKey);
    }

    private YambState ReduceFill(YambState state, ColumnKey columnKey, CategoryKey categoryKey)
    {
        var announcementColKey = Definition.ColumnDefinitions.First(cd => cd.Key.Value == "announcement").Key;
        var currentAnnouncement = state.Announcement;

        if (currentAnnouncement.HasValue && !columnKey.Equals(announcementColKey))
            throw new InvalidOperationException("Must score in announcement column.");
        if (currentAnnouncement.HasValue && !categoryKey.Equals(currentAnnouncement.Value))
            throw new InvalidOperationException("Must score announced category.");

        var column = state.GetColumn(columnKey);
        var updated = column.Fill(state.Dice, categoryKey);

        return state.AfterFill(columnKey, updated);
    }
}
