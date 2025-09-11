using DiceY.Domain.Interfaces;

namespace DiceY.TestUtil;

public sealed class FixedRollService(IEnumerable<int> values) : IRollService
{
    private readonly Queue<int> _values = new(values);

    public int NextRoll(int sides)
    {
        return _values.Count > 0 ? _values.Dequeue() : sides;
    }
}
