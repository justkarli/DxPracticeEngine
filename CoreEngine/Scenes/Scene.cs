using System;
using CoreEngine.DxManager;
using CoreEngine.Wavefront;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D10.Buffer;

namespace CoreEngine.Scene
{
    class Scene : IDisposable
    {
        DataStream SampleStream;
        InputLayout SampleLayout;
        Buffer SampleVertices;
        Effect SampleEffect;
        private DeviceManager _dev_manager;
        private MeshRender _mesh_render;
        private MeshRender _blue_render;
        private MeshRender _red_render;

        private RasterizerState _rasterizer_state;

        public Scene(DeviceManager device_manager, Camera.Camera camera)
        {
            _dev_manager = device_manager;
            //String objfile = @"C:\Users\karli\Documents\Visual Studio 2010\Projects\DirectXSamples\MeshFromOBJ10\media\cup.obj";
            String objfile = @"C:\Users\karli\Desktop\mmeshes\testcube.obj";
            String obj_blue = @"C:\Users\karli\Desktop\mmeshes\blue.obj";
            String obj_red = @"C:\Users\karli\Desktop\mmeshes\red.obj";
            //String objfile = @"C:\logicxklu\Projekte\F&E\150-Kis\8-Unterlagen\MeshObj\media\plane_green.obj";
            _mesh_render = new MeshRender(objfile, device_manager, camera);
            _blue_render = new MeshRender(obj_blue, device_manager, camera);
            _red_render = new MeshRender(obj_red, device_manager, camera);

            _blue_render.Scaling = 0.5f;
            _blue_render.Position = new Vector3(1.5f, 0.5f, 0.5f);

            _red_render.Scaling = 0.10f;
            _red_render.Position = new Vector3(-2f, -0.5f, 0f);

            InitScene();
            
        }

        public void Dispose()
        {
            DestroyD3D();
        }

        public void Render(int arg)
        {
            _dev_manager.Device.OutputMerger.SetTargets(_dev_manager.DepthView, _dev_manager.RenderTarget);
            _dev_manager.Device.Rasterizer.SetViewports(_dev_manager.ViewPort);

            _dev_manager.Device.ClearDepthStencilView(_dev_manager.DepthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            _dev_manager.Device.ClearRenderTargetView(_dev_manager.RenderTarget, new Color4(1.0f, 1, 1, 1));

            _mesh_render.Render(arg);
            _blue_render.Render(arg);
            _red_render.Render(arg);

            _dev_manager.Swapchain.Present(0, PresentFlags.None);
        }

        void InitScene()
        {
            SampleEffect = Effect.FromFile(_dev_manager.Device, "MiniTri.fx", "fx_4_0");
            EffectTechnique technique = SampleEffect.GetTechniqueByIndex(0); ;
            EffectPass pass = technique.GetPassByIndex(0);
            SampleLayout = new InputLayout(_dev_manager.Device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) 
            });

            SampleStream = new DataStream(3 * 32, true, true);
            SampleStream.WriteRange(new[] {
                new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
            });
            SampleStream.Position = 0;

            SampleVertices = new Buffer(_dev_manager.Device, SampleStream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 3 * 32,
                Usage = ResourceUsage.Default
            });
        }

        void DestroyD3D()
        {
            if (SampleVertices != null)
            {
                SampleVertices.Dispose();
                SampleVertices = null;
            }

            if (SampleLayout != null)
            {
                SampleLayout.Dispose();
                SampleLayout = null;
            }

            if (SampleEffect != null)
            {
                SampleEffect.Dispose();
                SampleEffect = null;
            }

            if (SampleStream != null)
            {
                SampleStream.Dispose();
                SampleStream = null;
            }

            if (SampleLayout != null)
            {
                SampleLayout.Dispose();
                SampleLayout = null;
            }
        }
    }
}
