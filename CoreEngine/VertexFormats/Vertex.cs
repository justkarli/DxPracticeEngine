using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.Direct3D9;
using Format = SlimDX.DXGI.Format;

namespace CoreEngine.VertexFormats
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : IEquatable<Vertex>
    {
        public Vector3 Position { get; set; }
        public Vector2 TexCoord { get; set; }

        public Vertex(Vector3 position, Vector2 tex_coord)
            : this()
        {
            Position = position;
            TexCoord = tex_coord;
        }

        public static bool operator ==(Vertex left, Vertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() + TexCoord.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (GetType() != obj.GetType()) return false;
            return Equals((Vertex)obj);
        }


        public bool Equals(Vertex other)
        {
            return (Position == other.Position && TexCoord == other.TexCoord);
        }

        public static InputElement[] InputElements
        {
            get
            {
                if(_input_elements == null)
                {
                    _input_elements = new []
                    {
                       new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0), 
                       new InputElement("TEXCOORD", 0, Format.R32G32_Float, 12, 0)
                    };
                }
                return _input_elements;
            }
        }

        public static int SizeOf
        {
            get
            {
                if(_size_of == 0)
                {
                    _size_of = Marshal.SizeOf(Type);
                }
                return _size_of;
            }
        }

        public static Type Type
        {
            get
            {
                if(_type == null)
                {
                    _type = typeof (Vertex);
                }
                return _type;
            }
        }

        private static InputElement[] _input_elements;
        private static int _size_of;
        private static Type _type;
    }
}
