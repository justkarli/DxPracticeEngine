using System;
using CoreEngine.DxManager;
using CoreEngine.VertexFormats;
using SlimDX;
using SlimDX.Direct3D10;
using Buffer = SlimDX.Direct3D10.Buffer;

namespace CoreEngine.Scenes
{
    public class SimpleScene : IDisposable
    {
        public SimpleScene(DeviceManager devicemanager, Camera.CameraV2 cam)
        {
            _dev_manager = devicemanager;
            _camera = cam;

            InitScene();
        }

        private void InitScene()
        {
            _effect = Effect.FromFile(_dev_manager.Device, @"Shader\MiniTri.fx", "fx_4_0");
            EffectTechnique technique = _effect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

            _input_layout = new InputLayout(_dev_manager.Device, pass.Description.Signature, ColoredVertex.InputElements);
            //Triangle triangle = new Triangle(new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0)  );
            Vector3[] quad = 
            { 
                new Vector3(-0.5f, 0.5f, 0), // top left
                new Vector3(0.5f, 0.5f, 0),  // top right
                new Vector3(-0.5f, -0.5f, 0), // bottom left
                new Vector3(0.5f, -0.5f, 0),  // bottom right
            };

            ColoredVertex[] colored_quad = {
                                               new ColoredVertex(quad[0], new Color4(1, 0, 0, 1)), 
                                               new ColoredVertex(quad[1], new Color4(1, 0, 0, 1)), 
                                               new ColoredVertex(quad[2], new Color4(1, 0, 0, 1)), 
                                               new ColoredVertex(quad[3], new Color4(1, 0, 0, 1)), 
                                           };
            // size in bytes (quad: (3x4)x4 + color:(4x4)x4
            DataStream quad_stream = new DataStream(ColoredVertex.SizeOf * quad.Length, true, true);
            quad_stream.WriteRange(colored_quad);
            quad_stream.Position = 0;

            _vbuffer = new Buffer(_dev_manager.Device, quad_stream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SizeInBytes = ColoredVertex.SizeOf * colored_quad.Length
            });
        }

        public void Render(long arg)
        {
            _dev_manager.Device.InputAssembler.SetInputLayout(_input_layout);
            _dev_manager.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
            _dev_manager.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding
                {
                    Buffer = _vbuffer,
                    Offset = 0,
                    Stride = ColoredVertex.SizeOf
                }
            );

            Matrix world_scale = Matrix.Scaling(1, 1, 1);
            Matrix mvp_matrix = world_scale * _camera.ViewMatrix * _camera.ProjMatrix;
            
            _effect.GetVariableByName("ViewProjection").AsMatrix().SetMatrix(mvp_matrix);

            EffectTechnique technique = _effect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

            for(int i = 0; i < technique.Description.PassCount; ++i)
            {
                pass.Apply();
                _dev_manager.Device.Draw(4, 0);
            }

        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        private DeviceManager _dev_manager;
        private Camera.CameraV2 _camera;
        
        private Effect _effect;
        private Buffer _vbuffer;
        private InputLayout _input_layout;

    }
}
