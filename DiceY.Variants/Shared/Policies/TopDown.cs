using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Variants.Shared.Policies;

public sealed class TopDown : IOrderPolicy
{
    public bool CanFill(IReadOnlyList<Category> categories, CategoryKey categoryKey)
    {
        throw new NotImplementedException();
    }
}
