using Yamb.Domain.Entities;
using Yamb.Domain.Interfaces;

namespace Yamb.Domain.UnitTests.Entities;

public sealed class GameTests
{
    private sealed record TestState(int Value, bool Completed, int Score) : IGameState
    {
        public bool IsCompleted => Completed;
        public int TotalScore => Score;
    }

    private sealed record Inc(int Delta, int ScoreDelta) : IGameCommand<TestState>;
    private sealed record Complete : IGameCommand<TestState>;
    private sealed record ResetScore : IGameCommand<TestState>;
    private sealed record Noop : IGameCommand<TestState>;

    private sealed class TestRuleset : IRuleset<TestState>
    {
        public TestState Create() => new(0, false, 0);

        public TestState Reduce(TestState state, IGameCommand<TestState> action)
        {
            if (state.IsCompleted) return state;
            return action switch
            {
                Inc v => state with { Value = state.Value + v.Delta, Score = state.TotalScore + v.ScoreDelta },
                Complete => state with { Completed = true },
                ResetScore => state with { Score = 0 },
                _ => state
            };
        }

        public ReduceResult<TestState> TryReduce(TestState state, IGameCommand<TestState> action)
        {
            if (state.IsCompleted) return new(false, state, CommandFailureReason.GameCompleted);
            return action switch
            {
                Inc v => new(true, state with { Value = state.Value + v.Delta, Score = state.TotalScore + v.ScoreDelta }, CommandFailureReason.None),
                Complete => new(true, state with { Completed = true }, CommandFailureReason.None),
                ResetScore => new(true, state with { Score = 0 }, CommandFailureReason.None),
                _ => new(false, state, CommandFailureReason.UnknownCommand)
            };
        }
    }

    [Fact]
    public void Ctor_InitializesStateFromRuleset()
    {
        var g = new Game<TestState>(new TestRuleset());
        Assert.Equal(new TestState(0, false, 0), g.State);
    }

    [Fact]
    public void Apply_SingleCommand_UpdatesState()
    {
        var g = new Game<TestState>(new TestRuleset());
        var s = g.Apply(new Inc(2, 3));
        Assert.Equal(new TestState(2, false, 3), s);
        Assert.Equal(s, g.State);
    }

    [Fact]
    public void Apply_MultipleCommands_SequentiallyUpdates()
    {
        var g = new Game<TestState>(new TestRuleset());
        var s = g.Apply([new Inc(1, 1), new Inc(3, 2), new ResetScore(), new Inc(5, 5)]);
        Assert.Equal(new TestState(9, false, 5), s);
        Assert.Equal(s, g.State);
    }

    [Fact]
    public void Apply_UnknownCommand_DoesNotChangeState()
    {
        var g = new Game<TestState>(new TestRuleset());
        var before = g.State;
        var after = g.Apply(new Noop());
        Assert.Equal(before, after);
        Assert.Equal(after, g.State);
    }

    [Fact]
    public void Apply_AfterCompletion_IgnoredByRuleset()
    {
        var g = new Game<TestState>(new TestRuleset());
        g.Apply(new Inc(2, 2));
        g.Apply(new Complete());
        var before = g.State;
        g.Apply(new Inc(10, 10));
        Assert.Equal(before, g.State);
    }

    [Fact]
    public void Ctor_NullRuleset_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Game<TestState>(null));
    }
}