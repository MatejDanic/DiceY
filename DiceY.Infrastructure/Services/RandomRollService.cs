using DiceY.Domain.Interfaces;
using System.Security.Cryptography;

namespace DiceY.Infrastructure.Services;

public sealed class RandomRollService : IRollService
{
    public int NextRoll(int sides)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sides, 1);
        return RandomNumberGenerator.GetInt32(1, sides + 1);
    }
}
