using DiceY.Domain.Entities;
using DiceY.Domain.Primitives;

namespace DiceY.Domain.Interfaces;

public interface IOrderPolicy
{
    bool CanFill(IReadOnlyList<Category> categories, CategoryKey categoryKey);
}
