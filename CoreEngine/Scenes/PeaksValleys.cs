using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using CoreEngine.DxManager;
using CoreEngine.VertexFormats;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D10.Buffer;
using Font = SlimDX.Direct3D10.Font;

namespace CoreEngine.Scene
{
    public class PeaksValleys : IDisposable
    {
        private DeviceManager _dev_manager;

        public PeaksValleys(DeviceManager device_manager, Camera.Camera camera)
        {
            _dev_manager = device_manager;
            //String objfile = @"C:\Users\karli\Documents\Visual Studio 2010\Projects\DirectXSamples\MeshFromOBJ10\media\cup.obj";
            _camera = camera;
            InitScene();
        }

        public void Dispose()
        {
            if (_vbuffer != null)
            {
                _vbuffer.Dispose();
                _vbuffer = null;
            }

            if (_sample_layout != null)
            {
                _sample_layout.Dispose();
                _sample_layout = null;
            }

            if (_sample_effect != null)
            {
                _sample_effect.Dispose();
                _sample_effect = null;
            }

            if (_sample_layout != null)
            {
                _sample_layout.Dispose();
                _sample_layout = null;
            }
        }
        /// <summary>
        /// TODO include camera 
        /// TODO scenemanagement, effectmanagement
        /// </summary>
        /// <param name="arg"></param>
        public void Render(long arg)
        {
            
            _dev_manager.Device.OutputMerger.SetTargets(_dev_manager.DepthView, _dev_manager.RenderTarget);
            _dev_manager.Device.Rasterizer.SetViewports(_dev_manager.ViewPort);

            _dev_manager.Device.ClearDepthStencilView(_dev_manager.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            _dev_manager.Device.ClearRenderTargetView(_dev_manager.RenderTarget, new Color4(1.0f, 1, 1, 1));

            _dev_manager.Device.InputAssembler.SetInputLayout(_sample_layout);
            
            if (_ibuffer != null)
            {
                _dev_manager.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleStrip);
                _dev_manager.Device.InputAssembler.SetInputLayout(_sample_layout);
                _dev_manager.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vbuffer, ColoredVertex.SizeOf, 0));
                _dev_manager.Device.InputAssembler.SetIndexBuffer(_ibuffer, Format.R32_UInt, 0);

                _sample_effect.GetVariableByName("ViewProjection").AsMatrix().SetMatrix(_camera.ViewMatrix * _camera.ProjectionMatrix);

                EffectTechnique technique = _sample_effect.GetTechniqueByIndex(0);
                EffectPass pass = technique.GetPassByIndex(0);
                pass.Apply();
                _dev_manager.Device.DrawIndexed(_indices.Length, 0, 0);


            } else
            {
                _dev_manager.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
                _dev_manager.Device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vbuffer, 32, 0));

                Matrix world_scale = Matrix.Scaling(1f, 1f, 1f);
                _sample_effect.GetVariableByName("ViewProjection").AsMatrix().SetMatrix(world_scale * _camera.ViewMatrix * _camera.ProjectionMatrix);
                EffectTechnique technique = _sample_effect.GetTechniqueByIndex(0);
                EffectPass pass = technique.GetPassByIndex(0);

                for (int i = 0; i < technique.Description.PassCount; ++i)
                {
                    pass.Apply();
                    _dev_manager.Device.Draw(3, 0);
                }
                DrawViewMatrix();

            }
            _dev_manager.Swapchain.Present(1, PresentFlags.None);
        }

        private void DrawViewMatrix()
        {
            if (_font == null)
                _font = new Font(_dev_manager.Device, 14, "Arial");

            _font.Draw(null, _camera.ViewMatrix.AsString(), new Rectangle(0, 0, 200, 100), FontDrawFlags.NoClip | FontDrawFlags.ExpandTabs | FontDrawFlags.WordBreak, new Color4(1, 0, 0, 0));

        }

        void InitScene()
        {
            _sample_effect = Effect.FromFile(_dev_manager.Device, "MiniTri.fx", "fx_4_0", ShaderFlags.EnableStrictness | ShaderFlags.Debug, EffectFlags.None);
            EffectTechnique technique = _sample_effect.GetTechniqueByIndex(0); ;
            EffectPass pass = technique.GetPassByIndex(0);
            _sample_layout = new InputLayout(_dev_manager.Device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) 
            });

            ColoredVertex[] xz_plane =  CalculateXZPlane();

            DataStream vertex_stream = new DataStream(xz_plane.Length * ColoredVertex.SizeOf, true, true);
            vertex_stream.WriteRange(xz_plane);
            vertex_stream.Position = 0;

            _vbuffer = new Buffer(_dev_manager.Device, vertex_stream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = xz_plane.Length * ColoredVertex.SizeOf,
                Usage = ResourceUsage.Default
            });

            _vbuffer = Triangle();
            
            _indices = CalculateIndices();
            DataStream index_stream = new DataStream(_indices.Length * Marshal.SizeOf(typeof(int)), true, true);
            index_stream.WriteRange(_indices);
            index_stream.Position = 0;

            _ibuffer = new Buffer(_dev_manager.Device, index_stream, new BufferDescription
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = _indices.Length * Marshal.SizeOf(typeof(int)),
                Usage = ResourceUsage.Default
            });
            //_ibuffer = null;
        }

        private ColoredVertex[] CalculateXYPlane()
        {
            List<ColoredVertex> vertices = new List<ColoredVertex>();
            
            return vertices.ToArray();
        }

        private Buffer Triangle()
        {
            DataStream triangle_stream = new DataStream(3 * 32, true, true);
            triangle_stream.WriteRange(new[] {
                new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
            });
            triangle_stream.Position = 0;

            Buffer b = new Buffer(_dev_manager.Device, triangle_stream, new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 3 * 32,
                Usage = ResourceUsage.Default
            });

            return b;
        }

        private ColoredVertex[] CalculateXZPlane()
        {
            //List<ColoredVertex> vertices = new List<ColoredVertex>();
            ColoredVertex[] vertices = new ColoredVertex[_plane_width * _plane_height];
            float half_width = (_plane_width - 1) * _distance_quad * 0.5f;
            float half_depth = (_plane_height - 1) * _distance_quad * 0.5f;

            ColoredVertex vertex;
            for(int i = 0; i < _plane_height; i++)
            {
                float z = half_depth - i * _distance_quad;
                for(int j = 0; j < _plane_width; j++)
                {
                    float x = -half_width + j * _distance_quad;
                    float y = i+j;
                    
                    vertex = new ColoredVertex();
                    vertex.Position = new Vector3(x, y, z);
                    vertex.Color = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
                    vertices[i * _plane_width + j] = vertex;
                }
            }

            return vertices.ToArray();
        }

        private int[] CalculateIndices()
        {
            List<int> indices = new List<int>();
            // iterate over each quad and compute indices
            int n = _plane_width;
            for (int i = 0; i < _plane_width - 1; i++)
            {
                for(int j = 0; j < _plane_height - 1; j++)
                {
                    indices.Add(i * n + j);
                    indices.Add(i * n + j + 1);
                    indices.Add((i + 1) * n + j);

                    indices.Add((i + 1) * n + j);
                    indices.Add(i * n + j + 1);
                    indices.Add((i + 1) * n + j + 1);
                }
            }

            return indices.ToArray();
        }

        private int _distance_quad = 2;
        private int _plane_width = 10;
        private int _plane_height = 10;
        private int[] _indices;

        private InputLayout _sample_layout;
        private Buffer _vbuffer;
        private Buffer _ibuffer;
        private Effect _sample_effect;

        private Camera.Camera _camera;

        private Font _font;
    }
}
