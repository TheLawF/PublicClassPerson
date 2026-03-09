using System;
using System.Collections.Generic;
using System.Linq;
using Fictology.Registry;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace Fictology.Util
{
    public static class GameObjectUtils
    {
        public static void DestroyAllChildren(this GameObject parent)
        {
            foreach (var child in parent.transform.Cast<Transform>()) Object.Destroy(child.gameObject);
        }

        public static Tree<RegistryObject> AddToTree(this Tree<RegistryObject> tree, string path, RegistryKey key) {
            tree.Root.AddChild(path, RegistryObject.CreatePrefab(key));
            return tree;
        }

        public static Tree<RegistryObject> AddToTreeUnder(this Tree<RegistryObject> tree, RegistryKey parentKey, RegistryKey key)
        {
            var parentRegiistry = Registries.Instance.GetRegistry(parentKey);
            if (!tree.GetIfContains(parentRegiistry, out var parentNode)) return tree;
            var fullPath = tree.GetFullNodePath(parentNode);
            parentNode.AddChild(fullPath, RegistryObject.CreatePrefab(key));
            return tree;
        }

        public static RegistryObject AsRegistry(this GameObject gameObject, RegistryKey key)
        {
            var tree = Registries.Instance.Tree;
            var registry = RegistryObject.Create(key, gameObject);
            if (!tree.GetIfContains(Registries.Instance.GetRegistry(key), out var node)) return registry;
            if (!tree.TryGetParent(node, out var parent)) return registry;
            
            var path = tree.GetNodePath(node);
            parent.ReplaceChild(path, registry);

            return registry;
        }
        
        public static void InstantiateOnHierarchy(this Tree<RegistryObject> tree)
        {
            tree.Root.GetAllChildren().ForEach(node =>
            {
                if (!tree.TryGetParent(node, out var parent)) return;
                if (parent.Value.MustBeInstantiated) return;
                node.Value.Instantiate(parent.Value.Value.transform);
            });
        }

        public static void SetLayout()
        {
            
        }

        public static Tree<RegistryObject> SetPos(this Tree<RegistryObject> tree, RegistryKey key, Vector3 pos)
        {
            var instance = Registries.Instance.GetObjectInstance(key);
            instance.transform.position = pos;
            return tree;
        }
        
        public static T AddFlag<T>(this T self, T flag) where T: Enum
        {
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false))
            {
                throw new ArgumentException($"枚举类型 {typeof(T).Name} 必须标记为 [Flags] 特性");
            }

            // 使用ulong进行位操作以兼容所有枚举基础类型（byte, sbyte, short, ushort, int, uint, long, ulong）
            var valueAsUlong = Convert.ToUInt64(self);
            var flagAsUlong = Convert.ToUInt64(flag);
        
            // 执行位或操作合并标志位
            var resultAsUlong = valueAsUlong | flagAsUlong;
        
            // 将结果转换回枚举类型
            return (T)Enum.ToObject(typeof(T), resultAsUlong);
        }

        public static bool ContainsFlag<T>(this T self, T flag) where T : Enum
        {
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false))
            {
                throw new ArgumentException($"枚举类型 {typeof(T).Name} 必须标记为 [Flags] 特性");
            }
            var valueAsUlong = Convert.ToUInt64(self);
            var flagAsUlong = Convert.ToUInt64(flag);
        
            // 检查是否完全包含所有位
            return (valueAsUlong & flagAsUlong) == flagAsUlong;
        }
        
        public static bool ContainsAny<T>(this T self, params T[] flags) where T : Enum
        {
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false))
            {
                throw new ArgumentException($"枚举类型 {typeof(T).Name} 必须标记为 [Flags] 特性");
            }
            var valueAsUlong = Convert.ToUInt64(self);
            return flags.Any(flag => (valueAsUlong & Convert.ToUInt64(flag)) == Convert.ToUInt64(flag));
        }
        
        /// <summary>
        /// 清除所有标志位，返回枚举的零值
        /// </summary>
        public static T ClearFlags<T>(this T value) where T : Enum
        {
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false))
            {
                throw new ArgumentException($"枚举类型 {typeof(T).Name} 必须标记为 [Flags] 特性");
            }
        
            // 返回枚举的默认值（零值）
            return (T)Enum.ToObject(typeof(T), 0UL);
        }
        
        /// <summary>
        /// 判断当前枚举值是否只包含单个标志位（幂等检查）
        /// </summary>
        public static bool ContainSingleFlag<T>(this T value) where T : Enum
        {
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false))
            {
                throw new ArgumentException($"枚举类型 {typeof(T).Name} 必须标记为 [Flags] 特性");
            }
        
            var valueAsUlong = Convert.ToUInt64(value);
        
            // 检查是否为2的幂次（只有一个位被设置）
            // 对于0，返回false（因为0不是单个标志位）
            return valueAsUlong != 0 && (valueAsUlong & (valueAsUlong - 1)) == 0;
        }

        public static void FillWith<T>(this List<T> list, Func<int, T> init)
        {
            var count = list.Capacity;
            for (var i = 0; i < count; i++)
            {
                list.Add(init(i));
            }
        }
        
        public static Stack<T> Shuffle<T>(this List<T> list)
        {
            var rng = new Random();
    
            // Fisher-Yates洗牌
            var newList = new List<T>(list);
            for (var i = newList.Count - 1; i > 0; i--) {
                var j = rng.Next(i + 1);
                (newList[j], newList[i]) = (newList[i], newList[j]);
            }

            return new Stack<T>(newList);
        }
    }
}