using UnityEngine;
using UnityEngine.UI;

namespace Fictology.UI
{
    public class LayoutUtil
    {
        public static void SetAllChildHorizontal(GameObject parent, Vector3 startPos)
        {
            var parentRect = parent.GetComponent<RectTransform>();
            if (parentRect == null) return;

            foreach (Transform child in parent.transform)
            {
                var childRect = child.GetComponent<RectTransform>();
                if (childRect == null) continue;
                childRect.position = startPos + new Vector3(childRect.rect.width, 0);
            }
        }
        public static void SetAllChildCentered(GameObject parent)
        {
            var parentRect = parent.GetComponent<RectTransform>();
            if (parentRect == null) return;

            foreach (Transform child in parent.transform)
            {
                var childRect = child.GetComponent<RectTransform>();
                if (childRect == null) continue;
                childRect.anchorMin = new Vector2(0.5f, 0.5f);
                childRect.anchorMax = new Vector2(0.5f, 0.5f);
                childRect.pivot = new Vector2(0.5f, 0.5f);
                childRect.anchoredPosition = Vector2.zero;
            }
        }
        
        /// <summary>
        /// 应用布局配置
        /// </summary>
        public static void ApplyLayoutConfig(RectTransform rectTransform, LayoutConfig config)
        {
            LayoutManager.Instance.RegisterLayout(rectTransform, config);
        }
        
        /// <summary>
        /// 应用层级配置
        /// </summary>
        public static void ApplyLayerConfig(RectTransform rectTransform, LayerConfig config)
        {
            LayoutManager.Instance.RegisterLayerConfig(rectTransform, config);
        }
        
        /// <summary>
        /// 创建默认布局配置
        /// </summary>
        public static LayoutConfig CreateDefaultLayoutConfig(LayoutMode mode = LayoutMode.None, 
            AdaptiveMode adaptive = AdaptiveMode.None)
        {
            return new LayoutConfig
            {
                LayoutMode = mode,
                AdaptiveMode = adaptive,
                Position = Vector2.zero,
                Size = new Vector2(200, 100),
                AnchorMin = new Vector2(0.5f, 0.5f),
                AnchorMax = new Vector2(0.5f, 0.5f),
                Pivot = new Vector2(0.5f, 0.5f),
                Margin = new RectOffset(0, 0, 0, 0),
                Padding = new RectOffset(0, 0, 0, 0)
            };
        }
        
        /// <summary>
        /// 创建全屏布局配置
        /// </summary>
        public static LayoutConfig CreateFullscreenLayout()
        {
            return new LayoutConfig
            {
                LayoutMode = LayoutMode.Stretch,
                AdaptiveMode = AdaptiveMode.Both,
                AnchorMin = Vector2.zero,
                AnchorMax = Vector2.one,
                Pivot = new Vector2(0.5f, 0.5f)
            };
        }
        
        /// <summary>
        /// 创建居中布局配置
        /// </summary>
        public static LayoutConfig CreateCenterLayout(Vector2 size)
        {
            return new LayoutConfig
            {
                LayoutMode = LayoutMode.Center,
                AdaptiveMode = AdaptiveMode.None,
                Size = size,
                AnchorMin = new Vector2(0.5f, 0.5f),
                AnchorMax = new Vector2(0.5f, 0.5f),
                Pivot = new Vector2(0.5f, 0.5f)
            };
        }
        
        /// <summary>
        /// 创建顶部工具栏布局
        /// </summary>
        public static LayoutConfig CreateTopBarLayout(float height = 60f)
        {
            return new LayoutConfig
            {
                LayoutMode = LayoutMode.TopStretch,
                AdaptiveMode = AdaptiveMode.Width,
                Size = new Vector2(0, height),
                AnchorMin = new Vector2(0, 1),
                AnchorMax = new Vector2(1, 1),
                Pivot = new Vector2(0.5f, 1f),
                Margin = new RectOffset(0, 0, 0, 0)
            };
        }
        
        /// <summary>
        /// 创建底部导航布局
        /// </summary>
        public static LayoutConfig CreateBottomNavLayout(float height = 80f)
        {
            return new LayoutConfig
            {
                LayoutMode = LayoutMode.BottomStretch,
                AdaptiveMode = AdaptiveMode.Width,
                Size = new Vector2(0, height),
                AnchorMin = new Vector2(0, 0),
                AnchorMax = new Vector2(1, 0),
                Pivot = new Vector2(0.5f, 0f),
                Margin = new RectOffset(0, 0, 0, 0)
            };
        }
        
        /// <summary>
        /// 创建侧边栏布局
        /// </summary>
        public static LayoutConfig CreateSidebarLayout(float width = 200f, bool leftSide = true)
        {
            return new LayoutConfig
            {
                LayoutMode = leftSide ? LayoutMode.LeftStretch : LayoutMode.RightStretch,
                AdaptiveMode = AdaptiveMode.Height,
                Size = new Vector2(width, 0),
                AnchorMin = leftSide ? new Vector2(0, 0) : new Vector2(1, 0),
                AnchorMax = leftSide ? new Vector2(0, 1) : new Vector2(1, 1),
                Pivot = leftSide ? new Vector2(0, 0.5f) : new Vector2(1, 0.5f),
                Margin = new RectOffset(0, 0, 0, 0)
            };
        }
        
        /// <summary>
        /// 创建安全区域布局配置
        /// </summary>
        public static LayoutConfig CreateSafeAreaLayout()
        {
            return new LayoutConfig
            {
                LayoutMode = LayoutMode.Stretch,
                AdaptiveMode = AdaptiveMode.SafeArea,
                AnchorMin = Vector2.zero,
                AnchorMax = Vector2.one,
                Pivot = new Vector2(0.5f, 0.5f)
            };
        }
        
        /// <summary>
        /// 创建响应式布局配置
        /// </summary>
        public static LayoutConfig CreateResponsiveLayout(params LayoutConfig[] configs)
        {
            return new LayoutConfig
            {
                LayoutMode = LayoutMode.Stretch,
                AdaptiveMode = AdaptiveMode.Responsive,
                ResponsiveConfigs = configs
            };
        }
        
        /// <summary>
        /// 刷新布局
        /// </summary>
        public static void RefreshLayout(RectTransform rectTransform)
        {
            LayoutManager.Instance.RefreshLayout(rectTransform);
        }
        
        /// <summary>
        /// 刷新所有布局
        /// </summary>
        public static void RefreshAllLayouts()
        {
            LayoutManager.Instance.RefreshAllLayouts();
        }
        
        /// <summary>
        /// 获取屏幕尺寸
        /// </summary>
        public static ScreenSize GetScreenSize()
        {
            return LayoutManager.Instance.GetCurrentScreenSize();
        }
        
        /// <summary>
        /// 获取安全区域
        /// </summary>
        public static Rect GetSafeArea()
        {
            return LayoutManager.Instance.GetSafeArea();
        }
        
        // /// <summary>
        // /// 创建水平布局组
        // /// </summary>
        // public static HorizontalLayoutGroup CreateHorizontalGroup(string groupId, RectTransform container, 
        //     Vector2 spacing, RectOffset padding)
        // {
        //     return LayoutGroup.Instance.CreateLayoutGroup<HorizontalLayoutGroup>(groupId, container);
        // }
        //
        // /// <summary>
        // /// 创建垂直布局组
        // /// </summary>
        // public static VerticalLayoutGroup CreateVerticalGroup(string groupId, RectTransform container, 
        //     Vector2 spacing, RectOffset padding)
        // {
        //     return LayoutGroup.Instance.CreateLayoutGroup<VerticalLayoutGroup>(groupId, container);
        // }
        //
        // /// <summary>
        // /// 创建网格布局组
        // /// </summary>
        // public static GridLayoutGroup CreateGridGroup(string groupId, RectTransform container, 
        //     Vector2 cellSize, Vector2 spacing, RectOffset padding)
        // {
        //     return LayoutGroup.Instance.CreateLayoutGroup<GridLayoutGroup>(groupId, container);
        // }
        
        /// <summary>
        /// 获取布局组
        /// </summary>
        public static T GetLayoutGroup<T>(string groupId) where T : BaseLayoutGroup
        {
            return LayoutGroup.Instance.GetLayoutGroup<T>(groupId);
        }
    }
}