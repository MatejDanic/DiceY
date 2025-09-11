using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Domain.ValueObjects;

public readonly record struct GameConfig(
    int DiceSides,
    int DiceCount,
    int MaxRollCount,
    IReadOnlyList<CategoryKey> CategoryOrder,
    IReadOnlyList<ColumnKey> ColumnOrder,
    IReadOnlyDictionary<ColumnKey, IOrderPolicy> Policies,
    IReadOnlyDictionary<CategoryKey, IScoringRule> Rules) { }