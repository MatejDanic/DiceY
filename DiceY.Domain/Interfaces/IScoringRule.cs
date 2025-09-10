using DiceY.Domain.Entities;

namespace DiceY.Domain.Interfaces;

public interface IScoringRule
{
    int GetScore(IReadOnlyList<Die> dice);
}
