using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Fictology.Util;
using UnityEngine;

namespace ClassPerson.GameSystem.Mahjong
{
    /// <summary>
    /// 副露组表示
    /// </summary>
    public class MahjongMeld
    {
        public MeldType Type { get; set; }
        public List<MahjongTile> Tiles { get; set; }

        public MahjongMeld(MeldType type, List<MahjongTile> tiles)
        {
            Type = type;
            Tiles = tiles;
        }
    }

    /// <summary>
    /// 副露类型枚举
    /// </summary>
    public enum MeldType
    {
        Pon, // 碰
        Chi, // 吃
        ConcealedKong, // 暗杠
        MeldedKong, // 明杠
        AddedKong // 加杠
    }

    public enum SequenceType
    {
        PON = 1,
        CHII = 2,
        ANKAN = 3,
        MINKAN = 4,
        KAKAN = 5,
        AKA_PON1 = 6,
        AKA_PON2 = 7,
        AKA_CHII = 8,
        TOITU = 9,
        ANKO = 10,
        SYUNTU = 11
    }

    public partial class MjScore
    {
        // 添加设置额外参数的公共方法
        public void SetAdditionalParameters(int honba, int kyoutakuRiichi)
        {
            Honba = honba;
            KyoutakuRiichi = kyoutakuRiichi;
        }

        // 添加获取向听数结果的公共方法
        public void GetSyantenResults(out int normal, out int chiitoitsu, out int kokushi)
        {
            normal = GetNormalSyanten();
            chiitoitsu = GetChiitoitsuSyanten();
            kokushi = GetKokushiSyanten();
        }
    }

    /// <summary>
    /// 风向枚举
    /// </summary>
    public enum WindDirection
    {
        East,
        South,
        West,
        North
    }

    /// <summary>
    /// Mahjong类型到MJScore类型的转换器
    /// </summary>
    public static class MjConvert
    {
        // MJScore牌索引定义 (0-37正常牌，38-40赤牌)
        private const int MJS_EAST = 31; // 东
        private const int MJS_SOUTH = 32; // 南
        private const int MJS_WEST = 33; // 西
        private const int MJS_NORTH = 34; // 北
        private const int MJS_WHITE = 35; // 白
        private const int MJS_GREEN = 36; // 发
        private const int MJS_RED = 37; // 中

        // 赤牌索引
        private const int MJS_RED_CHAR = 38; // 赤5万
        private const int MJS_RED_DOTS = 39; // 赤5筒
        private const int MJS_RED_BAMBOO = 40; // 赤5条

        // 副露类型定义
        private const int PON = 1;
        private const int CHII = 2;
        private const int ANKAN = 3;
        private const int MINKAN = 4;
        private const int KAKAN = 5;
        private const int AKA_PON1 = 6;
        private const int AKA_PON2 = 7;
        private const int AKA_CHII = 8;
        
        // 天凤麻将代码正则表达式
        private static readonly Regex TenhouPattern = new Regex(@"^([0-9]+[mps]|[1-7]+z)+$", RegexOptions.Compiled);
        
        // 天凤牌型解析正则（用于拆分各个部分）
        private static readonly Regex TilePattern = new Regex(@"(?<number>[0-9]+)(?<suit>[mpsz])", RegexOptions.Compiled);
        
        /// <summary>
        /// 天凤代码到Mahjong的映射表
        /// </summary>
        private static readonly Dictionary<char, Dictionary<int, Func<int>>> TileMap = new Dictionary<char, Dictionary<int, Func<int>>>
        {
            ['m'] = new Dictionary<int, Func<int>> // 万子
            {
                {0, () => MahjongTile.Char5}, // 0m = 赤5万
                {1, () => MahjongTile.Char1},
                {2, () => MahjongTile.Char2},
                {3, () => MahjongTile.Char3},
                {4, () => MahjongTile.Char4},
                {5, () => MahjongTile.Char5},
                {6, () => MahjongTile.Char6},
                {7, () => MahjongTile.Char7},
                {8, () => MahjongTile.Char8},
                {9, () => MahjongTile.Char9}
            },
            ['p'] = new Dictionary<int, Func<int>> // 筒子
            {
                {0, () => MahjongTile.Dots5}, // 0p = 赤5筒
                {1, () => MahjongTile.Dots1},
                {2, () => MahjongTile.Dots2},
                {3, () => MahjongTile.Dots3},
                {4, () => MahjongTile.Dots4},
                {5, () => MahjongTile.Dots5},
                {6, () => MahjongTile.Dots6},
                {7, () => MahjongTile.Dots7},
                {8, () => MahjongTile.Dots8},
                {9, () => MahjongTile.Dots9}
            },
            ['s'] = new Dictionary<int, Func<int>> // 索子
            {
                {0, () => MahjongTile.Bamboo5}, // 0s = 赤5条
                {1, () => MahjongTile.Bamboo1},
                {2, () => MahjongTile.Bamboo2},
                {3, () => MahjongTile.Bamboo3},
                {4, () => MahjongTile.Bamboo4},
                {5, () => MahjongTile.Bamboo5},
                {6, () => MahjongTile.Bamboo6},
                {7, () => MahjongTile.Bamboo7},
                {8, () => MahjongTile.Bamboo8},
                {9, () => MahjongTile.Bamboo9}
            },
            ['z'] = new Dictionary<int, Func<int>> // 字牌
            {
                {1, () => MahjongTile.East},
                {2, () => MahjongTile.South},
                {3, () => MahjongTile.West},
                {4, () => MahjongTile.North},
                {5, () => MahjongTile.White},
                {6, () => MahjongTile.Green},
                {7, () => MahjongTile.Red}
            }
        };

        public static List<MahjongTile> Parse(string tenhouCode)
        {
            if (string.IsNullOrWhiteSpace(tenhouCode))
                throw new ArgumentException("天凤代码不能为空", nameof(tenhouCode));
            
            tenhouCode = tenhouCode.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "");
            if (!TenhouPattern.IsMatch(tenhouCode))
                throw new FormatException($"无效的天凤麻将代码格式: {tenhouCode}");

            var hand = new List<MahjongTile>();
            var redTiles = new List<MahjongTile>();
            var hasRedFive = false;

            // 解析每个花色部分
            var matches = TilePattern.Matches(tenhouCode);
            
            foreach (Match match in matches)
            {
                var numbers = match.Groups["number"].Value;
                var suit = match.Groups["suit"].Value[0];
                
                // 验证数字范围
                ValidateNumbers(numbers, suit);
                
                // 解析每个数字
                foreach (var numChar in numbers)
                {
                    var number = int.Parse(numChar.ToString());
                    var tile = CreateTile(suit, number);
                    if (number == 0)
                    {
                        hasRedFive = true;
                        redTiles.Add(tile);
                    }
                    hand.Add(tile);
                }
            }
            return hand;
        }
        
        /// <summary>
        /// 生成天凤代码（反向转换）
        /// </summary>
        public static string GenerateCode(List<MahjongTile> tiles)
        {
            if (tiles == null || tiles.Count == 0)
                return "";
            
            // 按花色分组
            var grouped = new Dictionary<char, List<MahjongTile>>
            {
                ['m'] = new List<MahjongTile>(), // 万子
                ['p'] = new List<MahjongTile>(), // 筒子
                ['s'] = new List<MahjongTile>(), // 条子
                ['z'] = new List<MahjongTile>()  // 字牌
            };
            
            foreach (var tile in tiles)
            {
                var value = tile.Value;
                char suit;
                
                switch (value)
                {
                    case >= 0 and <= 8:
                        suit = 'm';
                        grouped[suit].Add(tile);
                        break;
                    case >= 9 and <= 17:
                        suit = 'p';
                        grouped[suit].Add(tile);
                        break;
                    case >= 18 and <= 26:
                        suit = 's';
                        grouped[suit].Add(tile);
                        break;
                    case >= 27 and <= 33:
                        suit = 'z';
                        grouped[suit].Add(tile);
                        break;
                }
            }
            
            // 构建代码
            var code = "";
            string[] suits = { "m", "p", "s", "z" };
            
            foreach (var suit in suits)
            {
                if (grouped[suit[0]].Count <= 0) continue;
                // 排序
                grouped[suit[0]].Sort((a, b) => a.Value.CompareTo(b.Value));
                    
                // 转换为数字（注意：赤牌需要用0表示，但Mahjong类没有赤牌标记）
                // 这里假设没有赤牌信息
                code = grouped[suit[0]]
                    .Select(tile => tile.Value)
                    .Select(value => suit[0] switch
                    {
                        'm' => (value % 9) + 1,
                        'p' => (value % 9) + 1,
                        's' => (value % 9) + 1,
                        'z' => (value - 27 + 1), // 字牌：东=1，南=2，西=3，北=4，白=5，发=6，中=7
                        _ => 0
                    })
                    .Aggregate(code, (current, number) => current + number.ToString());

                code += suit;
            }
            
            return code;
        }
        
        /// <summary>
        /// 验证数字范围
        /// </summary>
        private static void ValidateNumbers(string numbers, char suit)
        {
            foreach (var number in numbers.Select(numChar => int.Parse(numChar.ToString())))
            {
                switch (suit)
                {
                    case 'm':
                    case 'p':
                    case 's':
                        if (number < 0 || number > 9)
                            throw new ArgumentException($"数牌数字必须在0-9范围内: {number}");
                        break;
                    case 'z':
                        if (number < 1 || number > 7)
                            throw new ArgumentException($"字牌数字必须在1-7范围内: {number}");
                        break;
                }
            }
        }
        
        /// <summary>
        /// 创建麻将牌
        /// </summary>
        private static MahjongTile CreateTile(char suit, int number)
        {
            if (!TileMap.ContainsKey(suit))
                throw new ArgumentException($"无效的花色: {suit}");
            
            if (!TileMap[suit].ContainsKey(number))
                throw new ArgumentException($"无效的数字: {number} 对于花色: {suit}");
            
            return MahjongTile.New(TileMap[suit][number]());
        }

        public static MahjongTile ConvertToMahjong(int mjsIndex)
        {
            return mjsIndex switch
            {
                MJS_EAST => MahjongSystem.Instance.Get(MahjongTile.East),
                MJS_SOUTH => MahjongSystem.Instance.Get(MahjongTile.South),
                MJS_WEST => MahjongSystem.Instance.Get(MahjongTile.West),
                MJS_NORTH => MahjongSystem.Instance.Get(MahjongTile.North),
                MJS_WHITE => MahjongSystem.Instance.Get(MahjongTile.White),
                MJS_GREEN => MahjongSystem.Instance.Get(MahjongTile.Green),
                MJS_RED => MahjongSystem.Instance.Get(MahjongTile.Red),

                > 0 and < 10 => MahjongSystem.Instance.Get(mjsIndex - 1),
                > 10 and < 20 => MahjongSystem.Instance.Get(mjsIndex - 2),
                > 20 and < 30 => MahjongSystem.Instance.Get(mjsIndex - 3),
                _ => throw new ArgumentException($"Invalid MjScore index: {mjsIndex}")
            };
        }

        /// <summary>
        /// 将Mahjong值转换为MJScore索引
        /// </summary>
        private static int ConvertToMjsIndex(MahjongTile mahjong, bool isRed = false)
        {
            var value = mahjong.Value;

            // 如果是赤牌
            if (!isRed)
                return value switch
                {
                    MahjongTile.East => MJS_EAST,
                    MahjongTile.South => MJS_SOUTH,
                    MahjongTile.West => MJS_WEST,
                    MahjongTile.North => MJS_NORTH,
                    MahjongTile.White => MJS_WHITE,
                    MahjongTile.Green => MJS_GREEN,
                    MahjongTile.Red => MJS_RED,

                    _ => mahjong.Suit.ContainsFlag(Suit.Character)
                        ? value + 1
                        : mahjong.Suit.ContainsFlag(Suit.Circle)
                            ? value + 2
                            : mahjong.Suit.ContainsFlag(Suit.Bamboo)
                                ? value + 3
                                : throw new ArgumentException($"Invalid mahjong value: {value}")
                };
            switch (value)
            {
                case MahjongTile.Char5: return MJS_RED_CHAR;
                case MahjongTile.Dots5: return MJS_RED_DOTS;
                case MahjongTile.Bamboo5: return MJS_RED_BAMBOO;
            }

            // 数牌转换 + 字牌转换
            // Mahjong.cs: 0~8   映射至 MjScore.cs: 1~9    【萬字】
            // Mahjong.cs: 9~18  映射至 MjScore.cs: 11~19  【饼字】
            // Mahjong.cs: 19~26 映射至 MjScore.cs: 21~29  【索字】
            return value switch
            {
                MahjongTile.East => MJS_EAST,
                MahjongTile.South => MJS_SOUTH,
                MahjongTile.West => MJS_WEST,
                MahjongTile.North => MJS_NORTH,
                MahjongTile.White => MJS_WHITE,
                MahjongTile.Green => MJS_GREEN,
                MahjongTile.Red => MJS_RED,

                _ => mahjong.Suit.ContainsFlag(Suit.Character)
                    ? value + 1
                    : mahjong.Suit.ContainsFlag(Suit.Circle)
                        ? value + 2
                        : mahjong.Suit.ContainsFlag(Suit.Bamboo)
                            ? value + 3
                            : throw new ArgumentException($"Invalid mahjong value: {value}")
            };
        }

        /// <summary>
        /// 将Mahjong对象转换为MJScore索引
        /// </summary>
        private static int ConvertToMJSIndex(MahjongTile mahjong)
        {
            // 默认不是赤牌
            return ConvertToMjsIndex(mahjong, false);
        }

        /// <summary>
        /// 将手牌列表转换为MJScore的Tehai数组
        /// </summary>
        public static int[] ConvertHandToMjs(List<MahjongTile> hand, List<MahjongTile> redTiles = null)
        {
            var tehai = new int[41]; // MJScore的Tehai长度为41
            // 处理普通牌
            foreach (var index in hand.Select(tile => ConvertToMJSIndex(tile)))
            {
                tehai[index]++;
            }

            // 处理赤牌
            if (redTiles == null) return tehai;
            {
                foreach (var index in redTiles.Select(redTile => ConvertToMjsIndex(redTile, true)))
                {
                    tehai[index]++;
                }
            }

            return tehai;
        }

        /// <summary>
        /// 将副露列表转换为MJScore的Fuuro数组
        /// </summary>
        private static int[] ConvertMeldToFuuro(List<MahjongMeld> melds)
        {
            // Fuuro数组格式：每4个元素一组 [操作类型, 牌1, 牌2, 牌3]
            int[] fuuro = new int[20]; // MJScore的Fuuro长度为20

            int position = 0;
            foreach (var meld in melds)
            {
                if (position >= 20) break; // 防止溢出

                // 根据副露类型设置操作类型
                switch (meld.Type)
                {
                    case MeldType.Pon:
                        fuuro[position] = PON;
                        break;
                    case MeldType.Chi:
                        fuuro[position] = CHII;
                        break;
                    case MeldType.ConcealedKong:
                        fuuro[position] = ANKAN;
                        break;
                    case MeldType.MeldedKong:
                        fuuro[position] = MINKAN;
                        break;
                    case MeldType.AddedKong:
                        fuuro[position] = KAKAN;
                        break;
                    default:
                        continue; // 不支持的类型
                }

                // 添加牌（对于副露，通常只记录第一张牌）
                fuuro[position + 1] = ConvertToMJSIndex(meld.Tiles[0]);

                position += 4;
            }

            return fuuro;
        }

        /// <summary>
        /// 获取MJScore的Bakaze（场风）索引
        /// </summary>
        private static int GetBakazeIndex(WindDirection wind)
        {
            switch (wind)
            {
                case WindDirection.East: return MJS_EAST;
                case WindDirection.South: return MJS_SOUTH;
                case WindDirection.West: return MJS_WEST;
                case WindDirection.North: return MJS_NORTH;
                default: return MJS_EAST; // 默认东风场
            }
        }

        /// <summary>
        /// 获取MJScore的Jikaze（自风）索引
        /// </summary>
        private static int GetJikazeIndex(WindDirection wind)
        {
            return GetBakazeIndex(wind); // 转换方式相同
        }

        /// <summary>
        /// 创建MJScore实例并设置所有参数
        /// </summary>
        public static MjScore CreateMjScore(
            List<MahjongTile> hand,
            List<MahjongMeld> melds,
            MahjongTile winningTile,
            WindDirection bakaze = WindDirection.East,
            WindDirection jikaze = WindDirection.East,
            List<MahjongTile> redTiles = null,
            List<MahjongTile> doraIndicators = null,
            bool isTsumo = false,
            bool isRiichi = false,
            bool isDoubleRiichi = false,
            bool isIppatsu = false,
            bool isHaitei = false,
            int honba = 0,
            int riichiSticks = 0)
        {
            MjScore mjScore = new MjScore();

            // 转换手牌
            int[] tehai = ConvertHandToMjs(hand, redTiles);
            mjScore.SetTehai(tehai);

            // 转换副露
            if (melds != null && melds.Count > 0)
            {
                int[] fuuro = ConvertMeldToFuuro(melds);
                mjScore.SetFuuro(fuuro);
            }

            // 设置和牌牌
            int agarihaiIndex = ConvertToMJSIndex(winningTile);
            mjScore.SetState(
                agarihai: agarihaiIndex,
                tumo: isTsumo,
                riichi: isRiichi,
                doubleRiichi: isDoubleRiichi,
                ippatu: isIppatsu,
                bakaze: GetBakazeIndex(bakaze),
                jikaze: GetJikazeIndex(jikaze),
                haitei: isHaitei,
                akahai: redTiles != null && redTiles.Count > 0
            );

            // 设置宝牌指示器
            if (doraIndicators != null && doraIndicators.Count > 0)
            {
                int[] dora = new int[10]; // MJScore的Dora长度为10
                for (int i = 0; i < Math.Min(doraIndicators.Count, 10); i++)
                {
                    dora[i] = ConvertToMJSIndex(doraIndicators[i]);
                }

                mjScore.SetDora(dora);
            }

            // 设置其他参数
            mjScore.SetAdditionalParameters(honba, riichiSticks);

            return mjScore;
        }
    }
}