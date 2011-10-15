using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.Geometry
{
    public class Triangle : IShape, IDisposable
    {
        public Triangle()
        {
            _geometry = new Vector3[3];
        }

        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3) : this()
        {
            _geometry[0] = v1;
            _geometry[1] = v2;
            _geometry[2] = v3;
        }

        #region Implementation of IShape

        public ShapeType ShapeType
        {
            get { return ShapeType.Triangle; }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            _geometry = null;
        }

        #endregion

        protected Vector3[] _geometry;
    }
}
