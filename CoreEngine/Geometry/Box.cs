using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.Geometry
{
    public class Box : IShape, IDisposable
    {
        public Box()
        {
            Axis = new Vector3[3];
            Extent = new float[3];
        }

        public Box(Vector3 center, Vector3 forward, Vector3 up, Vector3 right, 
            float fextents, float uextents, float rextents) : this()
        {
            Center = center;

            Axis[0] = forward;
            Axis[1] = up;
            Axis[2] = right;

            Extent[0] = fextents;
            Extent[1] = uextents;
            Extent[2] = rextents;
        }

        #region Implementation of IShape

        public ShapeType ShapeType
        {
            get { return ShapeType.Box; }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Axis = null;
            Extent = null;
        }

        #endregion

        public Vector3 Center;
        public Vector3[] Axis;
        public float[] Extent;
    }
}
