using System;
using System.Collections.Generic;
using System.Linq;
using ClassPerson.GameSystem.Mahjong;
using Cysharp.Threading.Tasks;
using Fictology.UnityEditor;
using Fictology.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ClassPerson.Manager.Cards
{
    public class MahjongAction: MonoBehaviour
    {
        public GameObject timer;
        public GameObject board;
        public GameObject playerObject;
        public GameObject handObject;
        public TMP_Text endOfGameLabel;

        private HandManager _hand;
        private PlayerManager _player;
        private MahjongGameLoop _gameLoop;
        private TimerManager _timer;
        
        private List<MahjongTile> _cards = new();
        private List<MahjongMeld> _melds = new();
        
        public static event Action OnButtonClicked;

        [DisplayOnly]
        public bool isActionTaken = false;

        private void Start()
        {
            _hand = handObject.GetComponent<HandManager>();
            _player = playerObject.GetComponent<PlayerManager>();
            _gameLoop = board.GetComponent<MahjongGameLoop>();
            _timer = timer.GetComponent<TimerManager>();

            _timer.CurrentChanged += TimeUp;
            
        }

        public async UniTask<bool> WaitForActionsTaken()
        {
            
            return isActionTaken;
        }

        public void Agari()
        {
            var tile = _player.drawnCard.GetComponentInChildren<TileManager>();
            var winningTile = tile is null ? _gameLoop.CurrentDiscard : tile.Mahjong;
            
            _cards.AddRange(_hand.Cards.Select(manager => manager.Mahjong));
            _melds = new List<MahjongMeld>(_hand.Melds);
            _cards.Add(winningTile);

            Debug.Log($"{string.Join(",", _hand.Cards.Select(t => t.Mahjong))}");

            MahjongSystem.Instance.CheckVictory(_cards, _melds, winningTile, out var mjScore);
            // _player.drawnCard.DestroyAllChildren();
            _player.graveyard.DestroyAllChildren();
            // handObject.DestroyAllChildren();
            var scores = MahjongSystem.Instance.GetScoreList(_cards, _melds, winningTile);
            endOfGameLabel.text = string.Join("\n", scores); // MahjongSystem.Instance.GetCheckingLog(mjScore.GetErrorCode());
        }
        public void Chow()
        {
            _melds.Add(new MahjongMeld(MeldType.Chi, new List<MahjongTile> {_gameLoop.CurrentDiscard}));
            isActionTaken = true;
        }

        public void Pong()
        {
            _melds.Add(new MahjongMeld(MeldType.Pon, new List<MahjongTile> {_gameLoop.CurrentDiscard}));
            OnButtonClicked?.Invoke();
            isActionTaken = true;
        }

        public void Gang()
        {
            _melds.Add(new MahjongMeld(MeldType.MeldedKong, new List<MahjongTile> {_gameLoop.CurrentDiscard}));
            isActionTaken = true;
        }

        public void Skip()
        {
            isActionTaken = false;
        }

        public void TimeUp(int oldVal, int newVal)
        {
            if (newVal > 0) return;
            isActionTaken = false;
        }
    }
}