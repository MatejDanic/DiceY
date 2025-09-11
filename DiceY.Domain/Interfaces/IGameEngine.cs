using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;

namespace DiceY.Domain.Interfaces;

public interface IGameEngine<TState> where TState : IGameState
{
    GameConfig Config { get; }
    TState Create();
    TState Reduce(TState state, IGameCommand<TState> action);
}