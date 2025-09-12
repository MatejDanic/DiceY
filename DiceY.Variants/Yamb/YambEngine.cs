using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.ValueObjects;
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
        => throw new NotImplementedException();
}
