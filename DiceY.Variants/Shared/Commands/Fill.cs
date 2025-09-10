using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Variants.Shared.Commands;

public sealed record Fill(ColumnKey ColumnKey, CategoryKey CategoryKey) : IGameCommand<IGameState>;
