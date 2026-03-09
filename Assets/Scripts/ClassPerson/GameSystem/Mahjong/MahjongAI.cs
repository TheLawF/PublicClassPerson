using System;
using System.Collections.Generic;
using System.Linq;
using Fictology.Util;

namespace ClassPerson.GameSystem.Mahjong
{
    /// <summary>
    /// 麻将AI - 极致牌效率
    /// 专注于最快和牌，不考虑防守
    /// </summary>
    public class MahjongAI
    {
        private static MahjongAI _ai;
        public static MahjongAI Instance => _ai ??= new MahjongAI();
        
        private readonly MahjongSystem _mahjongSystem;
        private readonly Syanten _syantenCalculator;
        
        // 牌的价值评估权重
        private class TileEvaluation
        {
            public MahjongTile Tile { get; set; }
            public int Utility { get; set; }        // 进张改进数
            public int ShapeValue { get; set; }     // 形状价值
            public int Safety { get; set; }         // 安全性（暂时不用）
            public int TotalScore { get; set; }     // 总分
        }
        
        public MahjongAI()
        {
            _mahjongSystem = MahjongSystem.Instance;
            _syantenCalculator = new Syanten();
        }
        
        /// <summary>
        /// 主AI入口 - 获取最佳出牌
        /// </summary>
        public int GetBestDiscard(List<MahjongTile> hand, List<MahjongMeld> melds = null)
        {
            if (hand == null || hand.Count == 0) return 0;
            if (melds == null) melds = new List<MahjongMeld>();
            
            // 检查是否已经和牌
            if (_mahjongSystem.CheckVictoryNoLog(hand, melds, hand[0]))
            {
                return 0; // 应该和牌，这里返回0
            }
            
            // 如果只剩1张牌，只能打这张
            if (hand.Count == 1) return 0;
            
            // 计算当前向听数
            int currentSyanten = CalculateSyanten(hand, melds);
            
            // 如果已经是听牌状态，返回听牌打牌策略
            if (currentSyanten <= 0)
            {
                return GetTenpaiDiscard(hand, melds);
            }
            
            // 评估每张候选牌的效率
            var evaluations = EvaluateAllDiscards(hand, melds, currentSyanten);
            
            // 如果没有有效评估，返回第一张牌
            if (evaluations.Count == 0) return 0;
            
            // 选择总分最高的牌打出
            var bestDiscard = evaluations
                .OrderByDescending(e => e.TotalScore)
                .ThenByDescending(e => e.ShapeValue)
                .First();
                
            return hand.IndexOf(bestDiscard.Tile);
        }
        
        /// <summary>
        /// 计算手牌向听数（考虑副露）
        /// </summary>
        private int CalculateSyanten(List<MahjongTile> hand, List<MahjongMeld> melds)
        {
            if (hand == null) return 8; // 最大向听数
            
            // 将手牌转换为Syanten格式
            var tehai = ConvertHandToTehai(hand);
            _syantenCalculator.Clear();
            _syantenCalculator.SetTehai(tehai);
            _syantenCalculator.SetFuurosuu(melds?.Count ?? 0);
            
            return _syantenCalculator.AnySyanten(out _);
        }
        
        /// <summary>
        /// 评估所有可能出牌
        /// </summary>
        private List<TileEvaluation> EvaluateAllDiscards(List<MahjongTile> hand, List<MahjongMeld> melds, int currentSyanten)
        {
            var evaluations = new List<TileEvaluation>();
            
            // 统计手牌中每种牌的数量
            var tileCounts = CountTiles(hand);
            
            for (int i = 0; i < hand.Count; i++)
            {
                var tile = hand[i];
                
                // 跳过孤立的幺九牌和字牌（这些牌通常应该保留）
                TileEvaluation eval;
                if (IsIsolatedYaochu(tile, hand, tileCounts))
                {
                    // 但如果是单张无用字牌，可能应该打掉
                    if (IsUselessHonor(tile, hand, tileCounts))
                    {
                        eval = EvaluateDiscard(hand, i, melds, currentSyanten, tileCounts);
                        evaluations.Add(eval);
                    }
                    continue;
                }
                
                eval = EvaluateDiscard(hand, i, melds, currentSyanten, tileCounts);
                evaluations.Add(eval);
            }
            
            return evaluations;
        }
        
        /// <summary>
        /// 评估单张出牌
        /// </summary>
        private TileEvaluation EvaluateDiscard(List<MahjongTile> hand, int discardIndex, 
            List<MahjongMeld> melds, int currentSyanten, Dictionary<MahjongTile, int> tileCounts)
        {
            var tile = hand[discardIndex];
            var evaluation = new TileEvaluation { Tile = tile };
            
            // 模拟打出这张牌
            var newHand = new List<MahjongTile>(hand);
            newHand.RemoveAt(discardIndex);
            
            // 1. 计算进张改进数
            evaluation.Utility = CalculateUtilityImprovement(newHand, melds, currentSyanten, tile);
            
            // 2. 计算形状价值
            evaluation.ShapeValue = CalculateShapeValue(tile, hand, tileCounts, discardIndex);
            
            // 3. 计算总分
            evaluation.TotalScore = evaluation.Utility * 10 + evaluation.ShapeValue;
            
            return evaluation;
        }
        
        /// <summary>
        /// 计算进张改进数
        /// </summary>
        private int CalculateUtilityImprovement(List<MahjongTile> hand, List<MahjongMeld> melds, 
            int currentSyanten, MahjongTile discardedTile)
        {
            int improvement = 0;
            
            // 获取所有可能的进张
            var allTiles = GetAllPossibleTiles();
            
            foreach (var tile in allTiles)
            {
                // 模拟摸进这张牌
                var tempHand = new List<MahjongTile>(hand) { tile };
                
                // 计算摸牌后的向听数
                int newSyanten = CalculateSyanten(tempHand, melds);
                
                // 如果向听数减少，说明是有效进张
                if (newSyanten < currentSyanten)
                {
                    improvement++;
                }
            }
            
            return improvement;
        }
        
        /// <summary>
        /// 计算形状价值
        /// </summary>
        private int CalculateShapeValue(MahjongTile tile, List<MahjongTile> hand, 
            Dictionary<MahjongTile, int> tileCounts, int index)
        {
            int value = 0;
            
            // 如果是数牌
            if (tile.Suit.ContainsFlag(Suit.Character) || 
                tile.Suit.ContainsFlag(Suit.Circle) || 
                tile.Suit.ContainsFlag(Suit.Bamboo))
            {
                int number = tile.Value;
                
                // 检查是否有相邻牌
                bool hasLeft = HasAdjacentTile(hand, tile, -1);
                bool hasRight = HasAdjacentTile(hand, tile, 1);
                
                // 两面搭子价值最高
                if (hasLeft && hasRight)
                {
                    value += 30;
                }
                // 边张搭子
                else if (hasLeft || hasRight)
                {
                    value += 15;
                }
                
                // 检查是否形成对子
                if (tileCounts[tile] >= 2)
                {
                    value += 20; // 对子有价值
                }
                
                // 检查是否形成刻子
                if (tileCounts[tile] >= 3)
                {
                    value += 40; // 刻子价值高
                }
                
                // 中间牌价值高于边张
                if (number >= 4 && number <= 6)
                {
                    value += 10;
                }
                
                // 检查是否形成顺子
                if (CanFormSequence(hand, tile))
                {
                    value += 25;
                }
            }
            else // 字牌
            {
                // 字牌对子价值较高
                if (tileCounts[tile] >= 2)
                {
                    value += 20;
                }
                
                // 字牌刻子价值很高
                if (tileCounts[tile] >= 3)
                {
                    value += 50;
                }
            }
            
            return value;
        }
        
        /// <summary>
        /// 获取听牌时的最佳出牌
        /// </summary>
        private int GetTenpaiDiscard(List<MahjongTile> hand, List<MahjongMeld> melds)
        {
            // 计算每张牌打掉后的有效进张
            var evaluations = new List<TileEvaluation>();
            
            for (int i = 0; i < hand.Count; i++)
            {
                var tile = hand[i];
                var tempHand = new List<MahjongTile>(hand);
                tempHand.RemoveAt(i);
                
                // 计算打掉这张牌后的进张
                int utility = 0;
                var allTiles = GetAllPossibleTiles();
                
                foreach (var t in allTiles)
                {
                    var testHand = new List<MahjongTile>(tempHand) { t };
                    if (_mahjongSystem.CheckVictory(testHand, melds, t))
                    {
                        utility++;
                    }
                }
                
                evaluations.Add(new TileEvaluation
                {
                    Tile = tile,
                    Utility = utility,
                    TotalScore = utility
                });
            }
            
            // 选择进张最多的牌打出
            var best = evaluations
                .OrderByDescending(e => e.TotalScore)
                .ThenByDescending(e => CalculateShapeValue(e.Tile, hand, CountTiles(hand), 0))
                .FirstOrDefault();
                
            return best != null ? hand.IndexOf(best.Tile) : 0;
        }
        
        /// <summary>
        /// 辅助方法：手牌计数
        /// </summary>
        private Dictionary<MahjongTile, int> CountTiles(List<MahjongTile> hand)
        {
            var counts = new Dictionary<MahjongTile, int>();
            
            foreach (var tile in hand)
            {
                if (counts.ContainsKey(tile))
                {
                    counts[tile]++;
                }
                else
                {
                    counts[tile] = 1;
                }
            }
            
            return counts;
        }
        
        /// <summary>
        /// 检查是否是孤立的幺九牌
        /// </summary>
        private bool IsIsolatedYaochu(MahjongTile tile, List<MahjongTile> hand, Dictionary<MahjongTile, int> counts)
        {
            // 字牌
            if (tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind))
            {
                return counts[tile] == 1; // 单张字牌
            }
            
            // 幺九牌
            int number = tile.Value;
            bool isYaochu = (number == 0 || number == 8 || number == 9 || number == 17 || number == 18 || number == 26);
            
            if (!isYaochu) return false;
            
            // 检查是否有相邻牌
            bool hasLeft = HasAdjacentTile(hand, tile, -1);
            bool hasRight = HasAdjacentTile(hand, tile, 1);
            
            return !hasLeft && !hasRight && counts[tile] == 1;
        }
        
        /// <summary>
        /// 检查是否是无用字牌
        /// </summary>
        private bool IsUselessHonor(MahjongTile tile, List<MahjongTile> hand, Dictionary<MahjongTile, int> counts)
        {
            if (!tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind)) return false;
            
            // 单张字牌且没有形成搭子
            return counts[tile] == 1;
        }
        
        /// <summary>
        /// 检查是否有相邻牌
        /// </summary>
        private bool HasAdjacentTile(List<MahjongTile> hand, MahjongTile tile, int offset)
        {
            // 字牌没有相邻概念
            if (tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind)) return false;
            
            int targetValue = tile.Value + offset;
            
            // 检查边界
            if (targetValue < 0 || targetValue > 26) return false;
            
            // 检查同花色
            var targetTile = MahjongTile.New(targetValue);
            return hand.Any(t => t.Value == targetValue && t.Suit == tile.Suit);
        }
        
        /// <summary>
        /// 检查是否能形成顺子
        /// </summary>
        private bool CanFormSequence(List<MahjongTile> hand, MahjongTile tile)
        {
            if (tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind)) return false;
            
            int number = tile.Value;
            
            // 检查可能的顺子组合
            for (int i = Math.Max(0, number - 2); i <= number; i++)
            {
                if (i + 2 > 26) continue;
                
                bool has1 = hand.Any(t => t.Value == i && t.Suit == tile.Suit);
                bool has2 = hand.Any(t => t.Value == i + 1 && t.Suit == tile.Suit);
                bool has3 = hand.Any(t => t.Value == i + 2 && t.Suit == tile.Suit);
                
                if (has1 && has2 && has3)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取所有可能的牌
        /// </summary>
        private List<MahjongTile> GetAllPossibleTiles()
        {
            var tiles = new List<MahjongTile>();
            
            // 数牌
            for (int i = 0; i < 27; i++)
            {
                tiles.Add(MahjongTile.New(i));
            }
            
            // 字牌
            tiles.Add(MahjongTile.New(MahjongTile.East));
            tiles.Add(MahjongTile.New(MahjongTile.South));
            tiles.Add(MahjongTile.New(MahjongTile.West));
            tiles.Add(MahjongTile.New(MahjongTile.North));
            tiles.Add(MahjongTile.New(MahjongTile.White));
            tiles.Add(MahjongTile.New(MahjongTile.Green));
            tiles.Add(MahjongTile.New(MahjongTile.Red));
            
            return tiles;
        }
        
        /// <summary>
        /// 转换手牌为Syanten格式
        /// </summary>
        private int[] ConvertHandToTehai(List<MahjongTile> hand)
        {
            int[] tehai = new int[41];
            
            foreach (var tile in hand)
            {
                int index = tile.Value;
                
                // 转换为MjScore索引
                if (tile.Suit.ContainsFlag(Suit.Character))
                {
                    index = tile.Value + 1;
                }
                else if (tile.Suit.ContainsFlag(Suit.Circle))
                {
                    index = tile.Value + 2;
                }
                else if (tile.Suit.ContainsFlag(Suit.Bamboo))
                {
                    index = tile.Value + 3;
                }
                else if (tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind))
                {
                    index = tile.Value;
                }
                
                tehai[index]++;
            }
            
            return tehai;
        }
    }
    
    /// <summary>
    /// 扩展的麻将AI功能
    /// </summary>
    public class AdvancedMahjongAI
    {
        private readonly MahjongSystem _mahjongSystem;
        
        public AdvancedMahjongAI()
        {
            _mahjongSystem = MahjongSystem.Instance;
        }
        
        /// <summary>
        /// 计算手牌的潜在进张数
        /// </summary>
        public int CalculatePotentialImprovements(List<MahjongTile> hand, List<MahjongMeld> melds = null)
        {
            if (hand == null || hand.Count == 0) return 0;
            
            int improvements = 0;
            var allTiles = GetAllPossibleTiles();
            
            // 计算当前向听数
            int currentSyanten = _mahjongSystem.CalculateSteps(hand);
            
            // 模拟摸进每张牌
            foreach (var tile in allTiles)
            {
                var tempHand = new List<MahjongTile>(hand) { tile };
                
                // 如果摸进后手牌超过14张，需要模拟打出
                if (tempHand.Count > 14)
                {
                    // 找到最差的牌打出
                    int worstIndex = FindWorstTile(tempHand, melds);
                    if (worstIndex >= 0)
                    {
                        tempHand.RemoveAt(worstIndex);
                    }
                }
                
                int newSyanten = _mahjongSystem.CalculateSteps(tempHand);
                
                if (newSyanten < currentSyanten)
                {
                    improvements++;
                }
            }
            
            return improvements;
        }
        
        /// <summary>
        /// 获取手牌的形状评分
        /// </summary>
        public int EvaluateHandShape(List<MahjongTile> hand)
        {
            if (hand == null || hand.Count == 0) return 0;
            
            int score = 0;
            var counts = CountTiles(hand);
            
            // 按花色分组
            var groups = hand.GroupBy(t => t.Suit)
                           .Select(g => new { Suit = g.Key, Tiles = g.OrderBy(t => t.Value).ToList() })
                           .ToList();
            
            foreach (var group in groups)
            {
                if (group.Suit.ContainsAny(Suit.Dragon, Suit.Wind))
                {
                    // 字牌评分
                    score += EvaluateHonorGroup(group.Tiles, counts);
                }
                else
                {
                    // 数牌评分
                    score += EvaluateNumberGroup(group.Tiles, counts);
                }
            }
            
            return score;
        }
        
        /// <summary>
        /// 评估数牌组
        /// </summary>
        private int EvaluateNumberGroup(List<MahjongTile> tiles, Dictionary<MahjongTile, int> counts)
        {
            int score = 0;
            
            for (int i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                int number = tile.Value;
                
                // 检查对子
                if (counts[tile] >= 2)
                {
                    score += 10;
                }
                
                // 检查刻子
                if (counts[tile] >= 3)
                {
                    score += 20;
                }
                
                // 检查顺子
                if (i + 2 < tiles.Count)
                {
                    if (tiles[i + 1].Value == number + 1 && tiles[i + 2].Value == number + 2)
                    {
                        score += 15;
                    }
                }
                
                // 检查两面搭子
                if (i + 1 < tiles.Count)
                {
                    if (tiles[i + 1].Value == number + 1)
                    {
                        score += 8;
                    }
                }
                
                // 中间牌价值
                if (number >= 3 && number <= 5)
                {
                    score += 5;
                }
            }
            
            return score;
        }
        
        /// <summary>
        /// 评估字牌组
        /// </summary>
        private int EvaluateHonorGroup(List<MahjongTile> tiles, Dictionary<MahjongTile, int> counts)
        {
            int score = 0;
            
            foreach (var tile in tiles)
            {
                if (counts[tile] >= 2)
                {
                    score += 8;
                }
                
                if (counts[tile] >= 3)
                {
                    score += 15;
                }
            }
            
            return score;
        }
        
        /// <summary>
        /// 找到手牌中最差的牌
        /// </summary>
        private int FindWorstTile(List<MahjongTile> hand, List<MahjongMeld> melds = null)
        {
            if (hand.Count == 0) return -1;
            
            int worstIndex = 0;
            int worstScore = int.MaxValue;
            
            for (int i = 0; i < hand.Count; i++)
            {
                var tile = hand[i];
                int score = EvaluateTileForDiscard(tile, hand, melds);
                
                if (score < worstScore)
                {
                    worstScore = score;
                    worstIndex = i;
                }
            }
            
            return worstIndex;
        }
        
        /// <summary>
        /// 评估单张牌的打掉价值
        /// </summary>
        private int EvaluateTileForDiscard(MahjongTile tile, List<MahjongTile> hand, List<MahjongMeld> melds = null)
        {
            int score = 0;
            var counts = CountTiles(hand);
            
            // 如果是孤立的幺九牌或字牌，分数低（应该打掉）
            if (IsIsolatedYaochu(tile, hand, counts))
            {
                score -= 20;
            }
            
            // 如果是中间牌，分数高（应该保留）
            if (!tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind))
            {
                int number = tile.Value;
                if (number >= 3 && number <= 5)
                {
                    score += 10;
                }
            }
            
            // 检查是否是搭子的一部分
            if (IsPartOfSequence(tile, hand))
            {
                score += 15;
            }
            
            // 检查是否是刻子的一部分
            if (counts[tile] >= 2)
            {
                score += 10;
            }
            
            return -score; // 返回负分，低分表示应该打掉
        }
        
        /// <summary>
        /// 检查是否是顺子的一部分
        /// </summary>
        private bool IsPartOfSequence(MahjongTile tile, List<MahjongTile> hand)
        {
            if (tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind)) return false;
            
            int number = tile.Value;
            
            // 检查可能的顺子组合
            var sameSuit = hand.Where(t => t.Suit == tile.Suit).Select(t => t.Value).ToList();
            
            for (int i = Math.Max(0, number - 2); i <= number; i++)
            {
                if (sameSuit.Contains(i) && sameSuit.Contains(i + 1) && sameSuit.Contains(i + 2))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取所有可能的牌
        /// </summary>
        private List<MahjongTile> GetAllPossibleTiles()
        {
            var tiles = new List<MahjongTile>();
            
            for (int i = 0; i < 34; i++)
            {
                tiles.Add(MahjongTile.New(i));
            }
            
            return tiles;
        }
        
        /// <summary>
        /// 统计手牌
        /// </summary>
        private Dictionary<MahjongTile, int> CountTiles(List<MahjongTile> hand)
        {
            var counts = new Dictionary<MahjongTile, int>();
            
            foreach (var tile in hand)
            {
                if (counts.ContainsKey(tile))
                {
                    counts[tile]++;
                }
                else
                {
                    counts[tile] = 1;
                }
            }
            
            return counts;
        }
        
        /// <summary>
        /// 检查是否是孤立的幺九牌
        /// </summary>
        private bool IsIsolatedYaochu(MahjongTile tile, List<MahjongTile> hand, Dictionary<MahjongTile, int> counts)
        {
            // 字牌
            if (tile.Suit.ContainsAny(Suit.Dragon, Suit.Wind))
            {
                return counts[tile] == 1;
            }
            
            // 幺九牌
            int number = tile.Value;
            bool isYaochu = (number == 0 || number == 8 || number == 9 || number == 17 || number == 18 || number == 26);
            
            if (!isYaochu) return false;
            
            // 检查是否有相邻牌
            var sameSuit = hand.Where(t => t.Suit == tile.Suit).Select(t => t.Value).ToList();
            return !sameSuit.Contains(number - 1) && !sameSuit.Contains(number + 1) && counts[tile] == 1;
        }
    }
    
    /// <summary>
    /// AI决策管理器
    /// </summary>
    public class MahjongAIManager
    {
        private readonly MahjongAI _basicAI;
        private readonly AdvancedMahjongAI _advancedAI;
        private readonly MahjongSystem _mahjongSystem;
        
        public MahjongAIManager()
        {
            _basicAI = new MahjongAI();
            _advancedAI = new AdvancedMahjongAI();
            _mahjongSystem = MahjongSystem.Instance;
        }
        
        /// <summary>
        /// 获取AI决策
        /// </summary>
        public AIMove GetBestMove(List<MahjongTile> hand, List<MahjongMeld> melds, MahjongTile drawnTile = null)
        {
            var move = new AIMove();
            
            // 检查是否可以和牌
            if (drawnTile != null)
            {
                var testHand = new List<MahjongTile>(hand) { drawnTile };
                if (_mahjongSystem.CheckVictory(testHand, melds, drawnTile))
                {
                    move.Action = AIAction.Tsumo;
                    move.Tile = drawnTile;
                    return move;
                }
            }
            
            // 计算最佳出牌
            int bestDiscardIndex = _basicAI.GetBestDiscard(hand, melds);
            
            if (bestDiscardIndex >= 0 && bestDiscardIndex < hand.Count)
            {
                move.Action = AIAction.Discard;
                move.Tile = hand[bestDiscardIndex];
                move.DiscardIndex = bestDiscardIndex;
            }
            
            return move;
        }
        
        /// <summary>
        /// 评估手牌质量
        /// </summary>
        public HandQuality EvaluateHandQuality(List<MahjongTile> hand, List<MahjongMeld> melds = null)
        {
            var quality = new HandQuality();
            
            // 计算向听数
            quality.Syanten = _mahjongSystem.CalculateSteps(hand);
            
            // 计算潜在进张
            quality.PotentialImprovements = _advancedAI.CalculatePotentialImprovements(hand, melds);
            
            // 计算形状评分
            quality.ShapeScore = _advancedAI.EvaluateHandShape(hand);
            
            // 评估手牌潜力
            quality.Potential = EvaluateHandPotential(hand, melds, quality);
            
            return quality;
        }
        
        /// <summary>
        /// 评估手牌潜力
        /// </summary>
        private HandPotential EvaluateHandPotential(List<MahjongTile> hand, List<MahjongMeld> melds, HandQuality quality)
        {
            var potential = new HandPotential();
            
            if (quality.Syanten <= 0)
            {
                potential = HandPotential.Ready;
            }
            else if (quality.Syanten <= 1)
            {
                potential = HandPotential.Good;
            }
            else if (quality.Syanten <= 2 && quality.PotentialImprovements > 10)
            {
                potential = HandPotential.Good;
            }
            else if (quality.Syanten <= 3 && quality.PotentialImprovements > 15)
            {
                potential = HandPotential.Average;
            }
            else
            {
                potential = HandPotential.Poor;
            }
            
            return potential;
        }
    }
    
    /// <summary>
    /// AI动作
    /// </summary>
    public class AIMove
    {
        public AIAction Action { get; set; }
        public MahjongTile Tile { get; set; }
        public int DiscardIndex { get; set; }
    }
    
    /// <summary>
    /// AI动作类型
    /// </summary>
    public enum AIAction
    {
        Discard,
        Tsumo,
        Ron,
        Pon,
        Chi,
        Kan,
        Pass
    }
    
    /// <summary>
    /// 手牌质量评估
    /// </summary>
    public class HandQuality
    {
        public int Syanten { get; set; }               // 向听数
        public int PotentialImprovements { get; set; } // 潜在进张数
        public int ShapeScore { get; set; }            // 形状评分
        public HandPotential Potential { get; set; }   // 潜力评估
    }
    
    /// <summary>
    /// 手牌潜力
    /// </summary>
    public enum HandPotential
    {
        Ready,   // 听牌
        Good,    // 好形
        Average, // 一般
        Poor     // 差
    }
}