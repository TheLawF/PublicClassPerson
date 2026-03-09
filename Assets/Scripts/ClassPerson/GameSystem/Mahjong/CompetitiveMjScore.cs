namespace ClassPerson.GameSystem.Mahjong
{
    /// <summary>
    /// 国标麻将番种计算
    /// Orphan -> 幺九牌
    /// Tendon -> 筋牌
    /// Eyes/Pairs -> 雀头/对子
    /// EmbedTile -> 坎张
    /// </summary>
    public class CompetitiveMjScore
    {
        public static readonly Scoring FlushHeptaEyes = new ("连七对", 64);
        public static readonly Scoring HeptaEyes = new ("七对", 24);
        public static readonly Scoring HeptaTendon = new ("七星不靠", 24);
        
        public static readonly Scoring FullBig = new ("全大", 24);
        public static readonly Scoring FullMid = new ("全小", 24);
        public static readonly Scoring FullSmall = new ("全小", 24);
        public static readonly Scoring FullEven = new ("全双刻", 24);

        public static readonly Scoring FullDragon = new ("清龙", 16);
        public static readonly Scoring FullOrphan = new ("全不靠", 12);
        public static readonly Scoring TendonDragon = new ("组合龙", 12);
        public static readonly Scoring Symmetry = new ("推不倒", 8);

        public static readonly Scoring Connective6 = new ("连六", 1);
        
        private int[] _normalized = new int[42];
        private int[] _meldsType = new int[10];
        
    }

    public class CustomScores
    {
        public static readonly Scoring HeptaEmbedTile = new("七连坎", 64);
        public static readonly Scoring EmbedTriplets = new("你是搞嵌入式开发的吗", 24);
        public static readonly Scoring FullEmbedTiles = new ("非洲大连坎", 16);
    }
    public record Scoring(string Name, int Value) { }
}