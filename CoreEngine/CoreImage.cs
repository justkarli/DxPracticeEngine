using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using SlimDX;
using SlimDX.Direct3D9;


namespace CoreEngine
{
    /// <summary>
    /// Customized image to display dx(10) in wpf. 
    /// </summary>
    public class CoreImage : Image
    {
        public CoreImage()
        {
            InitD3D9();
            NumActiveImages++;

            _d3dImage = new D3DImage();
            Stretch = System.Windows.Media.Stretch.None;
            Source = _d3dImage;
        }

        public void Dispose()
        {
            SetBackBufferSlimDX(null);
            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            NumActiveImages--;
            ShutdownD3D9();
        }

        public void InvalidateD3DImage()
        {
            if (SharedTexture != null)
            {
                _d3dImage.Lock();
                _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
                //AddDirtyRect(new Int32Rect(0, 0, 800, 600));
                _d3dImage.Unlock();
            }
        }

        public void SetBackBufferSlimDX(SlimDX.Direct3D10.Texture2D Texture)
        {
            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (Texture == null)
            {
                if (SharedTexture != null)
                {
                    SharedTexture = null;
                    _d3dImage.Lock();
                    _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                    _d3dImage.Unlock();
                }
            }
            else if (IsShareable(Texture))
            {
                Format format = TranslateFormat(Texture);
                if (format == Format.Unknown)
                    throw new ArgumentException("Texture format is not compatible with OpenSharedResource");

                IntPtr Handle = GetSharedHandle(Texture);
                if (Handle == IntPtr.Zero)
                    throw new ArgumentNullException("Handle");

                SharedTexture = new Texture(D3DDevice, Texture.Description.Width, Texture.Description.Height, 1, Usage.RenderTarget, format, Pool.Default, ref Handle);
                using (Surface Surface = SharedTexture.GetSurfaceLevel(0))
                {
                    _d3dImage.Lock();
                    _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, Surface.ComPointer);
                    _d3dImage.Unlock();
                }
            }
            else
                throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");
        }

       private void InitD3D9()
        {
            if (NumActiveImages == 0)
            {
                D3DContext = new Direct3DEx();

                PresentParameters presentparams = new PresentParameters();
                presentparams.Windowed = true;
                presentparams.SwapEffect = SwapEffect.Discard;
                presentparams.DeviceWindowHandle = GetDesktopWindow();
                presentparams.PresentationInterval = PresentInterval.Immediate;

                D3DDevice = new DeviceEx(D3DContext, 0, DeviceType.Hardware, IntPtr.Zero, CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve, presentparams);
            }
        }

        private void ShutdownD3D9()
        {
            if (NumActiveImages == 0)
            {
                if (SharedTexture != null)
                {
                    SharedTexture.Dispose();
                    SharedTexture = null;
                }

                if (D3DDevice != null)
                {
                    D3DDevice.Dispose();
                    D3DDevice = null;
                }

                if (D3DContext != null)
                {
                    D3DContext.Dispose();
                    D3DContext = null;
                }
            }
        }

        IntPtr GetSharedHandle(SlimDX.Direct3D10.Texture2D Texture)
        {
            SlimDX.DXGI.Resource resource = new SlimDX.DXGI.Resource(Texture);
            IntPtr result = resource.SharedHandle;

            resource.Dispose();

            return result;
        }

        Format TranslateFormat(SlimDX.Direct3D10.Texture2D Texture)
        {
            switch (Texture.Description.Format)
            {
                case SlimDX.DXGI.Format.R10G10B10A2_UNorm:
                    return SlimDX.Direct3D9.Format.A2B10G10R10;

                case SlimDX.DXGI.Format.R16G16B16A16_Float:
                    return SlimDX.Direct3D9.Format.A16B16G16R16F;

                case SlimDX.DXGI.Format.B8G8R8A8_UNorm:
                    return SlimDX.Direct3D9.Format.A8R8G8B8;

                default:
                    return SlimDX.Direct3D9.Format.Unknown;
            }
        }

        bool IsShareable(SlimDX.Direct3D10.Texture2D Texture)
        {
            return (Texture.Description.OptionFlags & SlimDX.Direct3D10.ResourceOptionFlags.Shared) != 0;
        }

        public D3DImage D3DImage { get { return _d3dImage; } }

        private D3DImage _d3dImage;
        private Texture SharedTexture;

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr GetDesktopWindow();

        static int NumActiveImages = 0;
        static Direct3DEx D3DContext;
        static DeviceEx D3DDevice;
    }
}

