using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fictology.Registry
{
    public class RegistryObject: WeakReference, IDisposable
    {
        public readonly RegistryKey Key;
        public GameObject Value { get; }
        public bool IsActive { get; private set; }
        public bool MustBeInstantiated { get; private init; }
        public bool AllowMultipleInstance { get; private init; }
        private RegistryObject(RegistryKey key, GameObject instance, bool allowMultipleInstance = false) : base(instance)
        {
            Value = instance;
            IsActive = instance != null && instance.activeInHierarchy;
            AllowMultipleInstance = allowMultipleInstance;
        }

        public static RegistryObject Create(RegistryKey key, GameObject instance, bool allowMultipleInstance = false) 
            => new (key, instance, allowMultipleInstance);

        public static RegistryObject CreatePrefab(RegistryKey key, bool allowMultipleInstance = false)
        {
            var inst = new RegistryObject(key, null, allowMultipleInstance)
            {
                MustBeInstantiated = true,
                AllowMultipleInstance = allowMultipleInstance
            };
            return inst;
        }
        
        public GameObject Instantiate() => MustBeInstantiated 
            ? (GameObject) Object.Instantiate(Resources.Load(Key.PrefabPath)) 
            : Value;

        public GameObject Instantiate(Transform parent) => MustBeInstantiated 
            ? (GameObject) Object.Instantiate(Resources.Load(Key.PrefabPath), parent) 
            : Value;

        public GameObject NewInstance(Transform parent) => AllowMultipleInstance 
                ? (GameObject)Object.Instantiate(Resources.Load(Key.PrefabPath), parent) 
                : Value 
                    ? Value
                    : (GameObject)Object.Instantiate(Resources.Load(Key.PrefabPath), parent);
        
        public GameObject NewInstance(string path, Transform parent) => AllowMultipleInstance 
            ? (GameObject)Object.Instantiate(Resources.Load(path), parent) 
            : Value 
                ? Value
                : (GameObject)Object.Instantiate(Resources.Load(path), parent);
            

        public void Dispose()
        {
            Object.Destroy(Value);
        }

        public void SetActive(bool shouldActivate)
        {
            if (Value == null) return;
            Value.SetActive(shouldActivate);
            IsActive = Value.activeInHierarchy;
        }
    }
}