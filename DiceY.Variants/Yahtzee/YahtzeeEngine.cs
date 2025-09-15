using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
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

        var colDef = Definition.ColumnDefinitions.First();
        var cats = Definition.CategoryDefinitions.Select(catDef => new Category(catDef));
        var col = new Column(colDef, cats);

        return new YahtzeeState(dice, col);
    }

    public YahtzeeState Reduce(YahtzeeState state, IGameCommand<YahtzeeState> action)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(action);
        return action switch
        {
            Roll roll => ReduceRoll(state, roll.Mask),
            Fill fill => ReduceFill(state, fill.CategoryKey),
            _ => throw new NotSupportedException(action.GetType().Name)
        };
    }

    private YahtzeeState ReduceRoll(YahtzeeState state, int mask)
    {
        if ((mask >> Definition.DiceCount) != 0)
            throw new ArgumentOutOfRangeException(nameof(mask));
        if (state.RollCount >= Definition.MaxRollsPerTurn)
            throw new InvalidOperationException("Max rolls per turn reached.");

        var dice = state.DiceArray
            .Select((d, i) => ((mask >> i) & 1) == 1 ? d : d.Roll(rng))
            .ToImmutableArray();

        return state.AfterRoll(dice);
    }

    private YahtzeeState ReduceFill(YahtzeeState state, CategoryKey categoryKey)
    {
        var updated = state.Column.Fill(state.Dice, categoryKey);
        return state.AfterFill(updated);
    }
}
