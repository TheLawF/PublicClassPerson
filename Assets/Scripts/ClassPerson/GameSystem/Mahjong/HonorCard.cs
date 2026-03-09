namespace ClassPerson.GameSystem.Mahjong
{
    public class HonorCard: MahjongTile
    {
        public HonorCard(Suit suit, int value, MahjongTag mahjongTag = MahjongTag.Any) : base(suit, value, mahjongTag)
        {
        }

        public override MahjongTag GetTags() => MahjongTag;
    }
}