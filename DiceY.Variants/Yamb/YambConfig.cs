using DiceY.Domain.Delegates;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Domain.ValueObjects;
using DiceY.Variants.Shared.Policies;
using DiceY.Variants.Shared.Rules;
using System.Collections.Immutable;

namespace DiceY.Variants.Yamb;

public sealed record YambConfig
{
    private static readonly ColumnKey Down = new("down");
    private static readonly ColumnKey Up = new("up");
    private static readonly ColumnKey Free = new("free");
    private static readonly ColumnKey Announcement = new("announcement");

    private static readonly CategoryKey Ones = new("ones");
    private static readonly CategoryKey Twos = new("twos");
    private static readonly CategoryKey Threes = new("threes");
    private static readonly CategoryKey Fours = new("fours");
    private static readonly CategoryKey Fives = new("fives");
    private static readonly CategoryKey Sixes = new("sixes");
    private static readonly CategoryKey Max = new("max");
    private static readonly CategoryKey Min = new("min");
    private static readonly CategoryKey Trips = new("trips");
    private static readonly CategoryKey Straight = new("straight");
    private static readonly CategoryKey FullHouse = new("full_house");
    private static readonly CategoryKey Poker = new("poker");
    private static readonly CategoryKey Yamb = new("yamb");

    private static readonly ImmutableHashSet<CategoryKey> TopSection = [Ones, Twos, Threes, Fours, Fives, Sixes];

    private static readonly ImmutableHashSet<CategoryKey> BottomSection = [Trips, Straight, FullHouse, Poker, Yamb];

    private static readonly ImmutableArray<CategoryKey> CategoryOrder =
    [
        Ones, Twos, Threes, Fours, Fives, Sixes,
        Max, Min,
        Trips, Straight, FullHouse, Poker, Yamb
    ];

    private static readonly ImmutableArray<ColumnKey> ColumnOrder = [Down, Up, Free, Announcement];

    private static readonly ImmutableDictionary<ColumnKey, IOrderPolicy> Policies =
        new Dictionary<ColumnKey, IOrderPolicy>
        {
            [Down] = new TopDown(),
            [Up] = new BottomUp(),
            [Free] = new Free(),
            [Announcement] = new Free()
        }.ToImmutableDictionary();

    private const int TripsBonus = 10;
    private const int SmallStraightScore = 35;
    private const int LargeStraightScore = 45;
    private const int FullHouseBonus = 30;
    private const int PokerBonus = 40;
    private const int YambBonus = 50;

    private static readonly ImmutableDictionary<CategoryKey, IScoringRule> Rules =
        new Dictionary<CategoryKey, IScoringRule>
        {
            [Ones] = new FaceSum(1),
            [Twos] = new FaceSum(2),
            [Threes] = new FaceSum(3),
            [Fours] = new FaceSum(4),
            [Fives] = new FaceSum(5),
            [Sixes] = new FaceSum(6),
            [Max] = new Sum(),
            [Min] = new Sum(),
            [Trips] = new NOfAKind(3, bonus: TripsBonus),
            [Straight] = new Pattern(new Dictionary<IReadOnlySet<int>, int>
            {
                [new HashSet<int> { 1, 2, 3, 4, 5 }] = SmallStraightScore,
                [new HashSet<int> { 2, 3, 4, 5, 6 }] = LargeStraightScore
            }),
            [FullHouse] = new FullHouse(FullHouseBonus),
            [Poker] = new NOfAKind(4, bonus: PokerBonus),
            [Yamb] = new NOfAKind(5, bonus: YambBonus)
        }.ToImmutableDictionary();

    private const int TopSectionBonus = 35;
    private const int TopSectionBonusThreshold = 63;

    private static readonly CalculateScore _calculateScore = categories =>
    {
        var map = categories.ToDictionary(c => c.Key, c => c);
        var topSectionSum = TopSection.Where(k => map.TryGetValue(k, out var c) && c.Score.HasValue).Sum(k => map[k].Score ?? 0);
        var bottomSectionSum = BottomSection.Where(k => map.TryGetValue(k, out var c) && c.Score.HasValue).Sum(k => map[k].Score ?? 0);
        var baseSum = topSectionSum + (topSectionSum > TopSectionBonusThreshold ? TopSectionBonus : 0) + bottomSectionSum;
        if (!map.TryGetValue(Max, out var maxC) || !map.TryGetValue(Min, out var minC) || !map.TryGetValue(Ones, out var onesC))
            return baseSum;
        if (!maxC.Score.HasValue || !minC.Score.HasValue || !onesC.Score.HasValue)
            return baseSum;
        return baseSum + (maxC.Score.Value - minC.Score.Value) * onesC.Score.Value;
    };

    public static GameDefinition Build()
    {
        var categoryDefinitions = CategoryOrder.Select(key => new CategoryDefinition(key, Rules[key]));
        var columnDefinitions = ColumnOrder.Select(key => new ColumnDefinition(key, Policies[key], _calculateScore));

        return new GameDefinition(
            DiceCount: 5,
            DiceSides: 6,
            MaxRollsPerTurn: 3,
            ColumnDefinitions: [.. columnDefinitions],
            CategoryDefinitions: [.. categoryDefinitions]
        );
    }
}
