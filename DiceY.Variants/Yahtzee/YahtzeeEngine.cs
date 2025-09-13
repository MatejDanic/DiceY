using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.ValueObjects;
using DiceY.Variants.Shared.Commands;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed class YahtzeeEngine(IRollService rng, GameDefinition? definition = null) : IGameEngine<YahtzeeState>
{
    public GameDefinition Definition { get; } = definition ?? YahtzeeConfig.Build();

    public YahtzeeState Create()
    {
        var dice = Enumerable.Range(0, Definition.DiceCount)
            .Select(_ => new Die(Definition.DiceSides))
            .ToImmutableArray();

        var cols = Definition.ColumnDefinitions
            .Select(colDef =>
            {
                var cats = Definition.CategoryDefinitions.Select(catDef => new Category(catDef));
                return new Column(colDef, cats);
            })
            .ToImmutableArray();

        return new YahtzeeState(dice, cols);
    }

    public YahtzeeState Reduce(YahtzeeState state, IGameCommand<YahtzeeState> action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);

        switch (action)
        {
            case Roll roll:
                {
                    if (state.RollCount >= Definition.MaxRollsPerTurn)
                        throw new InvalidOperationException("Max rolls per turn reached.");

                    var mask = roll.Mask;
                    var dice = state.Dice
                        .Select((d, i) => ((mask >> i) & 1) == 1 ? d : d.Roll(rng))
                        .ToImmutableArray();

                    return new YahtzeeState(dice, state.Columns.ToImmutableArray(), state.RollCount + 1);
                }
            case Fill fill:
                {
                    var cols = state.Columns.ToImmutableArray();
                    var index = state.Columns
                        .Select((c, i) => (c, i))
                        .FirstOrDefault(x => x.c.Key.Equals(fill.ColumnKey)).i;
                    if (index == default && !state.Columns[0].Key.Equals(fill.ColumnKey))
                        throw new KeyNotFoundException($"{fill.ColumnKey}");

                    var updated = cols[index].Fill(state.Dice, fill.CategoryKey);
                    cols = cols.SetItem(index, updated);

                    var resetDice = Enumerable.Range(0, Definition.DiceCount)
                        .Select(_ => new Die(Definition.DiceSides))
                        .ToImmutableArray();

                    return new YahtzeeState(resetDice, cols, 0);
                }
            default:
                throw new NotSupportedException(action.GetType().Name);
        }
    }
}