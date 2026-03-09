using System.Collections.Generic;
using Fictology.Util;

namespace ClassPerson.GameSystem.Mahjong
{
    public class RankCard: MahjongTile
    {
        public RankCard(Suit suit, int value, MahjongTag mahjongTag = MahjongTag.Any) : base(suit, value, mahjongTag)
        {
            
        }
        
        public static List<RankCard> AsSequence(Suit suit)
        {
            var list = new List<RankCard>(9);
            if (suit.ContainsFlag(Suit.Character))
            {
                for (var i = 0; i < 9; i++) list.Add((RankCard)New(i));
            }

            else if (suit.ContainsFlag(Suit.Circle))
            {
                for (var i = 0; i < 9; i++) list.Add((RankCard)New(i + 9));
            }

            else if (suit.ContainsFlag(Suit.Bamboo))
            {
                for (var i = 0; i < 9; i++) list.Add((RankCard)New(i + 18));
            }
            return list;
        }

        public override MahjongTag GetTags() => MahjongTag;
    }
}