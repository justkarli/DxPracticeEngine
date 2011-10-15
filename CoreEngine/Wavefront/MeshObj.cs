using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreEngine.SceneManager;
using CoreEngine.VertexFormats;
using SlimDX;
using SlimDX.Direct3D10;

namespace CoreEngine.Wavefront
{
    public struct MeshObj
    {
        /// <summary>
        /// Initializes all lists on. If fields were initialized before all their items get cleared. 
        /// </summary>
        public void Initialize()
        {
            if (!_initialized)
            {
                Vertices = new List<PositionNormalTextureVertex>();
                Indices = new List<int>();
                Attributes = new List<int>();
                Materials = new List<MaterialStructure>();
                _initialized = true;
            } else
            {
                Vertices.Clear();
                Indices.Clear();
                Attributes.Clear();
                Materials.Clear();
                Mesh = null;
            }
        }

        /// <summary>
        /// Sets default values to a given material.
        /// </summary>
        /// <param name="material">material</param>
        public void InitMaterial(out MaterialStructure material)
        {
            material = new MaterialStructure
            {
                Ambient = new Vector3(0.2f, 0.2f, 0.2f),
                Diffuse = new Vector3(0.8f, 0.8f, 0.8f),
                Specular = new Vector3(1.0f, 1.0f, 1.0f),
                Shininess = 0,
                Alpha = 1.0f,
                bSpecular = false,
                ShaderResourceView = null
            };
        }

        /// <summary>
        /// Get information if the struct has been initialized before or not. 
        /// </summary>
        public bool IsInitialized
        {
            get { return _initialized; }
        }

        public void Dispose()
        {
            if (_initialized)
            {
                Vertices.Clear();
                Vertices = null;

                Indices.Clear();
                Indices = null;

                Attributes.Clear();
                Attributes = null;

                Materials.Clear();
                Materials = null;

                Mesh.Dispose();

                _initialized = false;
            }
        }

        public Mesh Mesh;
        public List<PositionNormalTextureVertex> Vertices;
        public List<int> Indices;
        public List<int> Attributes;
        public List<MaterialStructure> Materials;
        public int NumberAttribTableEntries;
        private bool _initialized;
    }
}
