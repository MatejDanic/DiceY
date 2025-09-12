namespace DiceY.Domain.Interfaces;

public interface IGameEngine<TState> where TState : IGameState
{
    TState Create();
    TState Reduce(TState state, IGameCommand<TState> action);
}