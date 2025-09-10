using DiceY.Domain.Interfaces;

namespace DiceY.Variants.Shared.Commands;

public sealed record Roll(int Mask) : IGameCommand<IGameState>;

