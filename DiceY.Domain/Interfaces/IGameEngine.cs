using DiceY.Domain.Primitives;

namespace DiceY.Domain.Interfaces;

public interface IGameEngine<TState> where TState : IGameState
{
    IRollService Rng { get; }
    int DiceCount { get; }
    int DiceSides { get; }
    int MaxRollCount { get; }
    ImmutableArray<CategoryKey> CategoryOrder { get; }
    SortedSet<ColumnKey> ColumnOrder { get; }
    IReadOnlyDictionary<ColumnKey, IOrderPolicy> Policies { get; }
    IReadOnlyDictionary<CategoryKey, IScoringRule> Rules { get; }
    TState Create();
    TState Reduce(TState state, IGameCommand<TState> action);
}