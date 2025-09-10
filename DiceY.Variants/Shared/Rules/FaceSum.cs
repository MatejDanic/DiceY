using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Rules;

public sealed class FaceSum : IScoringRUle
{
    private readonly int _face;
    public FaceSum(int face)
    {
        if (face < 0) throw new ArgumentOutOfRangeException(nameof(face), face, "Face must be larger than 0.");
        _face = face;
    }


    public int GetScore(IReadOnlyList<Die> dice)
    {
        if (dice is null || dice.Count == 0) return 0;
        return dice.Where(d => d.Value == _face).Sum(d => d.Value);
    }
}
