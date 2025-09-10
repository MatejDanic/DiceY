using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Variants.Yamb.Commands;

public sealed record Announce(CategoryKey CategoryKey) : IGameCommand<YambState>;
