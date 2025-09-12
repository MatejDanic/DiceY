using System.Collections.Immutable;

namespace DiceY.Domain.ValueObjects;

public sealed record GameDefinition(
    int DiceCount,
    int DiceSides,
    int MaxRollsPerTurn,
    ImmutableArray<ColumnDefinition> ColumnDefinitions,
    ImmutableArray<CategoryDefinition> CategoryDefinitions);
