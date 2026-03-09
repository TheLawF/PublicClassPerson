using System;
using System.Collections.Generic;
using System.Linq;
using ClassPerson.GameSystem.Mahjong;
using ClassPerson.Registry;
using Fictology.Registry;
using Fictology.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClassPerson.Manager.Cards
{
    public class HandManager: MonoBehaviour
    {
        private Transform _parent;
        private Transform _transform;
        
        public PlayerManager player;
        
        public readonly List<TileManager> Cards = new();
        public readonly List<MahjongMeld> Melds = new();

        private void Start()
        {
            _transform = transform;
            _parent = _transform.parent;
            player = _parent.GetComponent<PlayerManager>();
        }

        public List<MahjongTile> GetTiles() => Cards.Select(t => t.Mahjong).ToList();

        public void AddTileToMyHand(MahjongTile mahjong)
        {
            var inst = mahjong.Instantiate(_transform, MahjongState.Hidden);
            Cards.Add(inst.GetComponent<TileManager>());
        }

    }
}