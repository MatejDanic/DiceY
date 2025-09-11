using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Rules;

public sealed class FullHouse : IScoringRule
{
    private readonly int _bonus;
    private readonly int _fixedScore;

    public FullHouse(int bonus = 0, int fixedScore = 0)
    {
        _bonus = bonus;
        _fixedScore = fixedScore;
    }

    public int GetScore(IReadOnlyList<Die> dice)
    {
        if (dice is null || dice.Count < 3 + 2) return 0;
        var groups = dice.GroupBy(d => d.Value).Select(g => (Face: g.Key, Count: g.Count())).ToArray();
        var triples = groups.Where(t => t.Count >= 3).Select(t => t.Face).ToArray();
        if (triples.Length == 0) return 0;
        var pairs = groups.Where(t => t.Count >= 2).Select(t => t.Face).ToArray();
        if (pairs.Length == 0) return 0;
        int best = 0;
        foreach (var f3 in triples)
            foreach (var f2 in pairs)
                if (f2 != f3)
                    best = Math.Max(best, 3 * f3 + 2 * f2);
        if (best == 0) return 0;
        return _fixedScore > 0 ? _fixedScore : best + _bonus;
    }
}