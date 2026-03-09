using System.Collections.Generic;
using System.Linq;
using ClassPerson.GameSystem.Mahjong;
using ClassPerson.Manager.Cards;
using ClassPerson.Registry;
using Cysharp.Threading.Tasks;
using Fictology.Registry;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ClassPerson.Manager
{
    public class PlayerManager : MonoBehaviour
    {
        public bool isBot;
        public Side side;
        
        public GameObject counterObj;
        public GameObject handObject;
        public GameObject drawnCard;
        public GameObject graveyard;
        public GameObject meldCards;

        public TMP_Text dirLabel;
        public HandManager hand;
        
        private TimerManager _timer;
        private Direction _direction;
        private MahjongGameLoop _gameLoop;
        private static readonly Dictionary<Direction, string> DirDict = new()
        {
            { Direction.East, "東" }, { Direction.South, "南" }, { Direction.West, "西" }, { Direction.North, "北" }
        };
        
        private void Start()
        {
            _timer = counterObj.GetComponent<TimerManager>();
            _gameLoop = transform.parent.GetComponent<MahjongGameLoop>();
            hand = handObject.GetComponent<HandManager>();
            hand.player = this;

            _timer.CountdownEnd += DiscardOnTimerZero;
        }

        public void OnTimerStart(TimerManager timer) => timer.shouldTick = true;

        private void DiscardOnTimerZero()
        {
            if (isBot) return;
            var tile = drawnCard.transform.GetComponentInChildren<TileManager>();
            Discard(tile);
            Destroy(drawnCard.transform.GetChild(0).gameObject);
        }

        public Direction GetDirection() => _direction;
        public void SetDirection(Direction direction)
        {
            _direction = direction;
            dirLabel.text = DirDict[_direction];
        }

        public void SortCards()
        {
            var children = handObject.transform.Cast<Transform>().ToList();
            hand.Cards.Clear();
            hand.Cards.AddRange(children.Select(t => t.gameObject.GetComponent<TileManager>()));
            hand.Cards.Sort((tileA, tileB) => tileA.Mahjong.Value.CompareTo(tileB.Mahjong.Value));

            foreach (var t in children)
            {
                var tObj = t.gameObject.GetComponent<TileManager>();
                var tile = tObj.Mahjong;
                foreach (var index in from card in hand.Cards where card.Mahjong.Equals(tile) 
                         select hand.Cards.IndexOf(tObj))
                {
                    t.SetSiblingIndex(index);
                }
            }
        }

        public void SetCanDiscard(bool canDiscard) => hand.Cards.ForEach(card => card.canDiscard = canDiscard);

        public void AddInitialTile(MahjongTile mahjong)
        {
            if (isBot)
            {
                var inst = mahjong.Instantiate(handObject.transform, MahjongState.Hidden);
                var tile = inst.GetComponent<TileManager>();
                tile.canDiscard = false;
                hand.Cards.Add(tile);
                return;
            }
            
            hand.AddTileToMyHand(mahjong);
            SortCards();
            SortCards();
        }

        public void DrawTile(MahjongTile mahjong)
        {
            if (isBot)
            {
                var inst = mahjong.Instantiate(handObject.transform, MahjongState.Hidden);
                var tile = inst.GetComponent<TileManager>();
                tile.canDiscard = false;
                hand.Cards.Add(tile.GetComponent<TileManager>());
                AIDiscard();
                return;
            }
            mahjong.Instantiate(drawnCard.transform, MahjongState.Hidden);
            SortCards();
            SortCards();
        }

        private void AIDiscard()
        {
            var index = MahjongAI.Instance.GetBestDiscard(hand.GetTiles(), hand.Melds);
            var obj = hand.Cards[index].gameObject;
            var mahjong = hand.GetTiles()[index];
            
            _gameLoop.CurrentDiscard = mahjong;
            hand.Cards.RemoveAt(index);
            
            AddToGraveyard(mahjong);
            Destroy(obj);
            NextTurn();
        }

        public void Discard(TileManager tile)
        {
            _gameLoop.CurrentDiscard = tile.Mahjong;
            tile.canDiscard = false; 
            AddToGraveyard(tile.Mahjong);
            NextTurn();
        }

        public void AddToGraveyard(MahjongTile mahjong)
        {
            var instance = mahjong.Instantiate(graveyard.transform, MahjongState.FaceUp);
            instance.GetComponent<TileManager>().canDiscard = false;
        }

        public bool CanCallingPong(MahjongTile discard) => hand.GetTiles().Count(tile => tile == discard) >= 2;
        public bool CanCallingGang(MahjongTile discard) => hand.GetTiles().Count(tile => tile == discard) >= 3;
        public void NextTurn() => _gameLoop.NextTurn();
    }
}
