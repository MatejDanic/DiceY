using DiceY.Domain.Primitives;

namespace DiceY.Domain.Exceptions;

internal class CategoryAlreadyScoredException(CategoryKey key) : GameException($"{key}");
