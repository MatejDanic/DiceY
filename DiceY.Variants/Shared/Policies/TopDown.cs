using System.Linq;
using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;

namespace DiceY.Variants.Shared.Policies;

public sealed class TopDown : IOrderPolicy
{
    public bool CanFill(IReadOnlyList<Category> categories, CategoryKey categoryKey)
    {
        if (categories is null || categories.Count == 0) return false;
        var target = categories.Select((c, i) => (c, i)).FirstOrDefault(x => x.c.Key.Equals(categoryKey));
        if (target.c is null) return false;
        if (target.c.Score.HasValue) return false;
        var firstUnfilled = Enumerable.Range(0, categories.Count).FirstOrDefault(i => !categories[i].Score.HasValue);
        return target.i == firstUnfilled;
    }
}
