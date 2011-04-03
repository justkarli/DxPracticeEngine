using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;
using System.Windows;
using System.Windows.Resources;
namespace DxApplication
{
    public class MyDataModel
    {
        private byte[] _buffer;
        private D3DImageControl _control;

        public MyDataModel(D3DImageControl slimDXControl, string imagename, Color color)
        {
            _control = slimDXControl;
            Uri uri = new Uri("pack://application:,,,/Resources/" + imagename + ".png");

            using (Stream s = Application.GetResourceStream(uri).Stream)
            {
                _buffer = new byte[s.Length];
                s.Read(_buffer, 0, (int) s.Length);
            }

            Color = color;
        }

        public string Name { private set; get; }
        public Stream Stream
        {
            get
            {
                MemoryStream ms = new MemoryStream(_buffer);
                return ms;
            }
        }

        private Color m_color;
        public Color Color
        {
            get { return m_color; }
            set
            {
                if(m_color != value)
                {
                    m_color = value;
                    Color4 = new Color4(m_color);
                    _control.ForceRendering();
                }
            }
        }

        public Color4 Color4 { get; private set; }
    }

}
