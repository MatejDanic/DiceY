using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Rules;

public sealed class Pattern : IScoringRUle
{
    private readonly IReadOnlyDictionary<ISet<int>, int> _patterns;

    public Pattern(IReadOnlyDictionary<ISet<int>, int> patterns)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(patterns));
        _patterns = patterns;
    }

    public int GetScore(IReadOnlyList<Die> dice)
    {
        if (dice is null || dice.Count == 0) return 0;
        var present = dice.Select(d => d.Value).ToHashSet();
        var best = 0;
        foreach (var kv in _patterns)
            if (kv.Key.All(present.Contains) && kv.Value > best)
                best = kv.Value;
        return best;
    }
}
