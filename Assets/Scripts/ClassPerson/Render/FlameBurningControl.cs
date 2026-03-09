using UnityEngine;

namespace ClassPerson.Render
{
    public class FlameBurningControl: MonoBehaviour
    {
        public Material material;
        private int _offset = Shader.PropertyToID("_Offset");
        private float _currentOffset;
        
        private void Update()
        {
            if (material is null) return;
            _currentOffset -= Time.deltaTime * 1.5f;
            
            if (_currentOffset > 1.0f) _currentOffset += 1.0f;
            material.SetFloat(_offset, _currentOffset);
        }
    }
}