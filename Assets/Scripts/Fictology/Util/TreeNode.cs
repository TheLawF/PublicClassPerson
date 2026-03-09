using System;
using System.Collections.Generic;
using System.Linq;

namespace Fictology.Util
{
    public class TreeNode<TValue>
    {
        public string Name { get; }
        public TValue Value { get; set; }
        private Dictionary<string, TreeNode<TValue>> _children;

        public TreeNode(string name, TValue value)
        {
            Name = name;
            Value = value;
            _children = new Dictionary<string, TreeNode<TValue>>(StringComparer.OrdinalIgnoreCase);
        }

        public TreeNode<TValue> AddChild(string childName, TValue value)
        {
            if (string.IsNullOrEmpty(childName))
                throw new ArgumentException("Child name cannot be null or empty.");

            if (_children.ContainsKey(childName))
                throw new ArgumentException($"Child node '{childName}' already exists.");

            var childNode = new TreeNode<TValue>(childName, value);
            _children.Add(childName, childNode);
            return childNode;
        }
        

        public TreeNode<TValue> AddChild(TreeNode<TValue> childNode) => AddChild(childNode.Name, childNode.Value);

        public TreeNode<TValue> GetChild(string childName)
        {
            _children.TryGetValue(childName, out var node);
            return node;
        }

        public void ReplaceChild(string childName, TValue newChildValue)
        {
            _children[childName] = new TreeNode<TValue>(childName, newChildValue);
        }

        public List<TreeNode<TValue>> GetChildren() => _children.Values.ToList();

        public List<TValue> GetChildrenValues()
        {
            var list = new List<TValue>();
            list.AddRange(_children.Values.Select(node => node.Value).ToList());
            _children.Values.ToList().ForEach(node =>
            {
                if (!node.HasChild()) return;
                var recursiveList = node.GetChildrenValues();
                list.AddRange(recursiveList);
            });
            return list;
        }
        
        public List<TreeNode<TValue>> GetAllChildren()
        {
            var list = new List<TreeNode<TValue>>();
            list.AddRange(_children.Values.ToList());
            _children.Values.ToList().ForEach(node =>
            {
                if (!node.HasChild()) return;
                var recursiveList = node.GetAllChildren();
                list.AddRange(recursiveList);
            });
            return list;
        }

        public bool HasChild() => _children.Count > 0;
    }
    
    public class Tree<TValue>
    {
        public TreeNode<TValue> Root { get; }

        public Tree(string rootName, TValue rootValue)
        {
            if (string.IsNullOrEmpty(rootName))
                throw new ArgumentException("Root name cannot be null or empty.");

            Root = new TreeNode<TValue>(rootName, rootValue);
        }

        public List<TreeNode<TValue>> Flat()
        {
            return Root.GetAllChildren();
        }

        public TreeNode<TValue> GetNode(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Length == 0)
                return Root;

            var segments = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var current = Root;

            foreach (var segment in segments)
            {
                current = current?.GetChild(segment);
                if (current == null) break;
            }

            return current;
        }

        public bool Contains(TValue value) => Flat().Select(node => node.Value).Contains(value);

        public void Replace(string path, TValue newValue)
        {
            var node = GetNode(path);
            if (TryGetParent(node, out var parent))
            {
                parent.ReplaceChild(path, newValue);
            }
        }

        public bool GetIfContains(TValue value, out TreeNode<TValue> node)
        {
            node = Flat()
                .Select(n => new { n.Value, Node = n })
                .First(vn => vn.Value.Equals(value)).Node;

            return Contains(value);
        }
        
        /// <summary>
        /// 判断指定节点是否位于整棵树内
        /// </summary>
        /// <param name="node">要查找的节点</param>
        /// <returns>如果节点在树内返回true，否则返回false</returns>
        public bool ContainsNode(TreeNode<TValue> node)
        {
            if (node == null)
                return false;

            return FindNodeRecursive(Root, node);
        }

        public bool TryGetParent(TreeNode<TValue> child, out TreeNode<TValue> parent)
        {
            var treeNode = Root.GetAllChildren().FirstOrDefault(node => node.Name == child.Name);
            if (treeNode != null)
            {
                parent = treeNode;
                return true;
            }

            parent = null;
            return false;
        }
        
        /// <summary>
        /// 递归查找节点
        /// </summary>
        private bool FindNodeRecursive(TreeNode<TValue> currentNode, TreeNode<TValue> targetNode)
        {
            if (currentNode == targetNode)
                return true;

            foreach (var child in currentNode.GetChildren())
            {
                if (FindNodeRecursive(child, targetNode))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 获取指定节点在树中的路径
        /// </summary>
        /// <param name="node">要查找路径的节点</param>
        /// <returns>节点路径（以/分隔），如果节点不在树中返回null</returns>
        public string GetNodePath(TreeNode<TValue> node)
        {
            if (node == null)
                return null;

            var pathSegments = new List<string>();
            if (FindNodePathRecursive(Root, node, pathSegments))
            {
                return string.Join("/", pathSegments);
            }

            return null;
        }

        /// <summary>
        /// 递归查找节点路径
        /// </summary>
        private bool FindNodePathRecursive(TreeNode<TValue> currentNode, TreeNode<TValue> targetNode, List<string> pathSegments)
        {
            // 将当前节点名称加入路径
            pathSegments.Add(currentNode.Name);

            // 如果找到目标节点
            if (currentNode == targetNode)
                return true;

            // 在子节点中递归查找
            foreach (var child in currentNode.GetChildren())
            {
                if (FindNodePathRecursive(child, targetNode, pathSegments))
                {
                    return true;
                }
            }

            // 如果在当前分支没找到，移除当前节点
            pathSegments.RemoveAt(pathSegments.Count - 1);
            return false;
        }

        /// <summary>
        /// 获取指定节点在树中的路径（带根节点）
        /// </summary>
        /// <param name="node">要查找路径的节点</param>
        /// <returns>节点路径（包含根节点，以/分隔），如果节点不在树中返回null</returns>
        public string GetFullNodePath(TreeNode<TValue> node)
        {
            if (node == null)
                return null;

            var pathSegments = new List<string>();
            if (FindNodePathRecursive(Root, node, pathSegments))
            {
                return "/" + string.Join("/", pathSegments);
            }

            return null;
        }
    }
}