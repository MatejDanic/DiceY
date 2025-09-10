using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Rules;

public sealed class Sum : IScoringRUle
{
    public int GetScore(IReadOnlyList<Die> dice)
    {
        if (dice is null || dice.Count == 0) return 0;
        return dice.Sum(d => d.Value); ;
    }
}
