using System;
using UnityEngine;

namespace ClassPerson.Render
{
    public class BeamShaderControl: MonoBehaviour
    {
        public Material beamMaterial;
        private Camera _camera;
        private RenderTexture _texture;
        
        private int _offset = Shader.PropertyToID("_Offset");
        private int _rotation = Shader.PropertyToID("_Rotation");
        private int _cutoff = Shader.PropertyToID("_Cutoff");
        
        private float _currentOffset;
        private float _currentRotation;


        private void Start()
        {
            _camera = Camera.current;
        }

        private void OnDestroy()
        {
            _offset = 0;
            _rotation = 0;
        }

        private void Update()
        {
            if (beamMaterial is null) return;
            
            // 更新动画值
            _currentOffset -= Time.deltaTime * 1.5f;
            _currentRotation += Time.deltaTime * 1.5f;
            
            // 循环控制
            if (_currentOffset > 1.0f) _currentOffset += 1.0f;
            if (_currentRotation > 360.0f) _currentRotation -= 360.0f;
            
            // 传递给材质
            beamMaterial.SetFloat(_offset, _currentOffset);
            beamMaterial.SetFloat(_rotation, _currentRotation);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            Graphics.Blit(source, destination, beamMaterial);
        }
    }
}