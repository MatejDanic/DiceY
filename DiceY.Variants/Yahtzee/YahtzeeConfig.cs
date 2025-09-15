using DiceY.Domain.Delegates;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.Variants.Shared.Policies;
using DiceY.Variants.Shared.Rules;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed record YahtzeeConfig
{
    private static readonly ColumnKey Main = new("main");

    private static readonly CategoryKey Ones = new("ones");
    private static readonly CategoryKey Twos = new("twos");
    private static readonly CategoryKey Threes = new("threes");
    private static readonly CategoryKey Fours = new("fours");
    private static readonly CategoryKey Fives = new("fives");
    private static readonly CategoryKey Sixes = new("sixes");
    private static readonly CategoryKey ThreeOfAKind = new("three_of_a_kind");
    private static readonly CategoryKey FourOfAKind = new("four_of_a_kind");
    private static readonly CategoryKey FullHouse = new("full_house");
    private static readonly CategoryKey SmallStraight = new("small_straight");
    private static readonly CategoryKey LargeStraight = new("large_straight");
    private static readonly CategoryKey Yahtzee = new("yahtzee");
    private static readonly CategoryKey Chance = new("chance");

    private static readonly ImmutableHashSet<CategoryKey> TopSection = [Ones, Twos, Threes, Fours, Fives, Sixes];
    private static readonly ImmutableHashSet<CategoryKey> BottomSection = [ThreeOfAKind, FourOfAKind, FullHouse, SmallStraight, LargeStraight, Yahtzee, Chance];
    private static readonly ImmutableArray<CategoryKey> CategoryOrder = [Ones, Twos, Threes, Fours, Fives, Sixes, ThreeOfAKind, FourOfAKind, FullHouse, SmallStraight, LargeStraight, Yahtzee, Chance];

    private const int FullHouseScore = 25;
    private const int SmallStraightScore = 30;
    private const int LargeStraightScore = 40;
    private const int YahtzeeScore = 50;

    private static readonly ImmutableDictionary<CategoryKey, IScoringRule> Rules = new Dictionary<CategoryKey, IScoringRule>
    {
        [Ones] = new FaceSum(1),
        [Twos] = new FaceSum(2),
        [Threes] = new FaceSum(3),
        [Fours] = new FaceSum(4),
        [Fives] = new FaceSum(5),
        [Sixes] = new FaceSum(6),
        [ThreeOfAKind] = new NOfAKind(3),
        [FourOfAKind] = new NOfAKind(4),
        [FullHouse] = new FullHouse(fixedScore: FullHouseScore),
        [SmallStraight] = new Straight(4, fixedScore: SmallStraightScore),
        [LargeStraight] = new Straight(5, fixedScore: LargeStraightScore),
        [Yahtzee] = new NOfAKind(5, fixedScore: YahtzeeScore),
        [Chance] = new Sum()
    }.ToImmutableDictionary();

    private const int TopSectionBonus = 35;
    private const int TopSectionBonusThreshold = 63;

    private static readonly CalculateScore CalculateScore = categories =>
    {
        var topSectionSum = categories.Where(c => c.Score.HasValue && TopSection.Contains(c.Key)).Sum(c => c.Score ?? 0);
        var bottomSectionSum = categories.Where(c => c.Score.HasValue && BottomSection.Contains(c.Key)).Sum(c => c.Score ?? 0);
        return topSectionSum + (topSectionSum >= TopSectionBonusThreshold ? TopSectionBonus : 0) + bottomSectionSum;
    };

    public static GameDefinition Build()
    {
        var columnDefinition = new ColumnDefinition(Main, new Free(), CalculateScore);
        var categoryDefinitions = CategoryOrder.Select(key => new CategoryDefinition(key, Rules[key]));
        return new GameDefinition(
            DiceCount: 5,
            DiceSides: 6,
            MaxRollsPerTurn: 3,
            ColumnDefinitions: [columnDefinition],
            CategoryDefinitions: [.. categoryDefinitions]
        );
    }
}
