using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fictology.UI
{
    public class LayoutManager: MonoBehaviour
    {
        private static LayoutManager m_Instance;
        public static LayoutManager Instance = m_Instance;
        
        private Vector2Int m_LastScreenSize;
        private ScreenSize m_CurrentScreenSize = ScreenSize.Desktop;
        private Rect m_SafeArea;
        
        // 布局配置缓存
        private readonly Dictionary<RectTransform, LayoutConfig> m_LayoutConfigs = new();
        private readonly Dictionary<RectTransform, LayerConfig> m_LayerConfigs = new();

        [Header("响应式断点")]
        private ResponsiveBreakpoint[] m_Breakpoints = {
            new() { ScreenSize = ScreenSize.MobilePortrait, MaxWidth = 768, AspectRatioMax = 0.75f },
            new() { ScreenSize = ScreenSize.MobileLandscape, MaxWidth = 1024, MinWidth = 768, AspectRatioMin = 1.33f },
            new() { ScreenSize = ScreenSize.TabletPortrait, MaxWidth = 1024, MinWidth = 768, AspectRatioMax = 0.8f },
            new() { ScreenSize = ScreenSize.TabletLandscape, MaxWidth = 1366, MinWidth = 1024, AspectRatioMin = 1.33f },
            new() { ScreenSize = ScreenSize.Desktop, MinWidth = 1366 },
            new() { ScreenSize = ScreenSize.Wide, MinWidth = 1920 }
        };
        
        private void Awake()
        {
            if (m_Instance == null)
            {
                m_Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (m_Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            m_LastScreenSize = new Vector2Int(Screen.width, Screen.height);
            m_SafeArea = Screen.safeArea;
            m_CurrentScreenSize = DetectScreenSize();
        }
        
        /// <summary>
        /// 检测屏幕尺寸
        /// </summary>
        private ScreenSize DetectScreenSize()
        {
            int width = Screen.width;
            int height = Screen.height;
            float aspectRatio = (float)width / height;
            
            foreach (var breakpoint in m_Breakpoints)
            {
                bool widthMatch = (breakpoint.MinWidth == 0 || width >= breakpoint.MinWidth) &&
                                 (breakpoint.MaxWidth == 0 || width <= breakpoint.MaxWidth);
                bool heightMatch = (breakpoint.MinHeight == 0 || height >= breakpoint.MinHeight) &&
                                  (breakpoint.MaxHeight == 0 || height <= breakpoint.MaxHeight);
                bool aspectMatch = (breakpoint.AspectRatioMin == 0 || aspectRatio >= breakpoint.AspectRatioMin) &&
                                  (breakpoint.AspectRatioMax == 0 || aspectRatio <= breakpoint.AspectRatioMax);
                
                if (widthMatch && heightMatch && aspectMatch)
                {
                    return breakpoint.ScreenSize;
                }
            }
            
            return ScreenSize.Desktop;
        }
        
        /// <summary>
        /// 屏幕尺寸变化
        /// </summary>
        private void OnScreenSizeChanged()
        {
            ScreenSize newSize = DetectScreenSize();
            if (m_CurrentScreenSize != newSize)
            {
                m_CurrentScreenSize = newSize;
                ApplyResponsiveLayouts();
            }
        }
        
        /// <summary>
        /// 安全区域变化
        /// </summary>
        private void OnSafeAreaChanged()
        {
            ApplySafeAreaLayouts();
        }
        
        /// <summary>
        /// 应用响应式布局
        /// </summary>
        private void ApplyResponsiveLayouts()
        {
            foreach (var kvp in m_LayoutConfigs)
            {
                RectTransform rectTransform = kvp.Key;
                LayoutConfig config = kvp.Value;
                
                if (config.AdaptiveMode == AdaptiveMode.Responsive)
                {
                    ApplyLayout(rectTransform, config);
                }
            }
        }
        
        /// <summary>
        /// 应用安全区域布局
        /// </summary>
        private void ApplySafeAreaLayouts()
        {
            foreach (var kvp in m_LayoutConfigs)
            {
                RectTransform rectTransform = kvp.Key;
                LayoutConfig config = kvp.Value;
                
                if (config.AdaptiveMode == AdaptiveMode.SafeArea)
                {
                    ApplySafeArea(rectTransform);
                }
            }
        }
        
        /// <summary>
        /// 注册UI元素布局
        /// </summary>
        public void RegisterLayout(RectTransform rectTransform, LayoutConfig config)
        {
            if (rectTransform == null || config == null) return;
            
            m_LayoutConfigs[rectTransform] = config;
            ApplyLayout(rectTransform, config);
        }
        
        /// <summary>
        /// 注册UI元素层级配置
        /// </summary>
        public void RegisterLayerConfig(RectTransform rectTransform, LayerConfig config)
        {
            if (rectTransform == null || config == null) return;
            
            m_LayerConfigs[rectTransform] = config;
            ApplyLayerConfig(rectTransform, config);
        }
        
        /// <summary>
        /// 移除UI元素布局
        /// </summary>
        public void UnregisterLayout(RectTransform rectTransform)
        {
            m_LayoutConfigs.Remove(rectTransform);
        }
        
        /// <summary>
        /// 移除UI元素层级配置
        /// </summary>
        public void UnregisterLayerConfig(RectTransform rectTransform)
        {
            m_LayerConfigs.Remove(rectTransform);
        }
        
        /// <summary>
        /// 应用布局配置
        /// </summary>
        public void ApplyLayout(RectTransform rectTransform, LayoutConfig config)
        {
            if (rectTransform == null || config == null) return;
            
            // 获取响应式配置
            LayoutConfig responsiveConfig = GetResponsiveConfig(config);
            if (responsiveConfig != null)
            {
                config = responsiveConfig;
            }
            
            // 应用基本设置
            rectTransform.anchorMin = config.AnchorMin;
            rectTransform.anchorMax = config.AnchorMax;
            rectTransform.pivot = config.Pivot;
            
            // 根据布局模式设置位置和大小
            switch (config.LayoutMode)
            {
                case LayoutMode.Stretch:
                    ApplyStretchLayout(rectTransform, config);
                    break;
                case LayoutMode.Center:
                    ApplyCenterLayout(rectTransform, config);
                    break;
                case LayoutMode.TopLeft:
                    ApplyTopLeftLayout(rectTransform, config);
                    break;
                case LayoutMode.TopCenter:
                    ApplyTopCenterLayout(rectTransform, config);
                    break;
                case LayoutMode.TopRight:
                    ApplyTopRightLayout(rectTransform, config);
                    break;
                case LayoutMode.MiddleLeft:
                    ApplyMiddleLeftLayout(rectTransform, config);
                    break;
                case LayoutMode.MiddleCenter:
                    ApplyMiddleCenterLayout(rectTransform, config);
                    break;
                case LayoutMode.MiddleRight:
                    ApplyMiddleRightLayout(rectTransform, config);
                    break;
                case LayoutMode.BottomLeft:
                    ApplyBottomLeftLayout(rectTransform, config);
                    break;
                case LayoutMode.BottomCenter:
                    ApplyBottomCenterLayout(rectTransform, config);
                    break;
                case LayoutMode.BottomRight:
                    ApplyBottomRightLayout(rectTransform, config);
                    break;
                case LayoutMode.TopStretch:
                    ApplyTopStretchLayout(rectTransform, config);
                    break;
                case LayoutMode.BottomStretch:
                    ApplyBottomStretchLayout(rectTransform, config);
                    break;
                case LayoutMode.LeftStretch:
                    ApplyLeftStretchLayout(rectTransform, config);
                    break;
                case LayoutMode.RightStretch:
                    ApplyRightStretchLayout(rectTransform, config);
                    break;
                case LayoutMode.VerticalStretch:
                    ApplyVerticalStretchLayout(rectTransform, config);
                    break;
                case LayoutMode.HorizontalStretch:
                    ApplyHorizontalStretchLayout(rectTransform, config);
                    break;
            }
            
            // 应用边距
            ApplyMargin(rectTransform, config);
            
            // 应用自适应模式
            ApplyAdaptiveMode(rectTransform, config);
            
            // 应用响应式布局
            if (config.AdaptiveMode == AdaptiveMode.Responsive)
            {
                ApplyResponsiveLayout(rectTransform, config);
            }
        }
        
        /// <summary>
        /// 获取响应式配置
        /// </summary>
        private LayoutConfig GetResponsiveConfig(LayoutConfig baseConfig)
        {
            if (baseConfig.ResponsiveConfigs == null || baseConfig.ResponsiveConfigs.Length == 0)
                return null;
            
            foreach (var config in baseConfig.ResponsiveConfigs)
            {
                if ((int)config.AdaptiveMode == (int)m_CurrentScreenSize)
                {
                    return config;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 应用层级配置
        /// </summary>
        private void ApplyLayerConfig(RectTransform rectTransform, LayerConfig config)
        {
            if (rectTransform == null || config == null) return;
            
            // 设置Canvas
            Canvas canvas = rectTransform.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = rectTransform.gameObject.AddComponent<Canvas>();
            }
            
            // 设置排序
            if (config.OverrideSortingOrder >= 0)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = config.OverrideSortingOrder;
            }
            
            // 设置射线检测
            GraphicRaycaster raycaster = rectTransform.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                raycaster = rectTransform.gameObject.AddComponent<GraphicRaycaster>();
            }
            raycaster.enabled = config.RaycastTarget;
            
            // 设置子节点深度
            Canvas childCanvas = rectTransform.GetComponentInChildren<Canvas>();
            if (childCanvas != null && config.ChildDepth > 0)
            {
                childCanvas.overrideSorting = true;
                childCanvas.sortingOrder = (config.OverrideSortingOrder >= 0 ? 
                    config.OverrideSortingOrder : canvas.sortingOrder) + config.ChildDepth;
            }
        }
        
        #region 布局应用方法
        private void ApplyStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = new Vector2(config.Margin.left, config.Margin.bottom);
            rectTransform.offsetMax = new Vector2(-config.Margin.right, -config.Margin.top);
        }
        
        private void ApplyCenterLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = config.Position;
        }
        
        private void ApplyTopLeftLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(
                config.Margin.left, 
                -config.Margin.top
            );
        }
        
        private void ApplyTopCenterLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(0, -config.Margin.top);
        }
        
        private void ApplyTopRightLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(
                -config.Margin.right, 
                -config.Margin.top
            );
        }
        
        private void ApplyMiddleLeftLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(0, 0.5f);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(config.Margin.left, 0);
        }
        
        private void ApplyMiddleCenterLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        private void ApplyMiddleRightLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(1, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(-config.Margin.right, 0);
        }
        
        private void ApplyBottomLeftLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(
                config.Margin.left, 
                config.Margin.bottom
            );
        }
        
        private void ApplyBottomCenterLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(0, config.Margin.bottom);
        }
        
        private void ApplyBottomRightLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.sizeDelta = config.Size;
            rectTransform.anchoredPosition = new Vector2(
                -config.Margin.right, 
                config.Margin.bottom
            );
        }
        
        private void ApplyTopStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.sizeDelta = new Vector2(
                -(config.Margin.left + config.Margin.right), 
                config.Size.y
            );
            rectTransform.anchoredPosition = new Vector2(0, -config.Margin.top);
        }
        
        private void ApplyBottomStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = new Vector2(
                -(config.Margin.left + config.Margin.right), 
                config.Size.y
            );
            rectTransform.anchoredPosition = new Vector2(0, config.Margin.bottom);
        }
        
        private void ApplyLeftStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                config.Size.x,
                -(config.Margin.top + config.Margin.bottom)
            );
            rectTransform.anchoredPosition = new Vector2(config.Margin.left, 0);
        }
        
        private void ApplyRightStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                config.Size.x,
                -(config.Margin.top + config.Margin.bottom)
            );
            rectTransform.anchoredPosition = new Vector2(-config.Margin.right, 0);
        }
        
        private void ApplyVerticalStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                config.Size.x,
                -(config.Margin.top + config.Margin.bottom)
            );
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        private void ApplyHorizontalStretchLayout(RectTransform rectTransform, LayoutConfig config)
        {
            rectTransform.anchorMin = new Vector2(0, 0.5f);
            rectTransform.anchorMax = new Vector2(1, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(
                -(config.Margin.left + config.Margin.right),
                config.Size.y
            );
            rectTransform.anchoredPosition = Vector2.zero;
        }
        #endregion
        
        /// <summary>
        /// 应用边距
        /// </summary>
        private void ApplyMargin(RectTransform rectTransform, LayoutConfig config)
        {
            if (rectTransform == null || config == null) return;
            
            // 根据锚点应用边距
            Vector2 offsetMin = rectTransform.offsetMin;
            Vector2 offsetMax = rectTransform.offsetMax;
            
            // 左/下边距
            offsetMin.x += config.Margin.left;
            offsetMin.y += config.Margin.bottom;
            
            // 右/上边距
            offsetMax.x -= config.Margin.right;
            offsetMax.y -= config.Margin.top;
            
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
        
        /// <summary>
        /// 应用自适应模式
        /// </summary>
        private void ApplyAdaptiveMode(RectTransform rectTransform, LayoutConfig config)
        {
            if (rectTransform == null || config == null) return;
            
            switch (config.AdaptiveMode)
            {
                case AdaptiveMode.Width:
                    ApplyWidthAdaptive(rectTransform, config);
                    break;
                case AdaptiveMode.Height:
                    ApplyHeightAdaptive(rectTransform, config);
                    break;
                case AdaptiveMode.Both:
                    ApplyBothAdaptive(rectTransform, config);
                    break;
                case AdaptiveMode.AspectRatio:
                    ApplyAspectRatioAdaptive(rectTransform, config);
                    break;
                case AdaptiveMode.SafeArea:
                    ApplySafeArea(rectTransform);
                    break;
            }
        }
        
        /// <summary>
        /// 宽度自适应
        /// </summary>
        private void ApplyWidthAdaptive(RectTransform rectTransform, LayoutConfig config)
        {
            float parentWidth = rectTransform.parent.GetComponent<RectTransform>().rect.width;
            float targetWidth = parentWidth - (config.Margin.left + config.Margin.right);
            
            if (config.MinWidth > 0) targetWidth = Mathf.Max(targetWidth, config.MinWidth);
            if (config.MaxWidth > 0) targetWidth = Mathf.Min(targetWidth, config.MaxWidth);
            
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }
        
        /// <summary>
        /// 高度自适应
        /// </summary>
        private void ApplyHeightAdaptive(RectTransform rectTransform, LayoutConfig config)
        {
            float parentHeight = rectTransform.parent.GetComponent<RectTransform>().rect.height;
            float targetHeight = parentHeight - (config.Margin.top + config.Margin.bottom);
            
            if (config.MinHeight > 0) targetHeight = Mathf.Max(targetHeight, config.MinHeight);
            if (config.MaxHeight > 0) targetHeight = Mathf.Min(targetHeight, config.MaxHeight);
            
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }
        
        /// <summary>
        /// 宽高自适应
        /// </summary>
        private void ApplyBothAdaptive(RectTransform rectTransform, LayoutConfig config)
        {
            ApplyWidthAdaptive(rectTransform, config);
            ApplyHeightAdaptive(rectTransform, config);
        }
        
        /// <summary>
        /// 宽高比自适应
        /// </summary>
        private void ApplyAspectRatioAdaptive(RectTransform rectTransform, LayoutConfig config)
        {
            ContentSizeFitter sizeFitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
            }
            
            AspectRatioFitter aspectFitter = rectTransform.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null)
            {
                aspectFitter = rectTransform.gameObject.AddComponent<AspectRatioFitter>();
            }
            
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            aspectFitter.aspectRatio = config.AspectRatio;
        }
        
        /// <summary>
        /// 安全区域适配
        /// </summary>
        private void ApplySafeArea(RectTransform rectTransform)
        {
            Rect safeArea = Screen.safeArea;
            
            // 转换为UI坐标
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector2 anchorMin = safeArea.position;
                Vector2 anchorMax = safeArea.position + safeArea.size;
                
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;
                
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
            }
        }
        
        /// <summary>
        /// 应用响应式布局
        /// </summary>
        private void ApplyResponsiveLayout(RectTransform rectTransform, LayoutConfig config)
        {
            // 根据当前屏幕尺寸应用不同的布局配置
            LayoutConfig responsiveConfig = GetResponsiveConfig(config);
            if (responsiveConfig != null)
            {
                ApplyLayout(rectTransform, responsiveConfig);
            }
        }
        
        /// <summary>
        /// 获取当前屏幕尺寸
        /// </summary>
        public ScreenSize GetCurrentScreenSize()
        {
            return m_CurrentScreenSize;
        }
        
        /// <summary>
        /// 获取安全区域
        /// </summary>
        public Rect GetSafeArea()
        {
            return m_SafeArea;
        }
        
        /// <summary>
        /// 刷新所有布局
        /// </summary>
        public void RefreshAllLayouts()
        {
            foreach (var kvp in m_LayoutConfigs)
            {
                ApplyLayout(kvp.Key, kvp.Value);
            }
        }
        
        /// <summary>
        /// 刷新指定布局
        /// </summary>
        public void RefreshLayout(RectTransform rectTransform)
        {
            if (m_LayoutConfigs.TryGetValue(rectTransform, out LayoutConfig config))
            {
                ApplyLayout(rectTransform, config);
            }
        }
    }
    
    public enum LayoutMode
    {
        None,               // 无布局
        Stretch,            // 拉伸
        Center,             // 居中
        TopLeft,            // 左上
        TopCenter,          // 上中
        TopRight,           // 右上
        MiddleLeft,         // 中左
        MiddleCenter,       // 中中
        MiddleRight,        // 中右
        BottomLeft,         // 左下
        BottomCenter,       // 下中
        BottomRight,        // 右下
        TopStretch,         // 顶部拉伸
        BottomStretch,      // 底部拉伸
        LeftStretch,        // 左侧拉伸
        RightStretch,       // 右侧拉伸
        VerticalStretch,    // 垂直拉伸
        HorizontalStretch,  // 水平拉伸
        Grid,               // 网格
        Flow,               // 流式
        Stack               // 堆叠
    }
    
    /// <summary>
    /// 自适应模式
    /// </summary>
    public enum AdaptiveMode
    {
        None,               // 无自适应
        Width,             // 宽度自适应
        Height,            // 高度自适应
        Both,              // 宽高自适应
        AspectRatio,       // 宽高比
        SafeArea,          // 安全区域
        Responsive         // 响应式
    }
    
    /// <summary>
    /// 屏幕尺寸定义
    /// </summary>
    public enum ScreenSize
    {
        MobilePortrait,     // 手机竖屏
        MobileLandscape,    // 手机横屏
        TabletPortrait,     // 平板竖屏
        TabletLandscape,    // 平板横屏
        Desktop,           // 桌面
        Wide               // 超宽
    }
    
    /// <summary>
    /// 布局配置
    /// </summary>
    [Serializable]
    public class LayoutConfig
    {
        [Header("基本设置")]
        public LayoutMode LayoutMode = LayoutMode.None;
        public AdaptiveMode AdaptiveMode = AdaptiveMode.None;
        
        [Header("位置和大小")]
        public Vector2 Position = Vector2.zero;
        public Vector2 Size = Vector2.zero;
        public Vector2 AnchorMin = Vector2.zero;
        public Vector2 AnchorMax = Vector2.one;
        public Vector2 Pivot = new Vector2(0.5f, 0.5f);
        
        [Header("边距")]
        public RectOffset Margin = new RectOffset(0, 0, 0, 0);
        public RectOffset Padding = new RectOffset(0, 0, 0, 0);
        
        [Header("自适应设置")]
        public float AspectRatio = 1f;
        public float MinWidth = 0f;
        public float MaxWidth = 0f;
        public float MinHeight = 0f;
        public float MaxHeight = 0f;
        
        [Header("响应式配置")]
        public LayoutConfig[] ResponsiveConfigs; // 不同屏幕尺寸的配置
    }
    
    public class LayerConfig
    {
        // public UILayer Layer;
        public int Order = 0;                    // 排序值
        public bool BlockInput = false;          // 是否阻挡输入
        public bool RaycastTarget = true;        // 是否接收射线
        public int OverrideSortingOrder = -1;    // 覆盖排序值
        public int ChildDepth = 0;               // 子节点深度
    }

    public class ResponsiveBreakpoint
    {
        public ScreenSize ScreenSize;
        public int MinWidth = 0;
        public int MaxWidth = 0;
        public int MinHeight = 0;
        public int MaxHeight = 0;
        public float AspectRatioMin = 0f;
        public float AspectRatioMax = 0f;
    }
}