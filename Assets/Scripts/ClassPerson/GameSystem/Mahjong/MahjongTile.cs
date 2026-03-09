using System;
using System.Runtime.CompilerServices;
using ClassPerson.Manager;
using ClassPerson.Manager.Cards;
using ClassPerson.Registry;
using Fictology.Registry;
using Fictology.Util;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ClassPerson.GameSystem.Mahjong
{
    public abstract class MahjongTile: IMahjongGroup
    {
        public static Suit Character = Suit.Any.AddFlag(Suit.Dragon).AddFlag(Suit.Wind);
        public override bool Equals(object obj)
        {
            // 引用相等
            return ReferenceEquals(this, obj);
        }

        // 添加GetHashCode重写，与Equals保持一致
        public override int GetHashCode()
        {
            // 使用RuntimeHelpers.GetHashCode确保每个实例有唯一的哈希码
            // 这对于Dictionary/HashSet等基于哈希的集合很重要
            return RuntimeHelpers.GetHashCode(this);
        }

        public const int AnyValue = -1;
        public static readonly AnyCard Any = new (Suit.Any, AnyValue);
        
        public static MahjongTile New(int value)
        {
            return value switch
            {
                >= 0 and < 9 => new RankCard(Suit.Character, value),
                >= 9 and < 18 => new RankCard(Suit.Circle, value),
                >= 18 and < 27 => new RankCard(Suit.Bamboo, value),
                >= 27 and < 31 => new HonorCard(Suit.Wind, value),
                >= 31 => new HonorCard(Suit.Dragon, value),
                _ => new AnyCard(Suit.Any, -1)
            };
        }

        public GameObject Instantiate(Transform parent, MahjongState state)
        {
            var inst = Registries.Instance.NewInstance("Prefabs/Object/Mahjong", ObjectRegistry.Mahjong, parent);
            var tile = inst.GetComponent<TileManager>();
            tile.Mahjong = this;
            tile.mahjongImg.sprite = Resources.Load<Sprite>($"Sprites/Mahjong/{Value}-{(int)state}");
            return inst;
        }

        public const int Char1 = 0;
        public const int Char2 = 1;
        public const int Char3 = 2;
        public const int Char4 = 3;
        public const int Char5 = 4;
        public const int Char6 = 5;
        public const int Char7 = 6;
        public const int Char8 = 7;
        public const int Char9 = 8;
        
        public const int Dots1 = 9;
        public const int Dots2 = 10;
        public const int Dots3 = 11;
        public const int Dots4 = 12;
        public const int Dots5 = 13;
        public const int Dots6 = 14;
        public const int Dots7 = 15;
        public const int Dots8 = 16;
        public const int Dots9 = 17;
        
        public const int Bamboo1 = 18;
        public const int Bamboo2 = 19;
        public const int Bamboo3 = 20;
        public const int Bamboo4 = 21;
        public const int Bamboo5 = 22;
        public const int Bamboo6 = 23;
        public const int Bamboo7 = 24;
        public const int Bamboo8 = 25;
        public const int Bamboo9 = 26;
        
        public const int East    = 27;
        public const int South   = 28;
        public const int West    = 29;
        public const int North   = 30;
        public const int White   = 31;
        public const int Green   = 32;
        public const int Red     = 33;
        
        public static int RedChar = 38;
        public static int RedDots = 39;
        public static int RedBamboo = 40;
        
        public readonly MahjongTag MahjongTag;
        public readonly Suit Suit;
        public readonly int Value;
        protected MahjongTile(Suit suit, int value, MahjongTag mahjongTag = MahjongTag.Any)
        {
            Suit = suit;
            Value = value;
            MahjongTag = mahjongTag;
        }

        public abstract MahjongTag GetTags();
        public static bool operator ==([NotNull] MahjongTile left, [NotNull] MahjongTile right) => left.Value == right.Value;
        public static bool operator !=([NotNull] MahjongTile left, [NotNull] MahjongTile right) => !(left == right);
        public static MahjongTile operator +([NotNull] MahjongTile left, int offset) => MahjongSystem.Instance.Get(left.Value + offset);

        public int Count() => 1;
        public override string ToString()
        {
            return Value switch
            {
                AnyValue => Suit.ContainsFlag(Suit.Character)
                    ? "萬字百搭牌"
                    : Suit.ContainsFlag(Suit.Circle)
                        ? "饼字百搭牌"
                        : Suit.ContainsFlag(Suit.Bamboo)
                            ? "索字百搭牌"
                            : Suit.ContainsFlag(Suit.Wind)
                                ? "风牌百搭牌"
                                : Suit.ContainsFlag(Suit.Dragon)
                                    ? "箭牌百搭牌"
                                    : "万能百搭牌",
                Char1 => "一萬",
                Char2 => "二萬",
                Char3 => "三萬",
                Char4 => "四萬",
                Char5 => "五萬",
                Char6 => "六萬",
                Char7 => "七萬",
                Char8 => "八萬",
                Char9 => "九萬",

                Dots1 => "一饼",
                Dots2 => "二饼",
                Dots3 => "三饼",
                Dots4 => "四饼",
                Dots5 => "五饼",
                Dots6 => "六饼",
                Dots7 => "七饼",
                Dots8 => "八饼",
                Dots9 => "九饼",

                Bamboo1 => "一索",
                Bamboo2 => "二索",
                Bamboo3 => "三索",
                Bamboo4 => "四索",
                Bamboo5 => "五索",
                Bamboo6 => "六索",
                Bamboo7 => "七索",
                Bamboo8 => "八索",
                Bamboo9 => "九索",
                
                East => "东风",
                West => "西风",
                South => "南风",
                North => "北风",
                White => "白板",
                Green => "发财",
                Red => "红中"

            };
        }
    }

    public enum MahjongState
    {
        Hidden = 0,
        FaceUp = 1,
        Horizontal = 2
    }
    
    public class AnyCard : MahjongTile
    {
        public AnyCard(Suit suit, int value, MahjongTag mahjongTag = MahjongTag.Any) : base(suit, value, mahjongTag)
        {
        }

        public override MahjongTag GetTags() => MahjongTag.Any;
    }
    

    [Flags]
    public enum Suit
    {
        Any = 1,
        Character = 2,
        Circle = 4,
        Bamboo = 8,
        Dragon = 16,
        Wind = 32,
        Flower = 64
        
    }

    [Flags]
    public enum MahjongTag
    {
        Any = 1,
        Crimson = 2,
        Symmetry = 4,
        EvenNumber = 8
    }
}