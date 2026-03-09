using System;
using ClassPerson.Registry;
using Fictology.Registry;
using UnityEngine;
using VContainer.Unity;

namespace ClassPerson.Manager
{
    public class GameManager: MonoBehaviour
    {
        public GameObject player;

        private void Start()
        {
            player = Registries.Instance.GetObjectInstance(ObjectRegistry.Player);
        }
    }
}