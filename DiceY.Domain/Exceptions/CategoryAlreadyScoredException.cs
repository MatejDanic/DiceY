using DiceY.Domain.Primitives;

namespace DiceY.Domain.Exceptions;

public sealed class CategoryAlreadyScoredException(CategoryKey key) : GameException($"{key}");
