using DiceY.Domain.Entities;

namespace DiceY.Domain.Delegates;

public delegate int CalculateScore(IReadOnlyList<Category> categories);
