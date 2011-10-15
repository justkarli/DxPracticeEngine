using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.Direct3D10_1;
using SlimDX.DXGI;
using Device1 = SlimDX.Direct3D10_1.Device1;
using CoreEngine.Utilities;
using Resource = SlimDX.Direct3D10.Resource;

namespace CoreEngine.DxManager
{
    public class DeviceManager : IDisposable
    {
        public class BackBufferSizeChangedEventArgs : EventArgs
        {
            public readonly float Width, Height;

            public BackBufferSizeChangedEventArgs(float width, float height)
            {
                Width = width;
                Height = height;
            }
        }

        private const int VSYNC_ENABLED = 1;

        #region Constructor and Initialization

        public DeviceManager(Engine engine)
        {
            _engine = engine;
        }

        public void SetWidthAndHeight(int width, int height)
        {
            _current_width = width;
            _current_height = height;
        }

        public void Initialize(IntPtr wnd_handle, FeatureLevel feature_level)
        {
            if (_initialized) return;

            _window_handle = wnd_handle;

            if (_current_width <= 0)
                _current_width = 1024;

            if (_current_height <= 0)
                _current_height = 768;


            CreateDeviceWithSwapChain(wnd_handle, feature_level);

            // get actual render target from the swap chain
            UpdateMainTextures();

            // bind views to output merger state
            _device.OutputMerger.SetTargets(_depth_stencil_view, _render_target);

            // viewport 
            UpdateViewport();

            _initialized = true;

            RaiseDeviceInitialized(EventArgs.Empty);
        }

        private void UpdateViewport()
        {
            ViewPort = new Viewport(0, 0, _current_width, _current_height, 0, 1);
            _device.Rasterizer.SetViewports(ViewPort);
        }

        private void UpdateMainTextures()
        {
            Texture2D swapchain_resource = Resource.FromSwapChain<Texture2D>(_swapchain, 0);
            _render_target = new RenderTargetView(_device, swapchain_resource);
            _render_target.DebugName = "MainRenderTarget";

            // create depth/stencil texture
            Texture2DDescription depth_description = new Texture2DDescription
            {
                ArraySize = 1,
                Height = _current_height,
                Width = _current_width,
                MipLevels = 1,
                Format = Format.D32_Float,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SampleDescription = new SampleDescription(1, 0)
            };

            DepthStencilViewDescription depth_stencil_desc = new DepthStencilViewDescription
            {
                Format = depth_description.Format,
                Dimension = DepthStencilViewDimension.Texture2D, 
                MipSlice = 0
            };
            
            _depth_buffer = new Texture2D(_device, depth_description);
            _depth_buffer.DebugName = "Texture2D_DepthBuffer";

            _depth_stencil_view = new DepthStencilView(_device, _depth_buffer, depth_stencil_desc);
            _depth_stencil_view.DebugName = "DepthStencil";
        }

        #region wpf init
        public void Initialize(FeatureLevel feature_level)
        {
            CreateDevice(feature_level);

            // description for shared texture (final rendered texture)
            Texture2DDescription color_desc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = _current_width,
                Height = _current_height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0), 
                Usage = ResourceUsage.Default, 
                OptionFlags = ResourceOptionFlags.Shared, 
                CpuAccessFlags = CpuAccessFlags.None, 
                ArraySize = 1
            };

            SharedTexture = new Texture2D(_device, color_desc);
            _render_target = new RenderTargetView(_device, SharedTexture);

            // create depth buffer
            Texture2DDescription depth_desc = new Texture2DDescription
            {
                BindFlags = BindFlags.DepthStencil, 
                Format = Format.R32_Typeless,
                Width = _current_width, 
                Height = _current_height,
                MipLevels = 1, 
                SampleDescription = new SampleDescription(1, 0), 
                Usage = ResourceUsage.Default, 
                OptionFlags = ResourceOptionFlags.None, 
                CpuAccessFlags = CpuAccessFlags.None, 
                ArraySize = 1
            };

            DepthStencilViewDescription depth_stencil_desc = new DepthStencilViewDescription
            {
                Format = Format.D32_Float,
                Dimension = DepthStencilViewDimension.Texture2D,
                MipSlice = 0
            };

            _depth_buffer = new Texture2D(_device, depth_desc);
            _depth_stencil_view = new DepthStencilView(_device, _depth_buffer, depth_stencil_desc);

            // viewport 
            ViewPort = new Viewport(0, 0, _current_width, _current_height, 0, 1);
            _device.Rasterizer.SetViewports(ViewPort);

            BlendStateDescription blend_desc = new BlendStateDescription
            {
                AlphaBlendOperation = BlendOperation.Add,
                BlendOperation = BlendOperation.Add, 
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha, 
                IsAlphaToCoverageEnabled = true, 
                SourceAlphaBlend = BlendOption.SourceAlpha, 
                SourceBlend = BlendOption.SourceAlpha
            };

            AlhpaBlend = BlendState.FromDescription(_device, blend_desc);
            _device.OutputMerger.BlendState = AlhpaBlend;

        }

        /// <summary>
        /// Creates a device for wpf. 
        /// </summary>
        /// <param name="feature_level"></param>
        private void CreateDevice(FeatureLevel feature_level)
        {
            _device = new Device1(DriverType.Hardware, DeviceCreationFlags.Debug | DeviceCreationFlags.BgraSupport, feature_level);
        }
        #endregion

        /// <summary>
        /// Creates a swapchain and the device for a forms window.
        /// </summary>
        /// <param name="window_handle"></param>
        /// <param name="feature_level"></param>
        private void CreateDeviceWithSwapChain(IntPtr window_handle, FeatureLevel feature_level)
        {
            SwapChainDescription swap_chain_desc = new SwapChainDescription()
            {
                BufferCount = 1,
                IsWindowed = true,
                ModeDescription = new ModeDescription(_current_width, _current_height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = _window_handle,
                SampleDescription = new SampleDescription(1, 0), 
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device1.CreateWithSwapChain(null, DriverType.Hardware, DeviceCreationFlags.Debug, feature_level,
                                        swap_chain_desc, out _device, out _swapchain);

            Factory factory = _swapchain.GetParent<Factory>();
            //factory.SetWindowAssociation(_window_handle, WindowAssociationFlags.IgnoreAll);
        }

        #endregion // construction and creation
        

        #region DxDevice Wrapper
        public void Present()
        {
            _swapchain.Present(_sync_interval, PresentFlags.None);
        }

        #endregion

        #region Functionality
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResizeBuffer(int width, int height)
        {

            // TODO FIX THIS: 
            // [5128] Swapchain cannot be resized unless all outstanding buffer references have been released.

            if (_current_width == width && _current_height == height) return;

            _current_width = width;
            _current_height = height;


            // rel  ease outstanding buffers
            //_device.ClearState();
            
            //_depth_stencil_view.Dispose();
            //_depth_buffer.Dispose();

            lock (_engine.RenderLock)
            {
                _device.ClearState();

                _depth_stencil_view.Dispose();
                _depth_stencil_view = null;

                _depth_buffer.Dispose();
                _depth_buffer = null;

                _render_target.Dispose();
                _render_target = null;

                _swapchain.ResizeBuffers(2, _current_width, _current_height, Format.R8G8B8A8_UNorm,
                                         SwapChainFlags.AllowModeSwitch);

                UpdateMainTextures();
                UpdateViewport();
            }

            RaiseBackbufferSizeChanged(new BackBufferSizeChangedEventArgs(width, height));
        }
        #endregion

        #region Event handling

        /// <summary>
        /// Backbuffer size changed event, will be raised if the size of the back buffer has been changed. 
        /// </summary>
        public event EventHandler<BackBufferSizeChangedEventArgs> OnBackbufferSizeChanged;
        private void RaiseBackbufferSizeChanged(BackBufferSizeChangedEventArgs e)
        {
            e.Raise(this, ref OnBackbufferSizeChanged);
        }

        /// <summary>
        /// DeviceInitialized event will be raised when the device will be initialized for the first time. 
        /// </summary>
        public event EventHandler<EventArgs> OnDeviceManagerInitialized;
        private void RaiseDeviceInitialized(EventArgs e)
        {
            e.Raise(this, ref OnDeviceManagerInitialized);
        }
        
        #endregion // event handling

        #region Disposing
        public void Dispose()
        {
            if (_depth_stencil_view != null && !_depth_stencil_view.Disposed)
            {
                _depth_stencil_view.Dispose();
                _depth_stencil_view = null;
            }

            if (_depth_buffer != null && !_depth_buffer.Disposed)
            {
                _depth_buffer.Dispose();
                _depth_buffer = null;
            }

            if (_render_target != null && !_render_target.Disposed)
            {
                _render_target.Dispose();
                _render_target = null;
            }

            if (_swapchain != null && !_swapchain.Disposed)
            {
                _swapchain.Dispose();
                _swapchain = null;
            }

            if (_device != null && !_device.Disposed)
            {
                _device.Dispose();
                _device = null;
            }
        }
        #endregion

        #region Properties
        public Device1 Device { get { return _device; } }
        public RenderTargetView RenderTarget { get { return _render_target; } }
        public DepthStencilView DepthView { get { return _depth_stencil_view; } }

        public SwapChain Swapchain { get { return _swapchain; } }
        public Viewport ViewPort { get; private set; }
        public Texture2D DepthBufferTexture { get { return _depth_buffer; } }
        // exchange texture 
        public Texture2D SharedTexture { get; set; }

        public int BackBufferWidth { get { return _current_width; } }
        public int BackBufferHeight { get { return _current_height; } }


      

        #endregion

        #region Attributes
        private IntPtr _window_handle;
        private int _current_width;
        private int _current_height;

        private bool _initialized;

        private Device1 _device;
        private SwapChain _swapchain;
        private int _sync_interval = 0;
        

        private RenderTargetView _render_target;
        private Texture2D _depth_buffer;
        private DepthStencilView _depth_stencil_view;

        public BlendState AlhpaBlend;
        private Engine _engine;

        #endregion
    }
}
