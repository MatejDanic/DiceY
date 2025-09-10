using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Variants.Shared.Rules;
using DiceY.Domain.Delegates;
using DiceY.Variants.Shared.Policies;
using System.Collections.Immutable;

namespace DiceY.Variants.Yahtzee;

public sealed class YahtzeeEngine : IGameEngine<YahtzeeState>
{
    public IRollService Rng => _rng;
    public int DiceCount => _diceCount;
    public int MaxRollCount { get; } = 3;

    private const int _minDiceCount = 5;
    private const int _maxDiceCount = 6;
    private readonly int _diceCount;
    private readonly IRollService _rng;

    public YahtzeeEngine(IRollService rng, int diceCount = 5)
    {
        ArgumentNullException.ThrowIfNull(nameof(rng));
        ArgumentOutOfRangeException.ThrowIfLessThan(diceCount, _minDiceCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(diceCount, _maxDiceCount);
        _rng = rng;
        _diceCount = diceCount;
    }

    private static readonly ColumnKey _main = new("main");

    private static readonly CategoryKey _ones = new("ones");
    private static readonly CategoryKey _twos = new("twos");
    private static readonly CategoryKey _threes = new("threes");
    private static readonly CategoryKey _fours = new("fours");
    private static readonly CategoryKey _fives = new("fives");
    private static readonly CategoryKey _sixes = new("sixes");
    private static readonly CategoryKey _threeOfAKind = new("three_of_a_kind");
    private static readonly CategoryKey _fourOfAKind = new("four_of_a_kind");
    private static readonly CategoryKey _fullHouse = new("full_house");
    private static readonly CategoryKey _smallStraight = new("small_straight");
    private static readonly CategoryKey _largeStraight = new("large_straight");
    private static readonly CategoryKey _yahtzee = new("yahtzee");
    private static readonly CategoryKey _chance = new("chance");

    private static readonly IReadOnlyList<CategoryKey> _topSection = [_ones, _twos, _threes, _fours, _fives, _sixes];
    private static readonly IReadOnlyList<CategoryKey> _bottomSection = [_threeOfAKind, _fourOfAKind, _fullHouse, _smallStraight, _largeStraight, _yahtzee, _chance];

    public SortedSet<ColumnKey> Columns = [_main];
    public SortedSet<CategoryKey> CategoryKeys = [_ones, _twos, _threes, _fours, _fives, _sixes, _threeOfAKind, _fourOfAKind, _fullHouse, _smallStraight, _largeStraight, _yahtzee, _chance];

    private const int _topSectionBonus = 35;
    private const int _topSectionBonusThreshold = 63;

    private const int _fullHouseScore = 25;
    private const int _smallStraightScore = 30;
    private const int _largeStraightScore = 40;
    private const int _yahtzeeScore = 50;

    public IReadOnlyDictionary<ColumnKey, IOrderPolicy> Policies => throw new NotImplementedException();

    public IReadOnlyDictionary<CategoryKey, IScoringRule> Rules => new Dictionary<CategoryKey, IScoringRule>
    {
        [_ones] = new FaceSum(1),
        [_twos] = new FaceSum(2),
        [_threes] = new FaceSum(3),
        [_fours] = new FaceSum(4),
        [_fives] = new FaceSum(5),
        [_sixes] = new FaceSum(6),
        [_threeOfAKind] = new NOfAKind(3),
        [_fourOfAKind] = new NOfAKind(4),
        [_fullHouse] = new FullHouse(0, _fullHouseScore),
        [_smallStraight] = new Straight(4, _smallStraightScore),
        [_largeStraight] = new Straight(5, _largeStraightScore),
        [_yahtzee] = new NOfAKind(5, 0, _yahtzeeScore),
        [_chance] = new Sum()
    };

    public int DiceSides => throw new NotImplementedException();

    public int GetTotalScore => throw new NotImplementedException();


    public YahtzeeState Create()
    {
        var dice = Enumerable.Range(0, DiceCount).Select(_ => new Die(DiceSides)).ToList();
        var categories = CategoryKeys.Select(c => new Category(c, Rules[c])).ToImmutableSortedSet();
        var column = new Column(_main, new Free(), _calculateScore, categories);
        return new YahtzeeState(dice, column);
    }

    public YahtzeeState Reduce(YahtzeeState state, IGameCommand<YahtzeeState> action)
    {
        throw new NotImplementedException();
    }

    private CalculateScore _calculateScore = (IReadOnlyList<Category> categories) =>
    {
        int topSectionSum = categories.Where(c => c.Score.HasValue && _topSection.Contains(c.Key)).Sum(c => c.Score ?? 0);
        int bottomSectionSum = categories.Where(c => c.Score.HasValue && _bottomSection.Contains(c.Key)).Sum(c => c.Score ?? 0);
        return topSectionSum + (topSectionSum >= _topSectionBonusThreshold ? _topSectionBonus : 0) + bottomSectionSum;
    };

    private Die[] Roll(IReadOnlyList<Die> dice, int mask)
    {
        var arr = dice.ToArray();
        int maxMask = (1 << arr.Length) - 1;
        if ((mask & ~maxMask) != 0) throw new ArgumentOutOfRangeException(nameof(mask));
        for (int i = 0; i < arr.Length; i++)
            if ((mask >> i & 1) == 1)
                arr[i].Roll(_rng);
        return arr;
    }
    
}
