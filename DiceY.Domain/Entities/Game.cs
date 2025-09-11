using DiceY.Domain.Interfaces;

namespace DiceY.Domain.Entities;

public sealed class Game<TState> where TState : IGameState
{
    private readonly IGameEngine<TState> _engine;
    public TState State { get; }

    public Game(IGameEngine<TState> engine)
    {
        ArgumentNullException.ThrowIfNull(engine, nameof(engine));
        _engine = engine;
        State = _engine.Create();
    }

    private Game(IGameEngine<TState> engine, TState state)
    {
        _engine = engine;
        State = state;
    }

    public Game<TState> Apply(IGameCommand<TState> command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        var newState = _engine.Reduce(State, command);
        return new Game<TState>(_engine, newState);
    }

    public Game<TState> Apply(IEnumerable<IGameCommand<TState>> commands)
    {
        ArgumentNullException.ThrowIfNull(commands, nameof(commands));
        var s = State;
        foreach (var command in commands) s = _engine.Reduce(s, command);
        return new Game<TState>(_engine, s);
    }
}
