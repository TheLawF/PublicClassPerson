using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Fictology.Util;
using UnityEngine;

namespace ClassPerson.GameSystem.Mahjong
{ 
    public class MahjongSystem
    {
        private MjScore _mjScore;
        private static readonly MahjongSystem _mahjongSystem;
        public static MahjongSystem Instance = _mahjongSystem ??= new MahjongSystem();
        private static readonly List<MahjongTile> Mahjongs = CreateMahjong();
        
        private static List<MahjongTile> CreateMahjong()
        { 
            var mahjongs = new List<MahjongTile>();
            
            mahjongs.AddRange(RankCard.AsSequence(Suit.Character));
            mahjongs.AddRange(RankCard.AsSequence(Suit.Circle));
            mahjongs.AddRange(RankCard.AsSequence(Suit.Bamboo));
            
            mahjongs.Add(MahjongTile.New(MahjongTile.East));
            mahjongs.Add(MahjongTile.New(MahjongTile.South));
            mahjongs.Add(MahjongTile.New(MahjongTile.West));
            mahjongs.Add(MahjongTile.New(MahjongTile.North));
            mahjongs.Add(MahjongTile.New(MahjongTile.White));
            mahjongs.Add(MahjongTile.New(MahjongTile.Green));
            mahjongs.Add(MahjongTile.New(MahjongTile.Red));

            return mahjongs;
        }

        private MahjongSystem()
        {
            
        }
        public MahjongTile Get(int index) => Mahjongs[index];

        public int AIDiscard()
        {
            return 0;
        }

        public Stack<MahjongTile> CreateWall()
        {
            var list = new List<MahjongTile>(136);
            foreach (var tile in Mahjongs)
            {
                list.Add(MahjongTile.New(tile.Value));
                list.Add(MahjongTile.New(tile.Value));
                list.Add(MahjongTile.New(tile.Value));
                list.Add(MahjongTile.New(tile.Value));
            }
            return list.Shuffle();
        }

        public bool CheckVictory(List<MahjongTile> handCards, List<MahjongMeld> melds, MahjongTile latestTile, out MjScore mjs)
        {
            _mjScore = MjConvert.CreateMjScore(handCards, melds, latestTile);
            _mjScore.Run();
            VictoryCheckingLog(_mjScore.GetErrorCode());
            mjs = _mjScore;
            return _mjScore.GetErrorCode() == 7;
        }

        public MjScore GetScore() => _mjScore;
        
        public bool CheckVictory(List<MahjongTile> handCards, List<MahjongMeld> melds, MahjongTile latestTile)
        {
            _mjScore = MjConvert.CreateMjScore(handCards, melds, latestTile);
            _mjScore.Run();
            VictoryCheckingLog(_mjScore.GetErrorCode());
            return _mjScore.GetErrorCode() == 7;
        }
        
        public bool CheckVictoryNoLog(List<MahjongTile> handCards, List<MahjongMeld> melds, MahjongTile latestTile)
        {
            _mjScore = MjConvert.CreateMjScore(handCards, melds, latestTile);
            _mjScore.Run();
            // VictoryCheckingLog(_mjScore.GetErrorCode());
            return _mjScore.GetErrorCode() == 7;
        }

        public IEnumerable<string> GetScoreList(List<MahjongTile> handCards, List<MahjongMeld> melds, MahjongTile latestTile)
            => !CheckVictory(handCards, melds, latestTile) 
                ? new List<string>() 
                : _mjScore.GetYakuNames().ToList().TakeWhile(s => s is not null);
        
        
        /// <summary>
        /// 获取麻将向听数
        /// </summary>
        /// <returns>向听数</returns>
        public int CalculateSteps(List<MahjongTile> cards)
        {
            var syantenCalculator = new Syanten();
            syantenCalculator.SetTehai(MjConvert.ConvertHandToMjs(cards));
            return syantenCalculator.AnySyanten(out _);
        }

        /// <summary>
        /// 获得当前手牌的有效进张列表
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public List<MahjongTile> GetUtilityTiles(List<MahjongTile> hand)
        {
            var utilities = new List<MahjongTile>();
            Mahjongs.ForEach(mahjong =>
            {
                var list = new List<MahjongTile>(hand);
                for (var i = 0; i < hand.Count; i++)
                {
                    list[i] = mahjong;
                    var stepBefore = CalculateSteps(hand);
                    var stepAfter = CalculateSteps(list);
                    if (stepAfter < stepBefore) utilities.Add(mahjong);
                }
            });
            return utilities;
        }

        public Hashtable GetTileRemaining(List<MahjongTile> wall, List<MahjongTile> utilities)
        {
            var hashtable = new Hashtable();
            foreach (var mahjong in utilities) hashtable.Add(mahjong, wall.Select(tile => tile == mahjong).Count());
            return hashtable;
        }

        public int EffectiveDiscardStrategy(List<MahjongTile> hand, List<MahjongMeld> melds, 
            ConcurrentDictionary<int, int> discardEfficiency = null, int discardIndex = -1)
        {
            if (CalculateSteps(hand) <= 0)
            {
                _mjScore = MjConvert.CreateMjScore(hand, melds, hand[discardIndex]);
                _mjScore.Run();
                if (discardEfficiency is not null) return discardEfficiency.First(pair => 
                    pair.Value == discardEfficiency.Max(kv => kv.Value)).Key;
                
                var eff = new ConcurrentDictionary<int, int>() {};
                eff.TryAdd(discardIndex, _mjScore.GetOyaTumo());
                return eff.First(pair => pair.Value == eff.Max(kv => kv.Value)).Key;
            }

            var hashtable = new ConcurrentDictionary<int, int>();
            var utilityTiles = GetUtilityTiles(hand);
            utilityTiles.ForEach(async util =>
            {
                var list = new List<MahjongTile>(hand);
                for (var i = 0; i < hand.Count; i++)
                {
                    list[i] = util;
                    var newUtil = GetUtilityTiles(list);
                    EffectiveDiscardStrategy(newUtil, melds, hashtable, i);
                    await UniTask.DelayFrame(1);
                }
            });
            return discardIndex;
        }

        private string GetSequenceInfo()
        {
            var sb = new StringBuilder();
            foreach (var i in _mjScore.GetKiriwake())
            {
                if (i % 2 == 0) sb.Append(MjConvert.ConvertToMahjong(i));
                sb.Append("的");
                if (i % 2 != 0)
                {
                    sb.Append(i switch
                    {
                        1 => "碰",
                        2 => "吃",
                        3 => "暗杠",
                        4 => "明杠",
                        5 => "加杠",
                        6 => "赤牌碰",
                        7 => "双赤牌碰",
                        8 => "赤牌吃",
                        9 => "雀头/对子",
                        10 => "暗刻",
                        11 => "顺子",
                        _ => "未定义"
                    });
                }
                if (i != _mjScore.GetKiriwake().Length - 1) sb.Append(",");
            }
            return sb.ToString();
        }
        
        private void VictoryCheckingLog(int errorCode)
        {
            Debug.Log(errorCode switch
            {
                0 => "未进行和牌判定",
                1 => "没有足够的14张或五搭手牌",
                2 => "和牌不在手牌内",
                3 => "诈胡",
                4 => "没有场风或自风",
                5 => "鸣牌立直",
                6 => "无役",
                7 => "和牌判定成功",
                _ => "未知错误"
            });
        }
        
        public string GetCheckingLog(int errorCode)
        {
            return errorCode switch
            {
                0 => "未进行和牌判定",
                1 => "没有足够的14张或五搭手牌",
                2 => "和牌不在手牌内",
                3 => "诈胡",
                4 => "没有场风或自风",
                5 => "鸣牌立直",
                6 => "无役",
                7 => "和牌判定成功",
                _ => "未知错误"
            };
        }
    }
}