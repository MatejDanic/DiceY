using DiceY.Domain.Interfaces;

namespace DiceY.Domain.Entities;

public sealed class Game<TState> where TState : IGameState
{
    private readonly IGameEngine<TState> _engine;
    public TState State { get; private set; }

    public Game(IGameEngine<TState> engine)
    {
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        _engine = engine;
        State = _engine.Create();
    }

    public TState Apply(IGameCommand<TState> command) => State = _engine.Reduce(State, command);

    public TState Apply(IEnumerable<IGameCommand<TState>> commands)
    {
        foreach (var command in commands) State = _engine.Reduce(State, command);
        return State;
    }
}