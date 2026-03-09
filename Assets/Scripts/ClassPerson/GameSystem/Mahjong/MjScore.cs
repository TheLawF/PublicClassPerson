using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassPerson.GameSystem.Mahjong
{
    public enum TileType
    {
        Characters = 0, // 万
        Dots = 1,       // 筒
        Bamboos = 2,    // 条
        Honors = 3      // 字牌
    }

    public enum YakuType
    {
        TENHOU, // 天和
        TIIHOU,
        NAGASHIMANGAN,
        KOKUSHI,
        KOKUSHI13,
        TYUUREN,
        TYUUREN9,
        DAISUUSII,
        SYOUSUUSII,
        SUUANKOUTANKI,    // 四暗刻单骑
        SUUANKOU,         // 四暗刻
        DAISANGEN,
        TINROUTOU,        // 清老头
        RYUUIISOU,        // 绿一色
        SUUKANTU,       
        TUUIISOU,         // 字一色
        SANKANTU,
        RIICHI,
        DOUBLERIICHI,
        IPPATU,
        TANYAO,
        PINFU,
        IIPEIKOU,
        SANANKOU,         // 三暗刻
        TOITOI,           // 对对和
        SANSYOKUDOUKOU,   // 三色同刻
        RYANPEIKOU,       // 两杯口
        TIITOITU,         // 七对
        HONROUTOU,        // 混老头
        HAKU,
        HATU,
        TYUN,
        BAKAZE,           // 役牌：场风牌
        JIKAZE,           // 役牌：自风牌
        SYOUSANGEN,       // 小三元
        HAITEITUMO,       // 海底捞月
        HOUTEIRON,        // 河底捞鱼
        TYANKAN,          // 抢杠
        RINSYAN,          // 岭上开花
        TUMO,
        SANSYOKUDOUJYUN,  // 三色三同顺
        ITTUU,            // 一气通贯
        TYANTA,           // 全带幺
        JYUNTYANTA,       // 混全带幺九
        HONITU,
        TINITU,
        DORA,
        URADORA,
        AKADORA
    }

    public class AncientYaku
    {
        public static readonly Scoring GreatHeptaStar = new Scoring("大七星", 13);
    }

    /// <summary>
    /// 日麻和牌算法核心，从原C++代码转换而来
    /// <see herf="https://github.com/zhangjk95/MahjongAI/tree/master/MahjongLib"/>
    /// </summary>
    public partial class MjScore
    {
        // 常量定义
        private const int DefaultCode = 0;
        private const int NoEnough14Cards = 1;  // 没有足够的14张牌
        private const int ExceptionOnAgari = 2; // 和牌计算出现未知异常
        private const int NoAgari = 3;          // 未和牌
        private const int NoBakazeOrJiKaze = 4; // 没有场风或自风
        private const int NakiRiichi = 5;       // 鸣牌立直
        private const int NoScoredPattern = 6;  // 无役
        private const int Success = 7;          // 和牌判定成功

        private const int YAKUMAN = 13;
        private const int YAKUMAN2 = 26;
        private const int YAKUMAN3 = 39;
        private const int YAKUMAN4 = 52;
        private const int YAKUMAN5 = 65;
        private const int YAKUMAN6 = 78;
        private const int YAKUMAN7 = 91;
        private const int YAKUMAN8 = 104;
        private const int YAKUMAN9 = 117;
        private const int YAKUMAN10 = 130;
        private const int YAKUMAN11 = 143;

        // 牌操作类型
        private const int PON = 1;
        private const int CHII = 2;
        private const int ANKAN = 3;
        private const int MINKAN = 4;
        private const int KAKAN = 5;
        private const int AKA_PON1 = 6;
        private const int AKA_PON2 = 7;
        private const int AKA_CHII = 8;
        private const int TOITU = 9;
        private const int ANKO = 10;
        private const int SYUNTU = 11;

        // 成员变量
        private int[] Tehai = new int[41]; // 手牌
        private int[] TehaiNormalize = new int[41]; // 正规化手牌
        private int[] TehaiNormalizeMarge = new int[41]; // 合并手牌
        private int[] Fuuro = new int[20]; // 副露
        private int[] Dora = new int[10]; // 宝牌
        private bool[] Yaku = new bool[50]; // 役成立标志
        private bool[] k_tmp_yaku = new bool[50]; // 临时役标志
        private bool[] k_result_yaku = new bool[50]; // 结果役标志
        private string[] YakuName = new string[20]; // 役名
        private Dictionary<int, string> YakuNameTable = new Dictionary<int, string>(); // 役名表
        private Dictionary<int, int> YakuScoreTable = new();

        // 状态变量
        private int Jikaze, Bakaze, Honba, KyoutakuRiichi, ErrorCode, Haino;
        private int TokutenKo, TokutenOya, Fuurosuu, dora, uradora, akadora;
        private int Agarihai, tmp_fu, result_fu, tmp_fan, result_fan;
        private int TokutenKoTumo, TokutenOyaTumo, inputteddora;
        private int Nsyan = 8, Tsyan = 7, Ksyan = 13;

        private bool Akahai, Tenhou, Tiihou, Riichi, DoubleRiichi, Ippatu;
        private bool Tyankan, Rinsyan, NagashiMangan, ManganKiriage, Tumo;
        private bool Haitei, Ba1500, DoubleKokushi13, DoubleTyuuren9;
        private bool DoubleDaisuusii, DoubleSuttan, IsMangan, is_fuuro;

        private int Fan, Fu; // 番和符
        private bool Kuitan = true; // 食断规则

        // 用于向听数计算
        private int nsayn_mentu, nsayn_toitu, nsayn_kouho, nsayn_temp, syanten_normal;

        // 用于和牌判定
        private int[] kiriwake = new int[10];
        private int[] result_kiriwake = new int[10];
        private int p_kiriwake;

        // 得分结果
        public int OyaRon, OyaTumo, KoRon, KoTumoOya, KoTumoKo;

        public MjScore()
        {
            Initialize();
        }

        private void Initialize()
        {
            ClearAllArrays();
            InitializeYakuNames();
        }

        public int[] GetKiriwake() => result_kiriwake;
        
        private void ClearAllArrays()
        {
            Array.Clear(Tehai, 0, Tehai.Length);
            Array.Clear(TehaiNormalize, 0, TehaiNormalize.Length);
            Array.Clear(TehaiNormalizeMarge, 0, TehaiNormalizeMarge.Length);
            Array.Clear(Fuuro, 0, Fuuro.Length);
            Array.Clear(Dora, 0, Dora.Length);
            Array.Clear(Yaku, 0, Yaku.Length);
            Array.Clear(k_tmp_yaku, 0, k_tmp_yaku.Length);
            Array.Clear(k_result_yaku, 0, k_result_yaku.Length);

            for (int i = 0; i < YakuName.Length; i++)
                YakuName[i] = "";

            ResetVariables();
        }

        private void ResetVariables()
        {
            Jikaze = Bakaze = Honba = KyoutakuRiichi = ErrorCode = Haino = 0;
            TokutenKo = TokutenOya = Fuurosuu = dora = uradora = akadora = 0;
            Agarihai = tmp_fu = result_fu = tmp_fan = result_fan = 0;
            TokutenKoTumo = TokutenOyaTumo = inputteddora = 0;

            Nsyan = 8;
            Tsyan = 7;
            Ksyan = 13;

            Akahai = Tenhou = Tiihou = Riichi = DoubleRiichi = Ippatu = false;
            Tyankan = Rinsyan = NagashiMangan = ManganKiriage = Tumo = false;
            Haitei = Ba1500 = DoubleKokushi13 = DoubleTyuuren9 = false;
            DoubleDaisuusii = DoubleSuttan = IsMangan = is_fuuro = false;

            ErrorCode = DefaultCode;
            Kuitan = true;
        }

        private void InitializeYakuNames()
        {
            YakuNameTable.Clear();
            YakuNameTable[(int)YakuType.TENHOU] = "天和";
            YakuNameTable[(int)YakuType.TIIHOU] = "地和";
            YakuNameTable[(int)YakuType.NAGASHIMANGAN] = "流し満貫";
            YakuNameTable[(int)YakuType.KOKUSHI] = "国士無双";
            YakuNameTable[(int)YakuType.KOKUSHI13] = "国士無双１３面待";
            YakuNameTable[(int)YakuType.TYUUREN] = "九連宝燈";
            YakuNameTable[(int)YakuType.TYUUREN9] = "九連宝燈９面待";
            YakuNameTable[(int)YakuType.DAISUUSII] = "大四喜";
            YakuNameTable[(int)YakuType.SYOUSUUSII] = "小四喜";
            YakuNameTable[(int)YakuType.SUUANKOUTANKI] = "四暗刻単騎待";
            YakuNameTable[(int)YakuType.SUUANKOU] = "四暗刻";
            YakuNameTable[(int)YakuType.DAISANGEN] = "大三元";
            YakuNameTable[(int)YakuType.TINROUTOU] = "清老頭";
            YakuNameTable[(int)YakuType.RYUUIISOU] = "緑一色";
            YakuNameTable[(int)YakuType.SUUKANTU] = "四槓子";
            YakuNameTable[(int)YakuType.TUUIISOU] = "字一色";

            // 普通役（番数固定的）
            YakuNameTable[(int)YakuType.SANKANTU] = "三槓子 2";
            YakuNameTable[(int)YakuType.RIICHI] = "リーチ 1";
            YakuNameTable[(int)YakuType.DOUBLERIICHI] = "ダブルリーチ 2";
            YakuNameTable[(int)YakuType.IPPATU] = "一発 1";
            YakuNameTable[(int)YakuType.TANYAO] = "断ヤオ 1";
            YakuNameTable[(int)YakuType.PINFU] = "平和 1";
            YakuNameTable[(int)YakuType.IIPEIKOU] = "一盃口 1";
            YakuNameTable[(int)YakuType.SANANKOU] = "三暗刻 2";
            YakuNameTable[(int)YakuType.TOITOI] = "対々和 2";
            YakuNameTable[(int)YakuType.SANSYOKUDOUKOU] = "三色同刻 2";
            YakuNameTable[(int)YakuType.RYANPEIKOU] = "二盃口 3";
            YakuNameTable[(int)YakuType.TIITOITU] = "七対子 2";
            YakuNameTable[(int)YakuType.HONROUTOU] = "混老頭 2";
            YakuNameTable[(int)YakuType.HAKU] = "白 1";
            YakuNameTable[(int)YakuType.HATU] = "発 1";
            YakuNameTable[(int)YakuType.TYUN] = "中 1";
            YakuNameTable[(int)YakuType.BAKAZE] = "場風 1";
            YakuNameTable[(int)YakuType.JIKAZE] = "自風 1";
            YakuNameTable[(int)YakuType.SYOUSANGEN] = "小三元 2";
            YakuNameTable[(int)YakuType.HAITEITUMO] = "海底撈月 1";
            YakuNameTable[(int)YakuType.HOUTEIRON] = "河底撈魚 1";
            YakuNameTable[(int)YakuType.TYANKAN] = "槍槓 1";
            YakuNameTable[(int)YakuType.RINSYAN] = "嶺上開花 1";
            YakuNameTable[(int)YakuType.TUMO] = "門前清模和 1";

            // 番数根据是否鸣牌而变化的役（先不写番数，在Decision_Score中动态添加）
            YakuNameTable[(int)YakuType.SANSYOKUDOUJYUN] = "三色同順";
            YakuNameTable[(int)YakuType.ITTUU] = "一気通貫";
            YakuNameTable[(int)YakuType.TYANTA] = "全帯";
            YakuNameTable[(int)YakuType.JYUNTYANTA] = "純全帯";
            YakuNameTable[(int)YakuType.HONITU] = "混一色";
            YakuNameTable[(int)YakuType.TINITU] = "清一色";

            // 宝牌（番数动态计算）
            YakuNameTable[(int)YakuType.DORA] = "ドラ";
            YakuNameTable[(int)YakuType.URADORA] = "裏ドラ";
            YakuNameTable[(int)YakuType.AKADORA] = "赤ドラ";
            
            YakuScoreTable.Clear();
            YakuScoreTable[(int)YakuType.TENHOU] = 13;
            YakuScoreTable[(int)YakuType.TIIHOU] = 13;
            YakuScoreTable[(int)YakuType.NAGASHIMANGAN] = 5;
            YakuScoreTable[(int)YakuType.KOKUSHI] = 13;
            YakuScoreTable[(int)YakuType.KOKUSHI13] = 26;
            YakuScoreTable[(int)YakuType.TYUUREN] = 13;
            YakuScoreTable[(int)YakuType.TYUUREN9] = 26;
            YakuScoreTable[(int)YakuType.DAISUUSII] = 26;
            YakuScoreTable[(int)YakuType.SYOUSUUSII] = 13;
            YakuScoreTable[(int)YakuType.SUUANKOUTANKI] = 26;
            YakuScoreTable[(int)YakuType.SUUANKOU] = 13;
            YakuScoreTable[(int)YakuType.DAISANGEN] = 13;
            YakuScoreTable[(int)YakuType.TINROUTOU] = 13;
            YakuScoreTable[(int)YakuType.RYUUIISOU] = 13;
            YakuScoreTable[(int)YakuType.SUUKANTU] = 13;
            YakuScoreTable[(int)YakuType.TUUIISOU] = 13;

            // 普Score数固定的）
            YakuScoreTable[(int)YakuType.SANKANTU] = 2;
            YakuScoreTable[(int)YakuType.RIICHI] = 1;
            YakuScoreTable[(int)YakuType.DOUBLERIICHI] = 2;
            YakuScoreTable[(int)YakuType.IPPATU] = 1;
            YakuScoreTable[(int)YakuType.TANYAO] = 1;
            YakuScoreTable[(int)YakuType.PINFU] = 1;
            YakuScoreTable[(int)YakuType.IIPEIKOU] = 1;
            YakuScoreTable[(int)YakuType.SANANKOU] = 2;
            YakuScoreTable[(int)YakuType.TOITOI] = 2;
            YakuScoreTable[(int)YakuType.SANSYOKUDOUKOU] = 2;
            YakuScoreTable[(int)YakuType.RYANPEIKOU] = 3;
            YakuScoreTable[(int)YakuType.TIITOITU] = 2;
            YakuScoreTable[(int)YakuType.HONROUTOU] = 2;
            YakuScoreTable[(int)YakuType.HAKU] = 1;
            YakuScoreTable[(int)YakuType.HATU] = 1;
            YakuScoreTable[(int)YakuType.TYUN] = 1;
            YakuScoreTable[(int)YakuType.BAKAZE] = 1;
            YakuScoreTable[(int)YakuType.JIKAZE] = 1;
            YakuScoreTable[(int)YakuType.SYOUSANGEN] = 2;
            YakuScoreTable[(int)YakuType.HAITEITUMO] = 1;
            YakuScoreTable[(int)YakuType.HOUTEIRON] = 1;
            YakuScoreTable[(int)YakuType.TYANKAN] = 1;
            YakuScoreTable[(int)YakuType.RINSYAN] = 1;
            YakuScoreTable[(int)YakuType.TUMO] = 1;

            // 番Score否鸣牌而变化的役（先不写番数，在Decision_Score中动态添加）
            YakuScoreTable[(int)YakuType.SANSYOKUDOUJYUN] = -1;
            YakuScoreTable[(int)YakuType.ITTUU] = -1;
            YakuScoreTable[(int)YakuType.TYANTA] = -2;
            YakuScoreTable[(int)YakuType.JYUNTYANTA] = -1;
            YakuScoreTable[(int)YakuType.HONITU] = -1;
            YakuScoreTable[(int)YakuType.TINITU] = -1;

            // 宝Score动态计算）
            YakuScoreTable[(int)YakuType.DORA] = -2;
            YakuScoreTable[(int)YakuType.URADORA] = -3;
            YakuScoreTable[(int)YakuType.AKADORA] = -4;
        }

        // 清除所有数据
        public void Clear()
        {
            ClearAllArrays();
            ResetVariables();
            InitializeYakuNames();
        }

        // 设置手牌
        public void SetTehai(int[] tehai)
        {
            Array.Copy(tehai, Tehai, Math.Min(tehai.Length, Tehai.Length));
        }

        // 设置副露
        public void SetFuuro(int[] fuuro)
        {
            Array.Copy(fuuro, Fuuro, Math.Min(fuuro.Length, Fuuro.Length));
        }

        // 设置宝牌
        public void SetDora(int[] dora)
        {
            Array.Copy(dora, Dora, Math.Min(dora.Length, Dora.Length));
        }

        private bool Is_Fuuro()
        {
            for (int i = 0; i <= 12; i += 4)
            {
                if (Fuuro[i] != 0) return true;
            }

            return false;
        }

        // 设置状态
        public void SetState(int agarihai, bool tumo, bool riichi, bool doubleRiichi,
            bool ippatu, int bakaze, int jikaze, bool haitei, bool akahai)
        {
            Agarihai = agarihai;
            Tumo = tumo;
            Riichi = riichi;
            DoubleRiichi = doubleRiichi;
            Ippatu = ippatu;
            Bakaze = bakaze;
            Jikaze = jikaze;
            Haitei = haitei;
            Akahai = akahai;
        }

        // 主计算函数
        public void Run()
        {
            // 流し満貫
            if (NagashiMangan)
            {
                Fan = 5;
                Fu = -1;
                Yaku[(int)YakuType.NAGASHIMANGAN] = true;
                goto decision;
            }

            // 赤牌正规化
            Tehai_Normalize();

            // 检查牌数
            if (Check_Haino() != 14)
            {
                ErrorCode = NoEnough14Cards;
                return;
            }

            // 检查和牌牌
            if (Agarihai == 0 || TehaiNormalize[Agarihai] == 0)
            {
                ErrorCode = ExceptionOnAgari;
                return;
            }

            // 检查是否和牌
            NormalSyanten();
            TiitoituSyanten();
            KokusiSyanten();
            if (!(Nsyan == -1 || Tsyan == -1 || Ksyan == -1))
            {
                ErrorCode = NoAgari;
                return;
            }

            // 检查场风和自风
            if (Bakaze == 0 || Jikaze == 0)
            {
                ErrorCode = NoBakazeOrJiKaze;
                return;
            }

            // 检查鸣牌立直
            if (Is_FuuroWithoutAnkan() && (Riichi || DoubleRiichi))
            {
                ErrorCode = NakiRiichi;
                return;
            }

            // 初始化
            Array.Clear(Yaku, 0, Yaku.Length);
            Fan = Fu = 0;
            for (int i = 0; i < YakuName.Length; i++) YakuName[i] = "";

            // 国士无双
            if (Ksyan == -1)
            {
                Fan = YAKUMAN;
                if (Tenhou)
                {
                    Yaku[(int)YakuType.TENHOU] = true;
                    Fan++;
                }
                else if (Tiihou)
                {
                    Yaku[(int)YakuType.TIIHOU] = true;
                    Fan++;
                }

                if (Is_Kokusi13())
                {
                    Yaku[(int)YakuType.KOKUSHI13] = true;
                    if (DoubleKokushi13) Fan++;
                }
                else
                {
                    Yaku[(int)YakuType.KOKUSHI] = true;
                    Fan = YAKUMAN;
                }

                Fu = -1;
                goto decision;
            }

            // 七对子
            if (Tsyan == -1 && Nsyan > -1)
            {
                ProcessChiitoitsu();
                goto decision;
            }
            // 通常手

            ProcessNormalHand();

            decision:
            if (!NagashiMangan && Fan == 0)
            {
                ErrorCode = NoScoredPattern;
                return;
            }

            Decision_Score(Fan, Fu);
            ErrorCode = Success;
        }


        // 处理通常手
        private void ProcessNormalHand()
        {
            is_fuuro = Is_Fuuro();
            if (is_fuuro) Fuuro_Suu();

            // 役满检查
            if (Chk_NormalYakuman())
            {
                return;
            }

            // 与面子构成无关的役
            Chk_NotPatternYaku();

            // 与面子构成有关的役
            Chk_PatternYaku();

            // 门清自摸
            if (Tumo && !Is_FuuroWithoutAnkan())
            {
                Yaku[(int)YakuType.TUMO] = true;
                Fan++;
            }

            // 宝牌
            if (Fan > 0)
            {
                if (Is_Dora())
                {
                    Yaku[(int)YakuType.DORA] = true;
                    Fan += dora;
                }

                if (Akahai && Is_Akadora())
                {
                    Yaku[(int)YakuType.AKADORA] = true;
                    Fan += akadora;
                }
            }
        }

        // 赤牌正规化
        private void Tehai_Normalize()
        {
            Array.Copy(Tehai, TehaiNormalize, Tehai.Length);

            if (Akahai)
            {
                TehaiNormalize[5] += Tehai[38];
                TehaiNormalize[15] += Tehai[39];
                TehaiNormalize[25] += Tehai[40];
            }

            TehaiNormalize[38] = -1;

            // 创建合并手牌
            Array.Copy(TehaiNormalize, TehaiNormalizeMarge, TehaiNormalize.Length);

            for (int i = 0; Fuuro[i] != 0 && i <= 12; i += 4)
            {
                if (Fuuro[i] == PON || Fuuro[i] == AKA_PON1 || Fuuro[i] == AKA_PON2)
                {
                    TehaiNormalizeMarge[Fuuro[i + 1]] += 3;
                }
                else if (Fuuro[i] == CHII || Fuuro[i] == AKA_CHII)
                {
                    TehaiNormalizeMarge[Fuuro[i + 1]]++;
                    TehaiNormalizeMarge[Fuuro[i + 1] + 1]++;
                    TehaiNormalizeMarge[Fuuro[i + 1] + 2]++;
                }
                else if (Fuuro[i] == ANKAN || Fuuro[i] == MINKAN || Fuuro[i] == KAKAN)
                {
                    TehaiNormalizeMarge[Fuuro[i + 1]] += 4;
                }
            }
        }

        // 检查牌数
        private int Check_Haino()
        {
            int cnt = 0;
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] < 0 || TehaiNormalizeMarge[i] > 4) return -1;
                cnt += TehaiNormalizeMarge[i];
            }

            return cnt;
        }

        // 检查是否有副露（不含暗杠）
        private bool Is_FuuroWithoutAnkan()
        {
            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                if (Fuuro[i] != ANKAN) return true;
            }

            return false;
        }

        // 计算副露数
        private void Fuuro_Suu()
        {
            int f = 0;
            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                f++;
            }

            Fuurosuu = f;
        }

        // 国士无双向听数计算
        private void KokusiSyanten()
        {
            int kokusi_toitu = 0;
            int syanten_kokusi = 13;

            // 老头牌
            for (int i = 1; i < 30; i++)
            {
                if (i % 10 == 1 || i % 10 == 9)
                {
                    if (TehaiNormalize[i] != 0) syanten_kokusi--;
                    if (TehaiNormalize[i] >= 2 && kokusi_toitu == 0)
                        kokusi_toitu = 1;
                }
            }

            // 字牌
            for (int i = 31; i < 38; i++)
            {
                if (TehaiNormalize[i] != 0)
                {
                    syanten_kokusi--;
                    if (TehaiNormalize[i] >= 2 && kokusi_toitu == 0)
                        kokusi_toitu = 1;
                }
            }

            syanten_kokusi -= kokusi_toitu;
            Ksyan = syanten_kokusi;
        }

        // 七对子向听数计算
        private void TiitoituSyanten()
        {
            int toitu = 0;
            int syurui = 0;

            for (int i = 1; i <= 37; i++)
            {
                if (TehaiNormalize[i] == 0) continue;
                syurui++;
                if (TehaiNormalize[i] >= 2) toitu++;
            }

            int syanten_tiitoi = 6 - toitu;
            if (syurui < 7) syanten_tiitoi += 7 - syurui;

            Tsyan = syanten_tiitoi;
        }

        // 通常手向听数计算
        private void NormalSyanten()
        {
            // 初始化
            nsayn_mentu = 0;
            nsayn_toitu = 0;
            nsayn_kouho = 0;
            nsayn_temp = 0;
            syanten_normal = 8;

            Fuuro_Suu();
            // 尝试所有可能的雀头
            for (int i = 1; i < 38; i++)
            {
                if (TehaiNormalize[i] >= 2)
                {
                    nsayn_toitu++;
                    TehaiNormalize[i] -= 2;
                    nsayn_mentu_cut(1);
                    TehaiNormalize[i] += 2;
                    nsayn_toitu--;
                }
            }
            // 无雀头的情况
            nsayn_mentu_cut(1);
            Nsyan = syanten_normal - Fuurosuu * 2;
        }

        private void nsayn_mentu_cut(int i)
        {
            // 跳过空牌
            while (i < 38 && TehaiNormalize[i] == 0) i++;

            if (i >= 38)
            {
                nsayn_taatu_cut(1);
                return;
            }

            // 刻子
            if (TehaiNormalize[i] >= 3)
            {
                nsayn_mentu++;
                TehaiNormalize[i] -= 3;
                nsayn_mentu_cut(i);
                TehaiNormalize[i] += 3;
                nsayn_mentu--;
            }

            // 顺子
            if (i < 30 && TehaiNormalize[i] > 0 && TehaiNormalize[i + 1] > 0 && TehaiNormalize[i + 2] > 0)
            {
                nsayn_mentu++;
                TehaiNormalize[i]--;
                TehaiNormalize[i + 1]--;
                TehaiNormalize[i + 2]--;
                nsayn_mentu_cut(i);
                TehaiNormalize[i]++;
                TehaiNormalize[i + 1]++;
                TehaiNormalize[i + 2]++;
                nsayn_mentu--;
            }

            // 跳过当前牌
            nsayn_mentu_cut(i + 1);
        }

        // 搭子提取递归函数
        private void nsayn_taatu_cut(int i)
        {
            // 跳过空牌
            while (i < 38 && TehaiNormalize[i] == 0) i++;

            if (i >= 38)
            {
                nsayn_temp = 8 - nsayn_mentu * 2 - nsayn_kouho - nsayn_toitu;
                if (nsayn_temp < syanten_normal)
                    syanten_normal = nsayn_temp;
                return;
            }

            // 如果面子+搭子+副露数小于4
            if (nsayn_mentu + nsayn_kouho + Fuurosuu < 4)
            {
                // 对子
                if (TehaiNormalize[i] == 2)
                {
                    nsayn_kouho++;
                    TehaiNormalize[i] -= 2;
                    nsayn_taatu_cut(i);
                    TehaiNormalize[i] += 2;
                    nsayn_kouho--;
                }

                // 两面或边张搭子
                if (i < 30 && TehaiNormalize[i + 1] > 0)
                {
                    nsayn_kouho++;
                    TehaiNormalize[i]--;
                    TehaiNormalize[i + 1]--;
                    nsayn_taatu_cut(i);
                    TehaiNormalize[i]++;
                    TehaiNormalize[i + 1]++;
                    nsayn_kouho--;
                }

                // 坎张搭子
                if (i < 30 && i % 10 <= 8 && TehaiNormalize[i + 2] > 0)
                {
                    nsayn_kouho++;
                    TehaiNormalize[i]--;
                    TehaiNormalize[i + 2]--;
                    nsayn_taatu_cut(i);
                    TehaiNormalize[i]++;
                    TehaiNormalize[i + 2]++;
                    nsayn_kouho--;
                }
            }

            nsayn_taatu_cut(i + 1);
        }

        // 国士无双13面听牌检查
        private bool Is_Kokusi13()
        {
            int[] yaotyuu = { 1, 9, 11, 19, 21, 29, 31, 32, 33, 34, 35, 36, 37 };
            Tehai[Agarihai]--;

            for (int i = 0; i < 13; i++)
            {
                if (Tehai[yaotyuu[i]] != 1)
                {
                    Tehai[Agarihai]++;
                    return false;
                }
            }

            Tehai[Agarihai]++;
            return true;
        }

        // 字一色检查
        private bool Is_Tuuiisou()
        {
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalize[i] == 0) continue;
                if (i < 30) return false;
            }

            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                if (Fuuro[i + 1] <= 30) return false;
            }

            return true;
        }

        // 断幺九检查
        private bool Is_Tanyao()
        {
            if (!Kuitan && is_fuuro) return false;

            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalize[i] == 0) continue;
                if (i > 30 || i % 10 == 1 || i % 10 == 9)
                    return false;
            }

            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                int tile = Fuuro[i + 1];
                if (tile > 30 || tile % 10 == 1 || tile % 10 == 9 ||
                    (tile % 10 == 7 && (Fuuro[i] == CHII || Fuuro[i] == AKA_CHII)))
                    return false;
            }

            return true;
        }

        // 宝牌检查
        private bool Is_Dora()
        {
            int tmp;
            dora = uradora = 0;

            // 表宝牌
            for (int i = 1; i <= 5; i++)
            {
                if (Dora[i] != 0)
                {
                    tmp = GetNextDora(Dora[i]);
                    dora += TehaiNormalizeMarge[tmp];

                    for (int j = 0; Fuuro[j] != 0 && j <= 12; j += 4)
                    {
                        if ((Fuuro[j] == ANKAN || Fuuro[j] == MINKAN || Fuuro[j] == KAKAN) &&
                            Fuuro[j + 1] == tmp)
                            dora++;
                    }
                }
            }

            // 里宝牌（立直且门清）
            if (Riichi && !Is_FuuroWithoutAnkan())
            {
                for (int i = 6; i <= 9; i++)
                {
                    if (Dora[i] != 0)
                    {
                        tmp = GetNextDora(Dora[i]);
                        uradora += TehaiNormalizeMarge[tmp];
                    }
                }
            }

            // 外部输入的宝牌
            if (inputteddora > 0)
            {
                dora += inputteddora;
            }

            return (dora + uradora) > 0;
        }

        private int GetNextDora(int tile)
        {
            if (tile < 30 && tile % 10 == 9) return (tile / 10) * 10 + 1;
            else if (tile == 34) return 31;
            else if (tile == 37) return 35;
            else return tile + 1;
        }

        // 赤宝牌检查
        private bool Is_Akadora()
        {
            if (!Akahai) return false;

            int tmp = Get_AkahaiSuu();
            if (tmp > 0)
            {
                akadora = tmp;
                return true;
            }

            return false;
        }

        // 获取赤牌数
        private int Get_AkahaiSuu()
        {
            int tmp = 0;
            tmp += Tehai[38];
            tmp += Tehai[39];
            tmp += Tehai[40];

            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                if (Fuuro[i] == ANKAN || Fuuro[i] == MINKAN || Fuuro[i] == KAKAN || Fuuro[i] == AKA_PON1)
                {
                    if (Fuuro[i + 1] % 10 == 5 && Fuuro[i + 1] < 30) tmp++;
                }
                else if (Fuuro[i] == AKA_PON2)
                {
                    tmp += 2;
                }
                else if (Fuuro[i] == AKA_CHII)
                {
                    tmp++;
                }
            }

            return tmp;
        }

        // 与面子构成有关的役检查
        private void Chk_PatternYaku()
        {
            // 面子拆分
            MentuKiriwake();

            // 合并结果
            Fan += result_fan;
            Fu = result_fu;

            Yaku[(int)YakuType.PINFU] = k_result_yaku[(int)YakuType.PINFU];
            Yaku[(int)YakuType.IIPEIKOU] = k_result_yaku[(int)YakuType.IIPEIKOU];
            Yaku[(int)YakuType.SANSYOKUDOUJYUN] = k_result_yaku[(int)YakuType.SANSYOKUDOUJYUN];
            Yaku[(int)YakuType.ITTUU] = k_result_yaku[(int)YakuType.ITTUU];
            Yaku[(int)YakuType.SANANKOU] = k_result_yaku[(int)YakuType.SANANKOU];
            Yaku[(int)YakuType.TYANTA] = k_result_yaku[(int)YakuType.TYANTA];
            Yaku[(int)YakuType.JYUNTYANTA] = k_result_yaku[(int)YakuType.JYUNTYANTA];
            Yaku[(int)YakuType.TOITOI] = k_result_yaku[(int)YakuType.TOITOI];
            Yaku[(int)YakuType.SANSYOKUDOUKOU] = k_result_yaku[(int)YakuType.SANSYOKUDOUKOU];
            Yaku[(int)YakuType.RYANPEIKOU] = k_result_yaku[(int)YakuType.RYANPEIKOU];
        }

        // 面子拆分
        private void MentuKiriwake()
        {
            // 初始化
            Array.Clear(kiriwake, 0, kiriwake.Length);
            Array.Clear(result_kiriwake, 0, result_kiriwake.Length);
            p_kiriwake = 0;
            tmp_fan = tmp_fu = 0;
            Array.Clear(k_tmp_yaku, 0, k_tmp_yaku.Length);
            Array.Clear(k_result_yaku, 0, k_result_yaku.Length);

            // 整合副露牌
            for (int i = 0; i < 20; i += 4)
            {
                if (Fuuro[i] != 0)
                {
                    if (Fuuro[i] == CHII || Fuuro[i] == AKA_CHII)
                        kiriwake[p_kiriwake] = CHII;
                    else if (Fuuro[i] == PON || Fuuro[i] == AKA_PON1 || Fuuro[i] == AKA_PON2)
                        kiriwake[p_kiriwake] = PON;
                    else
                        kiriwake[p_kiriwake] = Fuuro[i];

                    kiriwake[p_kiriwake + 1] = Fuuro[i + 1];
                    p_kiriwake += 2;
                }
                else break;
            }

            // 提取雀头
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalize[i] >= 2)
                {
                    TehaiNormalize[i] -= 2;
                    kiriwake[p_kiriwake] = TOITU;
                    kiriwake[p_kiriwake + 1] = i;
                    p_kiriwake += 2;

                    KiriwakeNukidasi();

                    p_kiriwake -= 2;
                    kiriwake[p_kiriwake] = 0;
                    kiriwake[p_kiriwake + 1] = 0;
                    TehaiNormalize[i] += 2;
                }
            }
        }


        // 平和检查
        private bool Is_Pinfu()
        {
            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == TOITU)
                {
                    int tile = kiriwake[i + 1];
                    if (tile == Jikaze || tile == Bakaze || tile >= 35)
                        return false;
                }
                else if (kiriwake[i] != SYUNTU)
                    return false;
            }

            // 两面听牌检查
            bool ryanmen = false;
            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == SYUNTU)
                {
                    int start = kiriwake[i + 1];
                    if ((start == Agarihai - 2 && start % 10 >= 2) ||
                        (start == Agarihai && start % 10 < 7))
                        ryanmen = true;
                }
            }

            return ryanmen;
        }

        // 一杯口检查
        private bool Is_Iipeikou()
        {
            int[] chk = new int[38];
            for (int i = 2; i < 10; i += 2)
            {
                if (kiriwake[i] == SYUNTU)
                    chk[kiriwake[i + 1]]++;
            }

            for (int i = 0; i < 38; i++)
            {
                if (chk[i] >= 2) return true;
            }

            return false;
        }

        // 二杯口检查
        private bool Is_Ryanpeikou()
        {
            int[] chk = new int[38];
            int kazu = 0;

            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == SYUNTU)
                    chk[kiriwake[i + 1]]++;
            }

            for (int i = 0; i < 38; i++)
            {
                if (chk[i] >= 2)
                {
                    if (++kazu == 2 || chk[i] == 4)
                        return true;
                }
            }

            return false;
        }

        // 符计算
        private void Chk_Fu()
        {
            bool menzen = !Is_FuuroWithoutAnkan();

            // 平和
            if (k_tmp_yaku[(int)YakuType.PINFU])
            {
                tmp_fu = Tumo ? 20 : 30;
                return;
            }

            tmp_fu = 20; // 副底

            // 门清加符
            if (menzen && !Tumo) tmp_fu += 10;

            // 自摸符
            if (Tumo) tmp_fu += 2;

            // 听牌形式符
            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == TOITU)
                {
                    if (kiriwake[i + 1] == Agarihai)
                    {
                        tmp_fu += 2; // 单骑
                        break;
                    }
                }
                else if (kiriwake[i] == SYUNTU || kiriwake[i] == CHII)
                {
                    int start = kiriwake[i + 1];
                    if ((start == Agarihai - 2 && start % 10 == 1) || // 边张
                        (start == Agarihai && start % 10 == 7) || // 边张
                        start == Agarihai - 1 || // 坎张
                        start == Agarihai + 1) // 坎张
                    {
                        tmp_fu += 2;
                        break;
                    }
                }
            }
            
            // 雀头符
            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == TOITU)
                {
                    int tile = kiriwake[i + 1];
                    if (tile >= 35 || tile == Bakaze) tmp_fu += 2; // 役牌
                    if (tile == Jikaze) tmp_fu += 2; // 连风牌
                }
            }

            // 面子符
            for (int i = 0; i < 10; i += 2)
            {
                int tile = kiriwake[i + 1];
                bool yaotyuu = (tile % 10 == 1 || tile % 10 == 9 || tile > 30);

                switch (kiriwake[i])
                {
                    case ANKO: // 暗刻
                        tmp_fu += yaotyuu ? 8 : 4;
                        break;
                    case PON: // 明刻
                        tmp_fu += yaotyuu ? 4 : 2;
                        break;
                    case ANKAN: // 暗杠
                        tmp_fu += yaotyuu ? 32 : 16;
                        break;
                    case MINKAN: // 明杠
                    case KAKAN: // 加杠
                        tmp_fu += yaotyuu ? 16 : 8;
                        break;
                }
            }

            // 鸣き平和形
            if (tmp_fu == 20) tmp_fu = 30;

            // 进位
            if (tmp_fu % 10 > 0) tmp_fu += 10 - tmp_fu % 10;
        }

        // 处理七对子的完整实现
        private void ProcessChiitoitsu()
        {
            // 役满检查
            if (Tenhou)
            {
                Yaku[(int)YakuType.TENHOU] = true;
                Fan = YAKUMAN;
            }
            else if (Tiihou)
            {
                Yaku[(int)YakuType.TIIHOU] = true;
                Fan = YAKUMAN;
            }
            
            if (Is_Tuuiisou())
            {
                Yaku[(int)YakuType.TUUIISOU] = true;
                Fan = (Fan == YAKUMAN) ? Fan + 1 : YAKUMAN;
            }
            
            if (Fan >= YAKUMAN)
            {
                Fu = -1;
                return;
            }

            // 普通役
            Yaku[(int)YakuType.TIITOITU] = true;
            Fu = 25;
            Fan = 2;

            // 立直/两立直
            if (DoubleRiichi)
            {
                Yaku[(int)YakuType.DOUBLERIICHI] = true;
                Fan += 2;
            }
            else if (Riichi)
            {
                Yaku[(int)YakuType.RIICHI] = true;
                Fan++;
            }

            // 一発
            if (Ippatu && Riichi)
            {
                Yaku[(int)YakuType.IPPATU] = true;
                Fan++;
            }

            // 海底/河底
            if (Haitei && Tumo)
            {
                Yaku[(int)YakuType.HAITEITUMO] = true;
                Fan++;
            }
            else if (Haitei && !Tumo)
            {
                Yaku[(int)YakuType.HOUTEIRON] = true;
                Fan++;
            }

            // ドラ
            if (Is_Dora())
            {
                Yaku[(int)YakuType.DORA] = true;
                Fan += dora;
            }
            
            // 赤ドラ
            if (Akahai && Is_Akadora())
            {
                Yaku[(int)YakuType.AKADORA] = true;
                Fan += akadora;
            }
            
            // 混老頭
            if (Is_Honroutou())
            {
                Yaku[(int)YakuType.HONROUTOU] = true;
                Fan += 2;
            }
            
            // 清一色
            if (Is_Tinitu())
            {
                Yaku[(int)YakuType.TINITU] = true;
                Fan += is_fuuro ? 5 : 6;
                
                // 断幺九
                if (Is_Tanyao())
                {
                    Yaku[(int)YakuType.TANYAO] = true;
                    Fan++;
                }
            }
            // 混一色
            else if (Is_Honitu())
            {
                Yaku[(int)YakuType.HONITU] = true;
                Fan += is_fuuro ? 2 : 3;
            }
            else
            {
                // 断幺九
                if (Is_Tanyao())
                {
                    Yaku[(int)YakuType.TANYAO] = true;
                    Fan++;
                }
            }
            
            // 自摸
            if (Tumo)
            {
                Yaku[(int)YakuType.TUMO] = true;
                Fan++;
            }
        }

        // 检查通常手役满的完整实现
        private bool Chk_NormalYakuman()
        {
            // 检查是否鸣牌（包括暗杠）
            bool is_fuuro_withAnkan = false;
            for (int i = 0; i <= 12; i += 4)
            {
                if (Fuuro[i] != 0)
                {
                    is_fuuro_withAnkan = true;
                    break;
                }
            }

            // 天和/地和
            if (!is_fuuro_withAnkan)
            {
                if (Tenhou)
                {
                    Yaku[(int)YakuType.TENHOU] = true;
                    Fan++;
                }
                else if (Tiihou)
                {
                    Yaku[(int)YakuType.TIIHOU] = true;
                    Fan++;
                }
                
                // 九莲宝灯
                if (Is_Tyuuren9())
                {
                    Yaku[(int)YakuType.TYUUREN9] = true;
                    if (DoubleTyuuren9) Fan++;
                    Fan++;
                    goto end_thisfunc;
                }

                if (Is_Tyuuren())
                {
                    Yaku[(int)YakuType.TYUUREN] = true;
                    Fan++;
                    goto end_thisfunc;
                }
            }
            
            // 四暗刻/四暗刻单骑
            if (!Is_FuuroWithoutAnkan())
            {
                if (Is_SuuankouuTanki())
                {
                    Yaku[(int)YakuType.SUUANKOUTANKI] = true;
                    if (DoubleSuttan) Fan++;
                    Fan++;
                }
                else if (Is_Suuankouu())
                {
                    Yaku[(int)YakuType.SUUANKOU] = true;
                    Fan++;
                }
            }
            
            // 绿一色
            if (Is_Ryuuiisou())
            {
                Yaku[(int)YakuType.RYUUIISOU] = true;
                Fan++;
                if (Is_Suukantu())
                {
                    Yaku[(int)YakuType.SUUKANTU] = true;
                    Fan++;
                }
                goto end_thisfunc;
            }
            
            // 清老头
            if (Is_Tinroutou())
            {
                Yaku[(int)YakuType.TINROUTOU] = true;
                Fan++;
                if (Is_Suukantu())
                {
                    Yaku[(int)YakuType.SUUKANTU] = true;
                    Fan++;
                }
                Fu = -1;
                goto end_thisfunc;
            }
            
            // 大四喜/小四喜
            if (Is_Daisuusii())
            {
                Yaku[(int)YakuType.DAISUUSII] = true;
                if (DoubleDaisuusii) Fan++;
                Fan++;
            }
            else if (Is_Syousuusii())
            {
                Yaku[(int)YakuType.SYOUSUUSII] = true;
                Fan++;
            }
            
            // 字一色
            if (Is_Tuuiisou())
            {
                Yaku[(int)YakuType.TUUIISOU] = true;
                Fan++;
            }
            
            // 四杠子
            if (Is_Suukantu())
            {
                Yaku[(int)YakuType.SUUKANTU] = true;
                Fan++;
            }
            
            // 大三元
            if (Is_Daisangen())
            {
                Yaku[(int)YakuType.DAISANGEN] = true;
                Fan++;
            }

        end_thisfunc:
            if (Fan > 0)
            {
                Fan += YAKUMAN - 1;
                Fu = -1;
                return true;
            }
            return false;
        }

        // 检查与面子构成无关的役的完整实现
        private void Chk_NotPatternYaku()
        {
            // 两立直
            if (DoubleRiichi)
            {
                Yaku[(int)YakuType.DOUBLERIICHI] = true;
                Fan += 2;
            }
            // 立直
            else if (Riichi)
            {
                Yaku[(int)YakuType.RIICHI] = true;
                Fan++;
            }
            
            // 一発
            if (Ippatu && (Riichi || DoubleRiichi))
            {
                Yaku[(int)YakuType.IPPATU] = true;
                Fan++;
            }
            
            // 海底摸月
            if (Haitei && Tumo)
            {
                Yaku[(int)YakuType.HAITEITUMO] = true;
                Fan++;
            }
            // 河底捞鱼
            else if (Haitei && !Tumo)
            {
                Yaku[(int)YakuType.HOUTEIRON] = true;
                Fan++;
            }
            
            // 混老頭
            if (Is_Honroutou())
            {
                Yaku[(int)YakuType.HONROUTOU] = true;
                Fan += 2;
            }
            
            // 清一色
            if (!Yaku[(int)YakuType.HONROUTOU] && Is_Tinitu())
            {
                Yaku[(int)YakuType.TINITU] = true;
                Fan += is_fuuro ? 5 : 6;
                
                // 断幺九
                if (Is_Tanyao())
                {
                    Yaku[(int)YakuType.TANYAO] = true;
                    Fan++;
                }
            }
            // 混一色
            else if (!Yaku[(int)YakuType.TINITU] && Is_Honitu())
            {
                Yaku[(int)YakuType.HONITU] = true;
                Fan += is_fuuro ? 2 : 3;
            }
            else
            {
                // 断幺九
                if (Is_Tanyao())
                {
                    Yaku[(int)YakuType.TANYAO] = true;
                    Fan++;
                }
            }
            
            // 只有非断幺九时检查以下役
            if (!Yaku[(int)YakuType.TANYAO])
            {
                // 白
                if (TehaiNormalizeMarge[35] == 3)
                {
                    Yaku[(int)YakuType.HAKU] = true;
                    Fan++;
                }
                
                // 发
                if (TehaiNormalizeMarge[36] == 3)
                {
                    Yaku[(int)YakuType.HATU] = true;
                    Fan++;
                }
                
                // 中
                if (TehaiNormalizeMarge[37] == 3)
                {
                    Yaku[(int)YakuType.TYUN] = true;
                    Fan++;
                }
                
                // 场风
                if (TehaiNormalizeMarge[Bakaze] == 3)
                {
                    Yaku[(int)YakuType.BAKAZE] = true;
                    Fan++;
                }
                
                // 自风
                if (TehaiNormalizeMarge[Jikaze] == 3)
                {
                    Yaku[(int)YakuType.JIKAZE] = true;
                    Fan++;
                }
                
                // 小三元
                if (Is_Syousangen())
                {
                    Yaku[(int)YakuType.SYOUSANGEN] = true;
                    Fan += 2;
                }
            }
            
            // 枪杠
            if (Tyankan)
            {
                Yaku[(int)YakuType.TYANKAN] = true;
                Fan++;
            }
            
            // 岭上开花
            if (Rinsyan)
            {
                Yaku[(int)YakuType.RINSYAN] = true;
                Fan++;
            }
            
            // 三杠子
            if (Is_Sankantu())
            {
                Yaku[(int)YakuType.SANKANTU] = true;
                Fan += 2;
            }
        }

        // 面子提取递归函数的完整实现
        private void KiriwakeNukidasi()
        {
            int i;
            for (i = 0; i < 38; i++)
            {
                while (i < 38 && TehaiNormalize[i] == 0) i++;
                if (i >= 38)
                {
                    if (kiriwake[9] != 0) // 4面子1雀头齐全
                    {
                        bool fuuro = is_fuuro;
                        
                        if (!fuuro)
                        {
                            // 平和
                            if (Is_Pinfu())
                            {
                                k_tmp_yaku[(int)YakuType.PINFU] = true;
                                tmp_fan++;
                            }
                            
                            // 二杯口
                            if (Is_Ryanpeikou())
                            {
                                k_tmp_yaku[(int)YakuType.RYANPEIKOU] = true;
                                tmp_fan += 3;
                            }
                            // 一杯口
                            else if (Is_Iipeikou())
                            {
                                k_tmp_yaku[(int)YakuType.IIPEIKOU] = true;
                                tmp_fan++;
                            }
                        }
                        
                        // 一気通貫
                        if (Is_Ittuu())
                        {
                            k_tmp_yaku[(int)YakuType.ITTUU] = true;
                            if (!fuuro) tmp_fan++;
                            tmp_fan++;
                        }
                        
                        // 三色同順
                        if (Is_Sansyokudoujyun())
                        {
                            k_tmp_yaku[(int)YakuType.SANSYOKUDOUJYUN] = true;
                            if (!fuuro) tmp_fan++;
                            tmp_fan++;
                        }
                        // 三色同刻
                        else if (Is_Sansyokudoukou())
                        {
                            k_tmp_yaku[(int)YakuType.SANSYOKUDOUKOU] = true;
                            tmp_fan += 2;
                        }
                        
                        // 純全帯幺
                        if (!Yaku[(int)YakuType.HONROUTOU] && Is_Jyuntyanta())
                        {
                            k_tmp_yaku[(int)YakuType.JYUNTYANTA] = true;
                            if (!fuuro) tmp_fan++;
                            tmp_fan += 2;
                        }
                        // 全帯幺
                        else if (!Yaku[(int)YakuType.HONROUTOU] && Is_Tyanta())
                        {
                            k_tmp_yaku[(int)YakuType.TYANTA] = true;
                            if (!fuuro) tmp_fan++;
                            tmp_fan++;
                        }
                        
                        // 对々和和三暗刻
                        if (!k_tmp_yaku[(int)YakuType.ITTUU] && !k_tmp_yaku[(int)YakuType.SANSYOKUDOUJYUN])
                        {
                            // 对々和
                            if (Is_Toitoi())
                            {
                                k_tmp_yaku[(int)YakuType.TOITOI] = true;
                                tmp_fan += 2;
                            }
                            
                            // 三暗刻
                            if (Is_Sanankou())
                            {
                                k_tmp_yaku[(int)YakuType.SANANKOU] = true;
                                tmp_fan += 2;
                            }
                        }

                        // 计算符
                        Chk_Fu();
                        
                        // 选择最佳结果（优先番数，同番数选符高的）
                        if ((tmp_fan > result_fan) || (tmp_fan == result_fan && tmp_fu > result_fu))
                        {
                            result_fan = tmp_fan;
                            result_fu = tmp_fu;
                            Array.Copy(kiriwake, result_kiriwake, 10);
                            Array.Copy(k_tmp_yaku, k_result_yaku, k_tmp_yaku.Length);
                        }
                        
                        // 重置临时变量
                        tmp_fan = tmp_fu = 0;
                        Array.Clear(k_tmp_yaku, 0, k_tmp_yaku.Length);
                    }
                    return;
                }
                
                // 刻子提取
                if (TehaiNormalize[i] >= 3)
                {
                    TehaiNormalize[i] -= 3;
                    // 如果是荣和且和牌牌是这个刻子，则是明刻
                    kiriwake[p_kiriwake] = (i == Agarihai && !Tumo) ? PON : ANKO;
                    kiriwake[p_kiriwake + 1] = i;
                    p_kiriwake += 2;
                    
                    KiriwakeNukidasi(); // 递归
                    
                    p_kiriwake -= 2;
                    kiriwake[p_kiriwake] = 0;
                    kiriwake[p_kiriwake + 1] = 0;
                    TehaiNormalize[i] += 3;
                }
                
                // 顺子提取
                if (TehaiNormalize[i] > 0 && i < 30 && 
                    TehaiNormalize[i + 1] > 0 && TehaiNormalize[i + 2] > 0)
                {
                    TehaiNormalize[i]--;
                    TehaiNormalize[i + 1]--;
                    TehaiNormalize[i + 2]--;
                    
                    kiriwake[p_kiriwake] = SYUNTU;
                    kiriwake[p_kiriwake + 1] = i;
                    p_kiriwake += 2;
                    
                    KiriwakeNukidasi(); // 递归
                    
                    p_kiriwake -= 2;
                    kiriwake[p_kiriwake] = 0;
                    kiriwake[p_kiriwake + 1] = 0;
                    TehaiNormalize[i]++;
                    TehaiNormalize[i + 1]++;
                    TehaiNormalize[i + 2]++;
                }
            }
        }

        // 辅助检查函数的实现
        private bool Is_Suuankouu()
        {
            // 四暗刻必须自摸
            if (!Tumo) return false;
            
            int kootu = 0;
            bool toitu = false;
            
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                if (TehaiNormalizeMarge[i] == 1) return false; // 不能有单张
                else if (!toitu && TehaiNormalizeMarge[i] == 2)
                    toitu = true; // 雀头
                else if (TehaiNormalizeMarge[i] == 3)
                    kootu++; // 刻子
            }
            
            return (kootu == 4 && toitu);
        }

        private bool Is_SuuankouuTanki()
        {
            int kootu = 0;
            bool toitu = false;
            
            // 临时减少和牌牌数量检查单骑听牌
            TehaiNormalizeMarge[Agarihai]--;
            
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                if (TehaiNormalizeMarge[i] == 1 && (i == Agarihai))
                    toitu = true; // 单骑听牌
                else if (TehaiNormalizeMarge[i] == 3)
                    kootu++; // 刻子
            }
            
            TehaiNormalizeMarge[Agarihai]++;
            return (kootu == 4 && toitu);
        }

        private bool Is_Daisangen()
        {
            return (TehaiNormalizeMarge[35] >= 3 && 
                    TehaiNormalizeMarge[36] >= 3 && 
                    TehaiNormalizeMarge[37] >= 3);
        }

        private bool Is_Tyuuren()
        {
            int syurui = -1;
            // 检查是否只有一种数牌
            if (TehaiNormalizeMarge[1] > 0 && 
                TehaiNormalizeMarge[11] == 0 && 
                TehaiNormalizeMarge[21] == 0)
                syurui = 0; // 万子
            else if (TehaiNormalizeMarge[11] > 0 && 
                     TehaiNormalizeMarge[1] == 0 && 
                     TehaiNormalizeMarge[21] == 0)
                syurui = 1; // 筒子
            else if (TehaiNormalizeMarge[21] > 0 && 
                     TehaiNormalizeMarge[1] == 0 && 
                     TehaiNormalizeMarge[11] == 0)
                syurui = 2; // 条子
            else
                return false;
            
            // 检查字牌
            for (int i = 31; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] > 0) return false;
            }
            
            // 九莲宝灯形检查
            int start = syurui * 10 + 1;
            for (int i = start; i <= start + 8; i++)
            {
                int count = TehaiNormalizeMarge[i];
                int position = i % 10;
                
                if (position == 1 || position == 9)
                {
                    if (count < 3) return false; // 幺九牌至少3张
                }
                else
                {
                    if (count < 1) return false; // 中张牌至少1张
                }
            }
            
            return true;
        }

        private bool Is_Tyuuren9()
        {
            int syurui = -1;
            
            // 检查是否只有一种数牌
            if (TehaiNormalizeMarge[1] > 0 && 
                TehaiNormalizeMarge[11] == 0 && 
                TehaiNormalizeMarge[21] == 0)
                syurui = 0; // 万子
            else if (TehaiNormalizeMarge[11] > 0 && 
                     TehaiNormalizeMarge[1] == 0 && 
                     TehaiNormalizeMarge[21] == 0)
                syurui = 1; // 筒子
            else if (TehaiNormalizeMarge[21] > 0 && 
                     TehaiNormalizeMarge[1] == 0 && 
                     TehaiNormalizeMarge[11] == 0)
                syurui = 2; // 条子
            else
                return false;
            
            // 检查字牌
            for (int i = 31; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] > 0) return false;
            }
            
            // 临时减少和牌牌
            TehaiNormalizeMarge[Agarihai]--;
            
            int start = syurui * 10 + 1;
            for (int i = start; i <= start + 8; i++)
            {
                int count = TehaiNormalizeMarge[i];
                int position = i % 10;
                
                if (position == 1 || position == 9)
                {
                    if (count != 3) 
                    {
                        TehaiNormalizeMarge[Agarihai]++;
                        return false; // 幺九牌必须正好3张
                    }
                }
                else
                {
                    if (count != 1) 
                    {
                        TehaiNormalizeMarge[Agarihai]++;
                        return false; // 中张牌必须正好1张
                    }
                }
            }
            
            TehaiNormalizeMarge[Agarihai]++;
            return true;
        }

        private bool Is_Daisuusii()
        {
            for (int i = 31; i <= 34; i++)
            {
                if (TehaiNormalizeMarge[i] < 3) return false;
            }
            return true;
        }

        private bool Is_Syousuusii()
        {
            bool toitu = false;
            int anko = 0;
            
            for (int i = 31; i <= 34; i++)
            {
                if (!toitu && TehaiNormalizeMarge[i] == 2)
                    toitu = true;
                else if (TehaiNormalizeMarge[i] == 3)
                    anko++;
            }
            
            return (anko == 3 && toitu);
        }

        private bool Is_Suukantu()
        {
            if (Fuurosuu != 4) return false;
            
            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                if (Fuuro[i] != ANKAN && Fuuro[i] != MINKAN && Fuuro[i] != KAKAN)
                    return false;
            }
            return true;
        }

        private bool Is_Syousangen()
        {
            bool toitu = false;
            int koutu = 0;
            
            for (int i = 35; i <= 37; i++)
            {
                if (!toitu && TehaiNormalizeMarge[i] == 2)
                    toitu = true;
                else if (TehaiNormalizeMarge[i] == 3)
                    koutu++;
            }
            
            return (koutu == 2 && toitu);
        }

        private bool Is_Sankantu()
        {
            if (Fuurosuu < 3) return false;
            
            int kan = 0;
            for (int i = 0; Fuuro[i] != 0 && i < 20; i += 4)
            {
                if (Fuuro[i] == ANKAN || Fuuro[i] == MINKAN || Fuuro[i] == KAKAN)
                    kan++;
            }
            
            return kan >= 3;
        }

        private bool Is_Honroutou()
        {
            bool jihai = false;
            bool routou = false;
            
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                // 中张牌
                if ((i % 10 > 1 && i % 10 < 9) && i < 30)
                    return false;
                
                // 老头牌
                if ((i % 10 == 1 || i % 10 == 9) && i < 30)
                    routou = true;
                
                // 字牌
                if (i > 30)
                    jihai = true;
            }
            
            return routou && jihai;
        }

        private bool Is_Ryuuiisou()
        {
            // 绿一色的合法牌：2条、3条、4条、6条、8条、发
            int[] greenTiles = { 22, 23, 24, 26, 28, 36 };
            
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                bool isGreen = false;
                foreach (int green in greenTiles)
                {
                    if (i == green)
                    {
                        isGreen = true;
                        break;
                    }
                }
                
                if (!isGreen) return false;
            }
            return true;
        }

        private bool Is_Tinroutou()
        {
            // 老头牌：1万、9万、1筒、9筒、1条、9条
            int[] routouTiles = { 1, 9, 11, 19, 21, 29 };
            
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                bool isRoutou = false;
                foreach (int routou in routouTiles)
                {
                    if (i == routou)
                    {
                        isRoutou = true;
                        break;
                    }
                }
                
                if (!isRoutou) return false;
            }
            return true;
        }

        private bool Is_Honitu()
        {
            int syurui = -1;
            bool jihai = false;
            
            // 查找数牌种类
            for (int i = 0; i <= 29; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                if (syurui == -1)
                {
                    syurui = i / 10; // 获取花色
                }
                else if (i / 10 != syurui)
                {
                    return false; // 有不同花色的数牌
                }
            }
            
            // 检查字牌
            for (int i = 31; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] > 0)
                    jihai = true;
            }
            
            return jihai;
        }

        private bool Is_Tinitu()
        {
            int syurui = -1;
            
            for (int i = 0; i <= 37; i++)
            {
                if (TehaiNormalizeMarge[i] == 0) continue;
                
                if (syurui == -1)
                {
                    syurui = i / 10;
                }
                else if (i / 10 != syurui)
                {
                    return false;
                }
            }
            
            return syurui != 3; // 不能全是字牌
        }

        private bool Is_Ittuu()
        {
            bool[] chk = new bool[9];
            Array.Clear(chk, 0, chk.Length);
            
            for (int i = 0; i < 10; i += 2)
            {
                int tile = kiriwake[i + 1];
                int type = kiriwake[i];
                
                if (type == SYUNTU || type == CHII)
                {
                    if (tile == 1) chk[0] = true;   // 123万
                    else if (tile == 4) chk[1] = true;  // 456万
                    else if (tile == 7) chk[2] = true;  // 789万
                    else if (tile == 11) chk[3] = true; // 123筒
                    else if (tile == 14) chk[4] = true; // 456筒
                    else if (tile == 17) chk[5] = true; // 789筒
                    else if (tile == 21) chk[6] = true; // 123条
                    else if (tile == 24) chk[7] = true; // 456条
                    else if (tile == 27) chk[8] = true; // 789条
                }
            }
            
            return (chk[0] && chk[1] && chk[2]) || 
                   (chk[3] && chk[4] && chk[5]) || 
                   (chk[6] && chk[7] && chk[8]);
        }

        private bool Is_Sansyokudoujyun()
        {
            int[] chk = new int[30];
            Array.Clear(chk, 0, chk.Length);
            
            for (int i = 0; i < 10; i += 2)
            {
                int type = kiriwake[i];
                int tile = kiriwake[i + 1];
                
                if (type == SYUNTU || type == CHII)
                {
                    chk[tile]++;
                }
            }
            
            for (int i = 1; i < 10; i++)
            {
                if (chk[i] > 0 && chk[i + 10] > 0 && chk[i + 20] > 0)
                    return true;
            }
            return false;
        }

        private bool Is_Tyanta()
        {
            bool jihai = false;
            
            for (int i = 0; i < 10; i += 2)
            {
                int type = kiriwake[i];
                int tile = kiriwake[i + 1];
                
                // 雀头
                if (type == TOITU)
                {
                    if (!(tile % 10 == 1 || tile % 10 == 9) && tile < 30)
                        return false;
                    if (tile > 30)
                        jihai = true;
                }
                // 顺子
                else if (type == SYUNTU || type == CHII)
                {
                    if (!(tile % 10 == 1 || tile % 10 == 7))
                        return false;
                }
                // 刻子
                else if (type == ANKO || type == ANKAN || type == MINKAN || 
                         type == KAKAN || type == PON)
                {
                    if (!(tile % 10 == 1 || tile % 10 == 9 || tile > 30))
                        return false;
                    if (tile > 30)
                        jihai = true;
                }
            }
            return jihai;
        }

        private bool Is_Jyuntyanta()
        {
            for (int i = 0; i < 10; i += 2)
            {
                int type = kiriwake[i];
                int tile = kiriwake[i + 1];
                
                // 雀头
                if (type == TOITU)
                {
                    if (!(tile % 10 == 1 || tile % 10 == 9) || tile > 30)
                        return false;
                }
                // 顺子
                else if (type == SYUNTU || type == CHII)
                {
                    if (!(tile % 10 == 1 || tile % 10 == 7))
                        return false;
                }
                // 刻子
                else if (type == ANKO || type == ANKAN || type == MINKAN || 
                         type == KAKAN || type == PON)
                {
                    if (!(tile % 10 == 1 || tile % 10 == 9) || tile > 30)
                        return false;
                }
            }
            return true;
        }

        private bool Is_Toitoi()
        {
            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == SYUNTU || kiriwake[i] == CHII)
                    return false;
            }
            return true;
        }

        private bool Is_Sansyokudoukou()
        {
            int[] chk = new int[50];
            Array.Clear(chk, 0, chk.Length);
            
            for (int i = 0; i < 10; i += 2)
            {
                int type = kiriwake[i];
                int tile = kiriwake[i + 1];
                
                if (type == PON || type == ANKO || type == ANKAN || 
                    type == MINKAN || type == KAKAN)
                {
                    chk[tile]++;
                }
            }
            
            for (int i = 1; i < 10; i++)
            {
                if (chk[i] > 0 && chk[i + 10] > 0 && chk[i + 20] > 0)
                    return true;
            }
            return false;
        }

        private bool Is_Sanankou()
        {
            int chk = 0;
            for (int i = 0; i < 10; i += 2)
            {
                if (kiriwake[i] == ANKO || kiriwake[i] == ANKAN)
                    chk++;
            }
            return chk >= 3;
        }
        // 得分计算
        private void Decision_Score(int fan, int fu)
        {
            int kihonten;
            int tumibou = 300 * Honba;
            if (Ba1500) tumibou = 1500 * Honba;

            // 基本点计算
            switch (fan)
            {
                case 5:
                    kihonten = 2000;
                    break; // 满贯
                case 6:
                case 7:
                    kihonten = 3000;
                    break; // 跳满
                case 8:
                case 9:
                case 10:
                    kihonten = 4000;
                    break; // 倍满
                case 11:
                case 12:
                    kihonten = 6000;
                    break; // 三倍满
                case YAKUMAN:
                    kihonten = 8000;
                    break; // 役满
                case YAKUMAN2:
                    kihonten = 8000 * 2;
                    break; // 两倍役满
                // ... 其他役满倍数
                default:
                    kihonten = fu * (int)Math.Pow(2.0, fan + 2);
                    if (kihonten >= 2000) kihonten = 2000;
                    break;
            }

            // 满贯切上
            if (ManganKiriage)
            {
                if ((fan == 4 && fu == 30) || (fan == 6 && fu == 30))
                    kihonten = 2000;
            }

            // 是否满贯
            IsMangan = (kihonten == 2000);

            // 得分分配
            OyaRon = kihonten * 6;
            OyaTumo = kihonten * 2;
            KoRon = kihonten * 4;
            KoTumoOya = kihonten * 2;
            KoTumoKo = kihonten * 1;

            // 进位
            OyaRon = (OyaRon % 100 > 0) ? OyaRon + 100 - OyaRon % 100 : OyaRon;
            OyaTumo = (OyaTumo % 100 > 0) ? OyaTumo + 100 - OyaTumo % 100 : OyaTumo;
            KoRon = (KoRon % 100 > 0) ? KoRon + 100 - KoRon % 100 : KoRon;
            KoTumoOya = (KoTumoOya % 100 > 0) ? KoTumoOya + 100 - KoTumoOya % 100 : KoTumoOya;
            KoTumoKo = (KoTumoKo % 100 > 0) ? KoTumoKo + 100 - KoTumoKo % 100 : KoTumoKo;

            // 积棒
            OyaRon += tumibou;
            OyaTumo += tumibou / 3;
            KoRon += tumibou;
            KoTumoOya += tumibou / 3;
            KoTumoKo += tumibou / 3;

            // 供托立直棒
            TokutenOya = OyaRon + KyoutakuRiichi * 1000;
            TokutenKo = KoRon + KyoutakuRiichi * 1000;

            // 自摸时最终得分
            TokutenOyaTumo = OyaTumo * 3 + KyoutakuRiichi * 1000;
            TokutenKoTumo = KoTumoOya + KoTumoKo * 2 + KyoutakuRiichi * 1000;

            // 动态添加番数到役名表中（根据是否鸣牌）
            if (Yaku[(int)YakuType.HONITU] && is_fuuro)
            {
                YakuNameTable[(int)YakuType.HONITU] = "混一色 2";
            }
            else if (Yaku[(int)YakuType.HONITU])
            {
                YakuNameTable[(int)YakuType.HONITU] = "混一色 3";
            }

            if (Yaku[(int)YakuType.TINITU] && is_fuuro)
            {
                YakuNameTable[(int)YakuType.TINITU] = "清一色 5";
            }
            else if (Yaku[(int)YakuType.TINITU])
            {
                YakuNameTable[(int)YakuType.TINITU] = "清一色 6";
            }

            if (Yaku[(int)YakuType.TYANTA] && is_fuuro)
            {
                YakuNameTable[(int)YakuType.TYANTA] = "全帯 1";
            }
            else if (Yaku[(int)YakuType.TYANTA])
            {
                YakuNameTable[(int)YakuType.TYANTA] = "全帯 2";
            }

            if (Yaku[(int)YakuType.JYUNTYANTA] && is_fuuro)
            {
                YakuNameTable[(int)YakuType.JYUNTYANTA] = "純全帯 2";
            }
            else if (Yaku[(int)YakuType.JYUNTYANTA])
            {
                YakuNameTable[(int)YakuType.JYUNTYANTA] = "純全帯 3";
            }

            if (Yaku[(int)YakuType.ITTUU] && is_fuuro)
            {
                YakuNameTable[(int)YakuType.ITTUU] = "一気通貫 1";
            }
            else if (Yaku[(int)YakuType.ITTUU])
            {
                YakuNameTable[(int)YakuType.ITTUU] = "一気通貫 2";
            }

            if (Yaku[(int)YakuType.SANSYOKUDOUJYUN] && is_fuuro)
            {
                YakuNameTable[(int)YakuType.SANSYOKUDOUJYUN] = "三色同順 1";
            }
            else if (Yaku[(int)YakuType.SANSYOKUDOUJYUN])
            {
                YakuNameTable[(int)YakuType.SANSYOKUDOUJYUN] = "三色同順 2";
            }
            
            // 宝牌番数动态添加
            if (Yaku[(int)YakuType.DORA] && dora > 0)
            {
                YakuNameTable[(int)YakuType.DORA] = $"ドラ {dora}";
            }
            
            if (Yaku[(int)YakuType.URADORA] && uradora > 0)
            {
                YakuNameTable[(int)YakuType.URADORA] = $"裏ドラ {uradora}";
            }
            
            if (Yaku[(int)YakuType.AKADORA] && akadora > 0)
            {
                YakuNameTable[(int)YakuType.AKADORA] = $"赤ドラ {akadora}";
            }
            
            // 生成役名数组
            int j = 0;
            for (int i = 0; i < Yaku.Length; i++)
            {
                if (Yaku[i] && YakuNameTable.ContainsKey(i))
                {
                    YakuName[j] = YakuNameTable[i];
                    j++;
                    if (j >= YakuName.Length) break;
                }
            }
        }

        // 获取错误代码
        public int GetErrorCode() => ErrorCode;

        // 获取向听数
        public int GetNormalSyanten() => Nsyan;
        public int GetChiitoitsuSyanten() => Tsyan;
        public int GetKokushiSyanten() => Ksyan;

        // 获取役名
        public string[] GetYakuNames() => YakuName;

        public IEnumerable<YakuType> GetTenableScorings() => 
            k_result_yaku.Select(b => Array.IndexOf(k_result_yaku, b))
                .Select(i => (YakuType)Enum.GetValues(typeof(YakuType)).GetValue(i));
        
        // 获取得分
        public int GetOyaRon() => OyaRon;
        public int GetKoRon() => KoRon;
        public int GetOyaTumo() => OyaTumo;
        public int GetKoTumoOya() => KoTumoOya;
        public int GetKoTumoKo() => KoTumoKo;
    }
}