using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Rules;

public sealed class Straight : IScoringRule
{
    private readonly int _n;
    private readonly int _bonus;
    private readonly int _fixedScore;

    public Straight(int n, int bonus = 0, int fixedScore = 0)
    {
        if (n <= 1) throw new ArgumentOutOfRangeException(nameof(n), n, "N must be larger than 1.");
        _n = n;
        _bonus = bonus;
        _fixedScore = fixedScore;
    }

    public int GetScore(IReadOnlyList<Die> dice)
    {
        if (dice is null || dice.Count < _n) return 0;
        var vals = dice.Select(d => d.Value).Distinct().OrderBy(v => v).ToArray();
        if (vals.Length == 0) return 0;
        int best = 1, run = 1;
        for (int i = 1; i < vals.Length; i++)
        {
            if (vals[i] == vals[i - 1] + 1) { run++; best = Math.Max(best, run); }
            else run = 1;
        }
        if (best < _n) return 0;
        return _fixedScore > 0 ? _fixedScore : dice.Sum(d => d.Value) + _bonus;
    }
}
