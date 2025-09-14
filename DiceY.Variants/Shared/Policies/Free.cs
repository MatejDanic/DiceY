using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Variants.Shared.Policies;

public sealed class Free : IOrderPolicy
{
    public bool CanFill(IReadOnlyList<Category> categories, CategoryKey categoryKey)
    {
        if (categories is null || categories.Count == 0) return false;
        for (int i = 0; i < categories.Count; i++)
            if (categories[i].Key.Equals(categoryKey))
                return !categories[i].Score.HasValue;
        return false;
    }
}
