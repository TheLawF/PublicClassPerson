using System.Collections;
using System.Collections.Generic;

namespace Fictology.Util
{
    public class CircularList<T>: IEnumerator<T>, IEnumerable<T>
    {
        public CircularListNode<T> Head { get; private set; }
        public CircularListNode<T> Tail { get; private set; }
        public int Depth { get; private set; }
        private List<T> _list = new List<T>();
        public List<CircularListNode<T>> Nodes { get; private set; } = new();

        // 初始化一个空链表
        public CircularList(int capacity = 4)
        {
            Head = null;
            Tail = null;
        }

        /// <summary>
        /// 在链表末尾添加一个新节点，并使其首尾相连
        /// </summary>
        public void Add(T data)
        {
            var newNode = new CircularListNode<T>(data);
            _list.Add(data);
            Nodes.Add(newNode);
            if (Head == null) // 链表为空时
            {
                Head = newNode;
                Tail = newNode;
                newNode.Next = Head; // 关键：让第一个节点指向自己，形成循环
                Depth = 1; // 只有一个节点时深度为1
            }
            else // 链表已有节点
            {
                Tail.Next = newNode; // 当前尾节点指向新节点
                Tail = newNode;      // 更新尾节点为新节点
                Tail.Next = Head;    // 关键：让新尾节点的 Next 指向头节点
                Depth++;
            }
        }

        public List<T> TakeAll() => _list;

        public List<T> Take(int count)
        {
            var list = new List<T>(count);
            Head.ToListRecursive(list, 0, count);
            return list;
        }
        public T this[int index] => _list[index];

        public CircularListNode<T> Next(CircularListNode<T> currentNode) => Head == null ? null : currentNode?.Next;

        /// <summary>
        /// 遍历链表（示例方法，循环输出元素，防止无限循环）
        /// </summary>
        public void Traverse(int maxIterations = 10)
        {
            if (Head == null)
            {
                return;
            }

            var current = Head;
            var count = 0;
            do
            {
                current = current.Next;
                count++;
            } while (current != Head && count < maxIterations); // 回到头节点时停止
        }

        public bool MoveNext()
        {
            return _list.IndexOf(Head.Value) != Depth;
        }

        public void Reset()
        {
            Head = null;
            Tail = null;
            Depth = 0; // 只有一个节点时深度为1
        }

        public T Current => Head.Value;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
    public class CircularListNode<T>
    {
        public T Value { get; set; }
        public CircularListNode<T> Next { get; set; }

        public CircularListNode(T value)
        {
            Value = value;
            Next = null; // 初始化时 Next 为空
        }
        
        public void ToListRecursive(List<T> list, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth || list == null) return;
            list.Add(Value);
            currentDepth++;
        
            Next?.ToListRecursive(list, currentDepth, maxDepth);
        }
    }
}