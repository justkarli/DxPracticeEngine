using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;
using SlimDX.Direct3D9;

namespace DxApplication
{
    public class MyRenderEngine : IRenderEngine
    {
        private Sprite _sprite;
        private Texture _texture;
        private MyDataModel _dataModel;
        private VertexBuffer _vertex_buffer;

        public MyRenderEngine(MyDataModel dataModel) : base()
        {
            _dataModel = dataModel;
        }

        public void OnDeviceCreated(object sender, EventArgs e)
        {
            return;
        }

        private void WriteVertexBuffer()
        {
            DataStream stream = _vertex_buffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(new[] {
				new ColoredVertex( new Vector3(0.0f, 0.5f, 0.5f), Color.Red.ToArgb() ),
				new ColoredVertex( new Vector3(0.5f, -0.5f, 0.5f), Color.Blue.ToArgb() ),
				new ColoredVertex( new Vector3(-0.5f, -0.5f, 0.5f), Color.Green.ToArgb() ),
                new ColoredVertex( new Vector3(-0.8f, 0.3f, 0.5f), Color.Gray.ToArgb() )
			});
            _vertex_buffer.Unlock();
        }

        public void OnDeviceDestroyed(object sender, EventArgs e)
        {
            if (_sprite != null && !_sprite.Disposed)
            {
                _sprite.Dispose();
            }
            if (_texture != null && !_texture.Disposed)
            {
                _texture.Dispose();
                _texture = null;
            }
        }

        public void OnDeviceLost(object sender, EventArgs e)
        {
            if (_sprite != null && !_sprite.Disposed)
            {
                _sprite.Dispose();
            }
            if (_texture != null && !_texture.Disposed)
            {
                _texture.Dispose();
                _texture = null;
            }
        }

        public void OnDeviceReset(object sender, EventArgs e)
        {
            D3DImageControl control = sender as D3DImageControl;
            if(control == null) 
                throw new ArgumentNullException("sender");

            _vertex_buffer = new VertexBuffer(control.Device, 4 * ColoredVertex.SizeOf, Usage.WriteOnly, VertexFormat.None, Pool.Default);
            WriteVertexBuffer();
            control.Device.SetRenderState(RenderState.Lighting, false);
            if (_sprite != null)
            {
                _sprite.Dispose();
            }
            _sprite = new Sprite(control.Device);

            if (_texture == null)
                _texture = Texture.FromStream(control.Device, _dataModel.Stream, Usage.None, Pool.Default);

            return;
        }

        public void OnMainLoop(object sender, EventArgs e)
        {
            D3DImageControl control = sender as D3DImageControl;
            // drawing 2d
            //_sprite.Begin(SpriteFlags.AlphaBlend);
            //_sprite.Draw(_texture, Vector3.Zero, Vector3.Zero, _dataModel.Color4);
            //_sprite.End();

            control.Device.SetStreamSource(0, _vertex_buffer, 0, ColoredVertex.SizeOf);
            control.Device.VertexDeclaration = new VertexDeclaration(control.Device, ColoredVertex.VertexElements);
            control.Device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }
    }
}
