using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Rules;

public sealed class NOfAKind : IScoringRUle
{
    private readonly int _n;
    private readonly int _bonus;
    private readonly int _fixedScore;

    public NOfAKind(int n, int bonus = 0, int fixedScore = 0)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), n, "N must be higher than 0.");
        _n = n;
        _bonus = bonus;
        _fixedScore = fixedScore;
    }

    public int GetScore(IReadOnlyList<Die> dice)
    {
        if (dice is null || dice.Count < _n) return 0;
        var bestFace = dice
            .GroupBy(d => d.Value)
            .Where(g => g.Count() >= _n)
            .Select(g => g.Key)
            .DefaultIfEmpty(0)
            .Max();
        return _fixedScore > 0 ? _fixedScore : (_n * bestFace) + _bonus;
    }
}
