using DiceY.Domain.Delegates;
using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
using DiceY.Variants.Shared.Policies;
using DiceY.Variants.Shared.Rules;
using System.Collections.Immutable;
using System.Data;
using System.Threading;
using System.Threading.Channels;

namespace DiceY.Variants.Yahtzee;

public sealed class YahtzeeEngine : IGameEngine<YahtzeeState>
{
    public IRollService Rng => _rng;
    public int DiceCount => _diceCount;
    public int MaxRollCount { get; } = 3;
    public int DiceSides { get; } = 6;
    public IReadOnlyList<CategoryKey> CategoryOrder => _categoryOrder;
    public IReadOnlyList<ColumnKey> ColumnOrder => _columnOrder;
    public IReadOnlyDictionary<ColumnKey, IOrderPolicy> Policies => _policies;
    public IReadOnlyDictionary<CategoryKey, IScoringRule> Rules => _rules;

    private readonly int _diceCount;
    private readonly IRollService _rng;

    private const int _minDiceCount = 5;
    private const int _maxDiceCount = 6;

    public YahtzeeEngine(IRollService rng, int diceCount = 5)
    {
        ArgumentNullException.ThrowIfNull(rng, nameof(rng));
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

    private static readonly ImmutableArray<CategoryKey> _topSection = [_ones, _twos, _threes, _fours, _fives, _sixes];

    private static readonly ImmutableArray<CategoryKey> _bottomSection = [_threeOfAKind, _fourOfAKind, _fullHouse, _smallStraight, _largeStraight, _yahtzee, _chance];

    private static readonly ImmutableArray<CategoryKey> _allCategories = _topSection.AddRange(_bottomSection);

    private static readonly ImmutableArray<ColumnKey> _columns = [_main];

    private readonly ImmutableArray<CategoryKey> _categoryOrder = _allCategories;
    private readonly ImmutableArray<ColumnKey> _columnOrder = _columns;

    private readonly ImmutableDictionary<ColumnKey, IOrderPolicy> _policies = ImmutableDictionary<ColumnKey, IOrderPolicy>.Empty.Add(_main, new Free());

    private const int _fullHouseScore = 25;
    private const int _smallStraightScore = 30;
    private const int _largeStraightScore = 40;
    private const int _yahtzeeScore = 50;

    private readonly ImmutableDictionary<CategoryKey, IScoringRule> _rules = ImmutableDictionary.CreateRange(
        [
            new KeyValuePair<CategoryKey, IScoringRule>(_ones, new FaceSum(1)),
            new KeyValuePair<CategoryKey, IScoringRule>(_twos, new FaceSum(2)),
            new KeyValuePair<CategoryKey, IScoringRule>(_threes, new FaceSum(3)),
            new KeyValuePair<CategoryKey, IScoringRule>(_fours, new FaceSum(4)),
            new KeyValuePair<CategoryKey, IScoringRule>(_fives, new FaceSum(5)),
            new KeyValuePair<CategoryKey, IScoringRule>(_sixes, new FaceSum(6)),
            new KeyValuePair<CategoryKey, IScoringRule>(_threeOfAKind, new NOfAKind(3)),
            new KeyValuePair<CategoryKey, IScoringRule>(_fourOfAKind, new NOfAKind(4)),
            new KeyValuePair<CategoryKey, IScoringRule>(_fullHouse, new FullHouse(0, _fullHouseScore)),
            new KeyValuePair<CategoryKey, IScoringRule>(_smallStraight, new Straight(4, _smallStraightScore)),
            new KeyValuePair<CategoryKey, IScoringRule>(_largeStraight, new Straight(5, _largeStraightScore)),
            new KeyValuePair<CategoryKey, IScoringRule>(_yahtzee, new NOfAKind(5, 0, _yahtzeeScore)),
            new KeyValuePair<CategoryKey, IScoringRule>(_chance, new Sum())
        ]);

    public YahtzeeState Create()
    {
        var dice = Enumerable.Range(0, DiceCount).Select(_ => new Die(DiceSides)).ToImmutableArray();
        var categories = _categoryOrder.Select(k => new Category(k, _rules[k])).ToImmutableArray();
        var column = new Column(_main, new Free(), _calculateScore, categories);
        return new YahtzeeState(dice, [column]);
    }

    public YahtzeeState Reduce(YahtzeeState state, IGameCommand<YahtzeeState> action)
    {
        throw new NotImplementedException();
    }

    private const int _topSectionBonus = 35;
    private const int _topSectionBonusThreshold = 63;

    private readonly CalculateScore _calculateScore = (IReadOnlyList<Category> categories) =>
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
