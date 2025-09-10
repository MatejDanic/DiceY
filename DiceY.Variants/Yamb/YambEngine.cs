using DiceY.Domain.Entities;
using DiceY.Domain.Interfaces;
using DiceY.Domain.Primitives;
namespace DiceY.Variants.Yamb;

public sealed class YambEngine(IRollService rng, int diceCount = 5, int diceSides = 6) : IGameEngine<YambState>
{
    private readonly IRollService _rng = rng ?? new RandomRollService();

    public int DiceCount => diceCount;
    public int DiceSides => diceSides;
    public int MaxRollCount => 3;

    private const int _tripsBonus = 10;
    private const int _smallStraightScore = 35;
    private const int _largeStraightScore = 45;
    private const int _fullHouseBonus = 30;
    private const int _pokerBonus = 40;
    private const int _yambBonus = 50;

    private static readonly ColumnKey _down = new("down");
    private static readonly ColumnKey _up = new("up");
    private static readonly ColumnKey _free = new("free");
    private static readonly ColumnKey _announcement = new("announcement");

    private readonly IReadOnlyDictionary<ColumnKey, IOrderPolicy> _orderPolicies = new Dictionary<ColumnKey, Column.FillDirection>
    {

    };

    private static readonly CategoryKey _ones = new("ones");
    private static readonly CategoryKey _twos = new("twos");
    private static readonly CategoryKey _threes = new("threes");
    private static readonly CategoryKey _fours = new("fours");
    private static readonly CategoryKey _fives = new("fives");
    private static readonly CategoryKey _sixes = new("sixes");
    private static readonly CategoryKey _max = new("max");
    private static readonly CategoryKey _min = new("min");
    private static readonly CategoryKey _trips = new("trips");
    private static readonly CategoryKey _straight = new("straight");
    private static readonly CategoryKey _fullHouse = new("fullHouse");
    private static readonly CategoryKey _poker = new("poker");
    private static readonly CategoryKey _yamb = new("yamb");

    private static readonly CategoryKey[] _topSection = [_ones, _twos, _threes, _fours, _fives, _sixes];
    private static readonly CategoryKey[] _bottomSection = [_trips, _straight, _fullHouse, _poker, _yamb];

    public IReadOnlyDictionary<CategoryKey, IScoringRUle> Rules => new Dictionary<CategoryKey, IScoringRUle>
    {
        [_ones] = new FaceSum(1),
        [_twos] = new FaceSum(2),
        [_threes] = new FaceSum(3),
        [_fours] = new FaceSum(4),
        [_fives] = new FaceSum(5),
        [_sixes] = new FaceSum(6),
        [_max] = new Sum(),
        [_min] = new Sum(),
        [_trips] = new NOfAKind(3, _tripsBonus),
        [_straight] = new Pattern(
            new Dictionary<ISet<int>, int>
            {
                [new HashSet<int> { 1, 2, 3, 4, 5 }] = _smallStraightScore,
                [new HashSet<int> { 2, 3, 4, 5, 6 }] = _largeStraightScore
            }
        ),
        [_fullHouse] = new FullHouse(_fullHouseBonus),
        [_poker] = new NOfAKind(4, _pokerBonus),
        [_yamb] = new NOfAKind(5, _yambBonus)
    };


    public YambState Create()
    {
        var dice = Enumerable.Range(0, DiceCount).Select(_ => new Die(_config.DiceSides)).ToList();
        var columns = ColumnDirections
            .Select(kv => new Column(kv.Key, kv.Value, CreateCategories(), _config.Score))
            .ToList();
        return new YambState(dice, columns);
    }

    public YambState Reduce(YambState state, IGameCommand<YambState> action)
    {
        throw new NotImplementedException();
    }


    private static bool OnlyAnnouncementOptionsLeft(YambState state)
    {
        var annCol = state.Columns.FirstOrDefault(c => c.ColumnKey.Equals(_announcement));
        var anyNonAnn = state.Columns.Where(c => !c.ColumnKey.Equals(_announcement)).Any(c => c.GetAvailableCategories().Length > 0);
        var annAvail = annCol != null && annCol.GetAvailableCategories().Length > 0;
        return !anyNonAnn && annAvail;
    }

    private void Roll(IReadOnlyList<Die> dice, int mask)
    {
        for (int i = 0; i < dice.Count; i++)
            if (((mask >> i) & 1) == 1)
                dice[i].Roll(_rng);
    }

    private int ColumnScoreFunc(IReadOnlyDictionary<CategoryKey, Category> map)
    {
        int topSum = _topSection.Select(k => map[k]).Where(c => c.Score.HasValue).Sum(c => c.Score ?? 0);
        int bottomSum = _bottomSection.Select(k => map[k]).Where(c => c.Score.HasValue).Sum(c => c.Score ?? 0);
        int baseSum = (topSum >= topThreshold ? topSum + topBonus : topSum) + bottomSum;

        if (!map.TryGetValue(max, out var maxC) || !map.TryGetValue(min, out var minC) || !map.TryGetValue(ones, out var onesC))
            return baseSum;
        if (!maxC.Score.HasValue || !minC.Score.HasValue || !onesC.Score.HasValue)
            return baseSum;

        return baseSum + (maxC.Score.Value - minC.Score.Value) * onesC.Score.Value;
    }

}
