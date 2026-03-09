using System.Collections.Generic;
using UnityEngine;
using Or = Unity.VisualScripting.Or;

namespace Fictology.UI
{
    public enum LayoutGroupType
    {
        Horizontal,  // 水平
        Vertical,    // 垂直
        Grid,        // 网格
        Flow,        // 流式
        Stack,       // 堆叠
        Tab,         // 标签页
        Accordion,   // 手风琴
        Carousel     // 轮播
    }
    
    /// <summary>
    /// 布局组基类
    /// </summary>
    public abstract class BaseLayoutGroup : MonoBehaviour
    {
        [Header("布局组设置")]
        public string GroupId;
        public LayoutGroupType GroupType;
        public RectTransform Container;
        
        [Header("布局设置")]
        public Vector2 Spacing = Vector2.zero;
        public RectOffset Padding = new RectOffset(0, 0, 0, 0);
        public TextAnchor ChildAlignment = TextAnchor.UpperLeft;
        public bool ControlChildSize = true;
        public bool ControlChildPosition = true;
        
        [Header("自适应")]
        public bool FitWidth = false;
        public bool FitHeight = false;
        public float MaxWidth = 0f;
        public float MaxHeight = 0f;
        
        // 子元素列表
        protected readonly List<RectTransform> m_Children = new List<RectTransform>();
        
        // 是否需要刷新
        private bool m_NeedRefresh = true;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize(string groupId, RectTransform container)
        {
            GroupId = groupId;
            Container = container;
        }
        
        /// <summary>
        /// 添加子元素
        /// </summary>
        public virtual void AddChild(RectTransform child)
        {
            if (child == null || m_Children.Contains(child)) return;
            
            child.SetParent(Container, false);
            m_Children.Add(child);
            m_NeedRefresh = true;
        }
        
        /// <summary>
        /// 移除子元素
        /// </summary>
        public virtual void RemoveChild(RectTransform child)
        {
            if (child == null) return;
            
            m_Children.Remove(child);
            m_NeedRefresh = true;
        }
        
        /// <summary>
        /// 插入子元素
        /// </summary>
        public virtual void InsertChild(int index, RectTransform child)
        {
            if (child == null || index < 0 || index > m_Children.Count) return;
            
            child.SetParent(Container, false);
            m_Children.Insert(index, child);
            m_NeedRefresh = true;
        }
        
        /// <summary>
        /// 获取子元素
        /// </summary>
        public RectTransform GetChild(int index)
        {
            if (index < 0 || index >= m_Children.Count) return null;
            return m_Children[index];
        }
        
        /// <summary>
        /// 获取子元素数量
        /// </summary>
        public int GetChildCount()
        {
            return m_Children.Count;
        }
        
        /// <summary>
        /// 清空所有子元素
        /// </summary>
        public virtual void Clear()
        {
            foreach (var child in m_Children)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
            m_Children.Clear();
            m_NeedRefresh = true;
        }
        
        /// <summary>
        /// 刷新布局
        /// </summary>
        public virtual void RefreshLayout()
        {
            if (!m_NeedRefresh) return;
            
            CalculateLayout();
            m_NeedRefresh = false;
        }
        
        /// <summary>
        /// 计算布局
        /// </summary>
        protected abstract void CalculateLayout();
        
        /// <summary>
        /// 标记需要刷新
        /// </summary>
        public void MarkDirty()
        {
            m_NeedRefresh = true;
        }
        
        void LateUpdate()
        {
            if (m_NeedRefresh)
            {
                RefreshLayout();
            }
        }
    }
    public class LayoutGroup: MonoBehaviour
    {
        private static LayoutGroup m_Instance;
        public static LayoutGroup Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject go = new GameObject("LayoutGroup");
                    m_Instance = go.AddComponent<LayoutGroup>();
                    DontDestroyOnLoad(go);
                }
                return m_Instance;
            }
        }
        
        // 布局组
        private readonly Dictionary<string, BaseLayoutGroup> m_LayoutGroups = 
            new Dictionary<string, BaseLayoutGroup>();
        
        /// <summary>
        /// 创建布局组
        /// </summary>
        public T CreateLayoutGroup<T>(string groupId, RectTransform container) where T : BaseLayoutGroup
        {
            if (m_LayoutGroups.ContainsKey(groupId))
            {
                Debug.LogWarning($"布局组已存在: {groupId}");
                return m_LayoutGroups[groupId] as T;
            }
            
            GameObject go = new GameObject(groupId);
            go.transform.SetParent(container, false);
            
            T layoutGroup = go.AddComponent<T>();
            layoutGroup.Initialize(groupId, container);
            
            m_LayoutGroups[groupId] = layoutGroup;
            return layoutGroup;
        }
        
        /// <summary>
        /// 获取布局组
        /// </summary>
        public T GetLayoutGroup<T>(string groupId) where T : BaseLayoutGroup
        {
            if (m_LayoutGroups.TryGetValue(groupId, out BaseLayoutGroup group))
            {
                return group as T;
            }
            return null;
        }
        
        /// <summary>
        /// 移除布局组
        /// </summary>
        public void RemoveLayoutGroup(string groupId)
        {
            if (m_LayoutGroups.TryGetValue(groupId, out BaseLayoutGroup group))
            {
                group.Clear();
                Destroy(group.gameObject);
                m_LayoutGroups.Remove(groupId);
            }
        }
        
        /// <summary>
        /// 刷新所有布局组
        /// </summary>
        public void RefreshAllGroups()
        {
            foreach (var group in m_LayoutGroups.Values)
            {
                group.RefreshLayout();
            }
        }
        
        /// <summary>
        /// 清空所有布局组
        /// </summary>
        public void ClearAllGroups()
        {
            foreach (var group in m_LayoutGroups.Values)
            {
                group.Clear();
                Destroy(group.gameObject);
            }
            m_LayoutGroups.Clear();
        }
    }
    
    
}