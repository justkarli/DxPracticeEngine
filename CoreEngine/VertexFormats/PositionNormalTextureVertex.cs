using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;

namespace CoreEngine.VertexFormats
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PositionNormalTextureVertex : IEquatable<PositionNormalTextureVertex>
    {

        #region Constructor

        public PositionNormalTextureVertex(Vector3 position, Vector3 normal, Vector2 texcoord) : this()
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = texcoord;
        }

        #endregion

        #region Structure

        public Vector3 Position { get; set; }
        public Vector3 Normal { get; set; }
        public Vector2 TextureCoordinate { get; set; }

        #endregion

        public static bool operator ==(PositionNormalTextureVertex left, PositionNormalTextureVertex right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PositionNormalTextureVertex left, PositionNormalTextureVertex right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode() + Normal.GetHashCode() + TextureCoordinate.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;

            return Equals((PositionNormalTextureVertex) obj);
        }

        #region Implementation of IEquatable<PositionNormalTextureVertex>

        public bool Equals(PositionNormalTextureVertex other)
        {
            return (Position == other.Position && Normal == other.Normal && TextureCoordinate == other.TextureCoordinate);
        }

        #endregion

        public static InputElement[] InputElements
        {
            get
            {
                if(_input_elements == null || _input_elements.Length <= 0)
                {
                    _input_elements = new InputElement[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                        new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0), 
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0) 
                    };
                }

                return _input_elements;
            }
        }

        private static InputElement[] _input_elements;


    }
}
