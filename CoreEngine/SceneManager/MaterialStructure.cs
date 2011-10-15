using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;

namespace CoreEngine.SceneManager
{
    public struct MaterialStructure
    {
        public String Name;
        public Vector3 Diffuse;
        public Vector3 Ambient;
        public Vector3 Specular;
        public Vector3 Emmisive;
        public float Shininess;
        public String TextureString;
        public ShaderResourceView ShaderResourceView;
        public float Alpha;
        public bool bSpecular;
    }
}
