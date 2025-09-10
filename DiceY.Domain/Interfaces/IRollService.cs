using System.Security.Cryptography;

namespace DiceY.Domain.Interfaces;

public interface IRollService
{
    int NextRoll(int sides)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sides, 1);
        return RandomNumberGenerator.GetInt32(1, sides + 1);
    }
}