using System;
using System.Linq;
using ClassPerson.GameSystem.Mahjong;
using ClassPerson.Manager.Cards;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NotImplementedException = System.NotImplementedException;

namespace ClassPerson.Manager.Cards
{
    public class TileManager: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private HandManager _hand;
        private Transform _parent;
        private Transform _transform;
        private PlayerManager _player;
        
        public Image mahjongImg;
        public MahjongTile Mahjong;
        public bool canDiscard;

        private int _moveCount;
        private void Start()
        {
            _transform = transform;
            _parent = _transform.parent;
            _player = _parent.parent.gameObject.GetComponent<PlayerManager>();
            
            _hand = _parent.GetComponent<HandManager>();
            mahjongImg = gameObject.GetComponent<Image>();
        }
        
        public void DiscardOnClick()
        {
            if (!canDiscard) return;
            var drawn = _player.drawnCard.GetComponentInChildren<TileManager>();
            if (_parent == _player.drawnCard.transform)
            {
                _player.Discard(this);
                _player.SortCards();
                Destroy(gameObject);
                return;
            }

            if (drawn == null) return;
            drawn.Mahjong.Instantiate(_player.handObject.transform, MahjongState.Hidden);
            _player.Discard(this);
            _player.SortCards();
            Destroy(gameObject);
            Destroy(drawn.gameObject);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!canDiscard) return;
            var pos = _transform.position;
            var newPos = pos + new Vector3(0, 20, 0);
            _transform.position = newPos;
            _moveCount++;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_moveCount == 0) return;
            var pos = _transform.position;
            var newPos = pos - new Vector3(0, 20, 0);
            _transform.position = newPos;
            _moveCount = 0;
        }
    }
}