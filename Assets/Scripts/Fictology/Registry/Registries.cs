using System;
using System.Collections.Concurrent;
using ClassPerson.Registry;
using Fictology.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fictology.Registry
{
    public class Registries
    {
        private static Registries _instance;
        public static Registries Instance = _instance ??= new Registries();

        private readonly ConcurrentDictionary<string, RegistryObject> Key2ObjectMap = new();
        public Tree<RegistryObject> Tree;

        private Registries()
        {
            // Tree = new Tree<RegistryObject>("Canvas", GameObject.Find("Canvas").AsRegistry(UIRegistry.Instance.Canvas));
        }

        public RegistryKey Register(string rootName, string id, bool allowMultipleInstance = false)
        {
            var key = RegistryKey.Create(rootName, id);
            var registry = RegistryObject.CreatePrefab(key, allowMultipleInstance);
            if (!Key2ObjectMap.TryAdd(key.ToString(), registry))
            {
                Debug.LogError($"注册的预制体不唯一，已存在注册名为：\"{key}\" " +
                               $"的预制体：{Key2ObjectMap[key.ToString()].Value.name}， ");
            }
            return key;
        }

        public GameObject AddSingleton(RegistryKey key) => Key2ObjectMap[key.ToString()].Instantiate();
        public GameObject NewInstance(RegistryKey key, Transform parent) =>
            Key2ObjectMap[key.ToString()]
                .NewInstance(parent);

        public GameObject NewInstance(string path, RegistryKey key, Transform parent) =>
            Key2ObjectMap[key.ToString()]
                .NewInstance(path, parent);


        public RegistryObject GetRegistry(RegistryKey key) => Key2ObjectMap[key.ToString()];
        
        public GameObject GetObjectInstance(RegistryKey key, Transform parent = null)
        {
            var registry = Key2ObjectMap[key.ToString()];
            if (registry.Value is not null) return registry.Value;
            var instance = parent == null ? registry.Instantiate() : registry.Instantiate(parent);
            return instance;
        }

        public TComponent GetComponent<TComponent>(RegistryKey key) => GetObjectInstance(key).GetComponent<TComponent>();
        
        public void EnableObject(RegistryKey key) => GetObjectInstance(key).SetActive(true);
        public void DisableObject(RegistryKey key) => GetObjectInstance(key).SetActive(false);
        
        public void SetPos(RegistryKey key, Vector3 pos)
        {
            var instance = Instance.GetObjectInstance(key);
            instance.transform.position = pos;
        }
    }
}