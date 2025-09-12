using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.ValueObjects;
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
        => throw new NotImplementedException();
}
