namespace DiceY.Domain.Interfaces;

public interface IGameCommand<in TState> where TState : IGameState { }
