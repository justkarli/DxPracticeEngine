using System;
using System.Linq;
using System.Runtime.InteropServices;
using CoreEngine.DxManager;
using CoreEngine.SceneManager;
using CoreEngine.VertexFormats;
using DxApplication.Wavefront;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D10;
using System.IO;

namespace CoreEngine.Wavefront
{
    public class MeshRender
    {
        public MeshRender(String object_file, DeviceManager dev_manager, Camera.Camera cam)
        {
            _cam = cam;
            _dev_manager = dev_manager;
            _device = _dev_manager.Device;
            _object_file_path = object_file;
            _mesh_parser = new Lexer();

            Scaling = 1f;
            Position = Vector3.Zero;

            OnRestore();
        }

        public void Dispose()
        {
            //dispose
            _mesh_parser.Destroy();
            _mesh_object.Dispose();
        }

        public bool OnRestore()
        {
            Dispose();

            //parse
            _mesh_parser.Create(_object_file_path, _device);
            _mesh_object = _mesh_parser.GetMeshObj();

            // initialize the effect
            _effect = Effect.FromFile(_device, @"Shader/MeshFromOBJ10.fx", "fx_4_0", ShaderFlags.EnableStrictness | ShaderFlags.Debug, EffectFlags.None);
            EffectTechnique technique = _effect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);
            _input_layout = new InputLayout(_device, pass.Description.Signature, PositionNormalTextureVertex.InputElements);
            

            return true;
        }

        public bool Render(int time)
        {
            Matrix object_world = Matrix.Identity;
            Matrix.Scaling(4 / 3, 1, 8, out object_world);
            object_world *= Matrix.Translation(_position);

            Matrix world_view_projection = object_world * _cam.ViewMatrix * _cam.ProjectionMatrix;

            //set global parameters
            _effect.GetVariableByName("g_vCameraPosition").AsVector().Set(_cam.Eye);
            //_effect.GetVariableByName("g_mWorld").AsMatrix().SetMatrix(Matrix.Identity * Matrix.Scaling(2, 2, 2));
            _effect.GetVariableByName("g_mWorld").AsMatrix().SetMatrix(object_world);

            _effect.GetVariableByName("g_mWorldViewProjection").AsMatrix().SetMatrix(world_view_projection);

            _device.InputAssembler.SetInputLayout(_input_layout);

            // iterate through all materials and render subsets
            MaterialStructure material;
            
            for (int index_subset = 0; index_subset < _mesh_object.Mesh.GetAttributeTable().Count; index_subset++)
            {
                int id = _mesh_object.Mesh.GetAttributeTable().ElementAt(index_subset).Id;
                material = _mesh_object.Materials.ElementAt(_mesh_object.Mesh.GetAttributeTable().ElementAt(index_subset).Id);

                _effect.GetVariableByName("g_vMaterialAmbient").AsVector().Set(material.Ambient);
                _effect.GetVariableByName("g_vMaterialDiffuse").AsVector().Set(material.Diffuse);
                _effect.GetVariableByName("g_vMaterialSpecular").AsVector().Set(material.Specular);
                _effect.GetVariableByName("g_fMaterialAlpha").AsScalar().Set(material.Alpha);
                _effect.GetVariableByName("g_nMaterialShininess").AsScalar().Set(material.Shininess);
                _effect.GetVariableByName("g_MeshTexture").AsResource().SetResource(material.ShaderResourceView);

                // set technique
                EffectTechnique technique = _effect.GetTechniqueByName(GetTechnique(material));
                   
                // foreach material do render the mesh.
                int passes = technique.Description.PassCount;
                for (int i = 0; i < passes; i++)
                {
                    technique.GetPassByIndex(i).Apply();
                    _mesh_object.Mesh.DrawSubset(index_subset);
                }
            }

            return true;
        }

        private String GetTechnique(MaterialStructure material)
        {
            if(material.ShaderResourceView != null)
            {
                if(material.bSpecular)
                {
                    return "TexturedSpecular";
                } 
                
                return "TexturedNoSpecular";
            } 
            
            if(material.bSpecular)
            {
                return "Specular";
            } 
           
            return "NoSpecular";
        }

        public float Scaling { get; set; }
        public Vector3 Position { get { return _position; } set { _position = value; } }

        private String _object_file_path = string.Empty;
        private MeshObj _mesh_object;
        private Lexer _mesh_parser;
        private Vector3 _position;

        private DeviceManager _dev_manager;
        private Camera.Camera _cam;
        private Device _device;
        private Effect _effect;
        private InputLayout _input_layout;
        
    }
}
