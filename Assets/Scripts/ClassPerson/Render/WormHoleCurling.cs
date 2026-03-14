using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClassPerson.Render
{
    public class WormHoleCurling: MonoBehaviour
    {
        public GameObject wormhole;
        private Mesh _mesh;
        private List<Color> _colors;

        // 虫洞网格是一个上下底面被删除了的标准圆柱体，其柱面上包括上下底边，共有24条环切边
        private void Start()
        {
            _mesh = wormhole.GetComponent<MeshFilter>().mesh;
            _colors = _mesh.colors.ToList();
        }
    }
}