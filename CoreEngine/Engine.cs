using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CoreEngine.Camera;
using CoreEngine.DxManager;
using CoreEngine.Scene;
using CoreEngine.SceneManager;
using CoreEngine.Scenes;
using SlimDX;
using SlimDX.Direct3D10;

namespace CoreEngine
{
    public class Engine : IDisposable
    {
        public Engine()
        {
            _engine_timer = new Stopwatch();
            _device_manager = new DeviceManager(this);
           
            // device will be initialized after getting all forms information (size, window handler) 
            // wait until the device initialization to initialize the camera
            _camera = new Camera.Camera();
            _eventhub = new EventHub();
            _camera_v2 = new FirstPersonCamera(_eventhub);
            _device_manager.OnDeviceManagerInitialized += (OnDeviceManagerInitialized);
            
        }

        #region Event handler
        private void OnDeviceManagerInitialized(object sender, EventArgs e)
        {
            float aspect_ratio = _device_manager.BackBufferWidth / (float)_device_manager.BackBufferHeight;
            const float fov = 90;

            _camera.SetPerspectiveProjection(fov, aspect_ratio, 1, 100);
            _camera_v2.SetProjectionParams(1, 100, aspect_ratio, (float) (Math.PI * 0.5f));
            _camera.SetPositionAndView(0, 0, 10, 0, 0);
            
            InitializeScene();

            _engine_timer.Start();
        }

        #endregion // event handler

        private void InitializeScene()
        {
            //_peaks = new PeaksValleys(DeviceManager, Camera);
            _simple_scene = new SimpleScene(DeviceManager, _camera_v2);
        }

        public void FrameTick()
        {
            // update mechanisms
            _camera_v2.Update();

            // render mechanism
            // decide which rendering

            lock (RenderLock)
            {
                _device_manager.Device.OutputMerger.SetTargets(_device_manager.DepthView, _device_manager.RenderTarget);
                _device_manager.Device.Rasterizer.SetViewports(_device_manager.ViewPort);

                _device_manager.Device.ClearDepthStencilView(_device_manager.DepthView, DepthStencilClearFlags.Depth, 1f, 0);
                _device_manager.Device.ClearRenderTargetView(_device_manager.RenderTarget, new Color4(1, 1, 1, 1));

                _simple_scene.Render(_engine_timer.ElapsedMilliseconds);
                _device_manager.Present();
            }
        }

        private void RenderForward() {}

        private void RenderDeferred() {}

        #region Implementation of IDisposable
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion


        public DeviceManager DeviceManager { get { return _device_manager; } }
        public Camera.Camera Camera { get { return _camera; } }
        public CameraV2 CameraV2 { get { return _camera_v2; } }
        public EventHub EventHub { get { return _eventhub; } }

        #region Attributes
        private DeviceManager _device_manager;
        private Camera.Camera _camera;
        private CameraV2 _camera_v2;
        private Stopwatch _engine_timer;

        private EventHub _eventhub;

        private Kinect.KinectManager _kinect_manager;

        // current scenes
        private PeaksValleys _peaks;
        //private Scene _scene;
        private SimpleScene _simple_scene;


        // renderlock
        public object RenderLock = new object();

        #endregion

    }
}
