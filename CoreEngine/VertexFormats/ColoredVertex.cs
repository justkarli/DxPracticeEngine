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
    public struct ColoredVertex : IEquatable<ColoredVertex>
    {
        public Vector3 Position { get; set; }
        public Color4 Color { get; set; }

        public ColoredVertex(Vector3 position, Color4 color)
            : this()
        {
            Position = position;
            Color = color;
        }

        public static bool operator ==(ColoredVertex left, ColoredVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator  !=(ColoredVertex left, ColoredVertex right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() + Color.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (GetType() != obj.GetType()) return false;
            return Equals((ColoredVertex) obj);
        }
        

        public bool Equals(ColoredVertex other)
        {
            return (Position == other.Position && Color == other.Color);
        }

        public static VertexElement[] VertexElements
        {
            get
            {
                if(_vertex_elements == null)
                {
                    _vertex_elements = new VertexElement[]
                    {
                        new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),                            
                        new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0), 
                        VertexElement.VertexDeclarationEnd
                    };
                }
                return _vertex_elements;
            }
        }

        public static InputElement[] InputElements
        {
            get
            {
                if(_input_elements == null)
                {
                    _input_elements = new[]
                                          {
                                              new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                                              new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
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
                    _type = typeof (ColoredVertex);
                }
                return _type;
            }
        }

        private static VertexElement[] _vertex_elements;
        private static InputElement[] _input_elements;
        private static int _size_of;
        private static Type _type;
    }
}
