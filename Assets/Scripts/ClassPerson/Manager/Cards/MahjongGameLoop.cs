using System;
using System.Collections.Generic;
using System.Linq;
using ClassPerson.GameSystem.Mahjong;
using Cysharp.Threading.Tasks;
using Fictology.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = System.Random;

namespace ClassPerson.Manager.Cards
{
    public class MahjongGameLoop: MonoBehaviour
    {
        public Image pointer;
        
        public Image self;
        public Image left;
        public Image right;
        public Image oppo;
        
        public GameObject counterObj;
        public GameObject actionObj;
        public MahjongTile CurrentDiscard;
        public CircularListNode<PlayerManager> Current;
        public PlayerManager mainView;

        private MahjongAction _actions;
        private TimerManager _timer;
        private Stack<MahjongTile> _wall = new();
        private CircularList<GameObject> _hands = new();
        private CircularList<GameObject> _graveyards = new();
        private readonly CircularList<PlayerManager> _players = new();
        private UniTaskCompletionSource<ActionType> _task;

        private bool _shouldBreak;

        private void Start()
        {
            SetMahjongDirection();
            _actions = actionObj.GetComponent<MahjongAction>();
            _timer = counterObj.GetComponent<TimerManager>();
            _timer.CountdownStart += ShowTimer;
            foreach (var player in _players.Nodes)
            {
                if (player.Value.GetDirection() == Direction.East) Current = player;
                _hands.Add(player.Value.gameObject.GetComponentInChildren<HandManager>().gameObject);
                _graveyards.Add(player.Value.gameObject.GetComponentInChildren<GraveyardManager>().gameObject);
            }
            InitTurnTipArrow();
            ShuffleAndDraw13Each();
        }
        
        private void SetMahjongDirection()
        {
            var temp = transform.GetComponentsInChildren<PlayerManager>().ToList();
            var list = new CircularList<PlayerManager>();
            
            temp.ForEach(p =>
            {
                p.SetCanDiscard(p.GetDirection() == Direction.East);
                list.Add(p);
            });

            var node = list.Nodes[new Random().Next(0, 3)];
            node.Value.SetDirection(Direction.East);
            node.Next.Value.SetDirection(Direction.South);
            node.Next.Next.Value.SetDirection(Direction.West);
            node.Next.Next.Next.Value.SetDirection(Direction.North);
            
            foreach (var player in list)
            {
                _players.Add(player);
            }
        }
        
        public async void InitTurnTipArrow()
        {
            var player = _players.First(p => p.GetDirection() == Direction.East);
            var angle = (float)(player.side) * 90;

            const int ticks = 25;
            for (var i = 0; i < ticks; i++)
            {
                var unit = angle / ticks;
                pointer.transform.Rotate(0, 0, unit);
                await UniTask.WaitForFixedUpdate();
            }
        }
        
        private void ShuffleAndDraw13Each()
        {
            _wall = MahjongSystem.Instance.CreateWall();
            foreach (var player in _players)
            {
                for (var j = 0; j < 13; j++)
                {
                    player.AddInitialTile(_wall.Pop());
                }
            }
            
            Current.Value.DrawTile(_wall.Pop());
        }
        
        private void ShowTimer(TimerManager cd) => cd.gameObject.SetActive(true);

        public async void NextTurn()
        {
            
            Current.Value.SetCanDiscard(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5));
            var canCall = await CheckIfCanCallingCard();

            const int ticks = 25;
            var targetAngle = 90f;
            for (var i = 0; i < ticks; i++)
            {
                var unit = targetAngle / ticks;
                pointer.transform.Rotate(0, 0, unit);
                await UniTask.WaitForFixedUpdate();
            }
            Current.Value.DrawTile(_wall.Pop());
            Current.Value.SetCanDiscard(true);
            
            _timer.StopCountdown();
            _timer.CountdownStart += Current.Value.OnTimerStart;
            if (!Current.Value.isBot) _timer.StartCountDown(15);
        }

        private async UniTask<bool> CheckIfCanCallingCard()
        {
            foreach (var player in _players)
            {
                if (!player.CanCallingPong(CurrentDiscard)) continue;

                var n = player.hand.GetTiles().Count(tile =>
                {
                    Debug.Log(tile);
                    return tile == CurrentDiscard;
                });
                Debug.Log($"碰不碰 \"{CurrentDiscard}\"，你有 {n} 张");
                _task = new UniTaskCompletionSource<ActionType>();
                _timer.CountdownEnd += OnCountdownEnd;
                _timer.StartCountDown(15);
                var action = await _task.Task;
                if (action != ActionType.Discard | action != ActionType.None | action != ActionType.Agari)
                {
                    _timer.CountdownEnd -= OnCountdownEnd;
                    Current = _players.Nodes.First(node =>
                        node.Value.Equals(mainView.GetComponent<PlayerManager>()));
                    return true;
                }
            }
            _timer.CountdownEnd -= OnCountdownEnd;
            Current = Current.Next;
            return false;
        }

        private void OnCountdownEnd()
        {
            if (_task != null && !_task.Task.Status.IsCompleted())
            {
                _task.TrySetResult(ActionType.None);
            }
        }
    }

    public enum Direction
    {
        Undefined = 0, East = 1, South = 2, West = 3, North = 4
    }

    public enum Side
    {
        Self = 0, Right = 1, Opposite = 2, Left = 3, 
    }

    public enum ActionType
    {
        None, Discard, Chow, Pong, Gang, Agari
    }
}