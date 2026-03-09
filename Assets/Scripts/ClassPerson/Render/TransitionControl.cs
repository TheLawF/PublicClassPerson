using System;
using UnityEngine;

namespace ClassPerson.Render
{
    public class TransitionControl: MonoBehaviour
    {
        public Material transitionMaterial;
        public float transitionDuration = 2.0f;
    
        private Camera mainCamera;
        private RenderTexture renderTexture;
        private float currentProgress = 0f;
        private bool isTransitioning = false;

        private static readonly int Progress = Shader.PropertyToID("_Progress");

        void Start()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            StartTransition();
        }

        private void OnDisable()
        {
            ResetTransition();
        }

        void Update()
        {
            if (isTransitioning)
            {
                currentProgress -= Time.deltaTime / transitionDuration;
                currentProgress = Mathf.Clamp01(currentProgress);
                transitionMaterial.SetFloat(Progress, currentProgress);
            
                if (currentProgress >= 1f)
                {
                    isTransitioning = false;
                }
            }
        }
    
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // 渲染场景到临时纹理
            Graphics.Blit(source, renderTexture);
        
            // 应用特效
            Graphics.Blit(source, destination, transitionMaterial);
        }
    
        public void StartTransition()
        {
            isTransitioning = true;
            currentProgress = 1f;
        }
    
        public void ResetTransition()
        {
            currentProgress = 1f;
            transitionMaterial.SetFloat(Progress, 1f);
            isTransitioning = false;
        }
    
        void OnDestroy()
        {
            if (renderTexture != null)
                renderTexture.Release();
        }
    }
}