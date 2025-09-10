using DiceY.Domain.Primitives;

namespace DiceY.Domain.Exceptions;

public sealed class ColumnFillPolicyException(ColumnKey columnKey, CategoryKey categoryKey) : GameException($"{columnKey}, {categoryKey}") { }