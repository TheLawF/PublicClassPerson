namespace ClassPerson.GameSystem.Mahjong
{
    public interface IMahjongGroup
    {
        int Count();

        public static IMahjongGroup Create(params MahjongTile[] values)
        {
            return values.Length switch
            {
                1 => values[0],
                2 => PairGroup.New(values[0]),
                3 => values[0] + 1 == values[1] && values[1] + 1 == values[2] ? FlushGroup.New(values[0]) : StraightGroup.New(values[0]),
                4 => QuadGroup.New(values[0]),
                _ => new ExceptionGroup(),
            };
        }

        public static IMahjongGroup CreateAny(params MahjongTile[] values)
        {
            return values.Length switch
            {
                1 => AnyGroup.New(values[0], 1),
                2 => AnyGroup.New(values[0], 2),
                3 => values[0] + 1 == values[1] && values[1] + 1 == values[2] ? FlushGroup.New(values[0]) : StraightGroup.New(values[0]),
                _ => new ExceptionGroup(),
            };
        }
    }

    public record ExceptionGroup() : IMahjongGroup
    {
        public int Count() => -1;
    }
    public record PairGroup(MahjongTile Value): IMahjongGroup
    {
        public static PairGroup New(MahjongTile mahjong) => new (mahjong);
        public int Count() => 2;
    }

    public record AnyGroup(MahjongTile Value, int CurrentCount) : IMahjongGroup
    {
        public static AnyGroup New(MahjongTile value, int currentCount) => new AnyGroup(value, currentCount);
        public int Count() => -1;
    }
    public record StraightGroup(MahjongTile Value): IMahjongGroup
    {
        public static StraightGroup New(MahjongTile mahjong) => new (mahjong);
        public int Count() => 3;
    }
    public record FlushGroup(MahjongTile Value): IMahjongGroup
    {
        public static FlushGroup New(MahjongTile mahjong) => new (mahjong);
        public int Count() => 3;
    }
    public record QuadGroup(MahjongTile Value): IMahjongGroup
    {
        public static QuadGroup New(MahjongTile mahjong) => new (mahjong);
        public int Count() => 4;
    }
}