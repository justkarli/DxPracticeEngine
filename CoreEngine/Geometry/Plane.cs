using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.Geometry
{
    public class Plane : IShape, IDisposable
    {
        public Plane()
        {
            _plane_components = new float[4];
        }

        /// <summary>
        /// Defines a plane with it's side lengthes. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public Plane(float a, float b, float c, float d) : this()
        {
            _plane_components[0] = a;
            _plane_components[1] = b;
            _plane_components[2] = c;
            _plane_components[3] = d;
        }
        
        /// <summary>
        /// For distance from origin calculations. But do further readings for that!!
        /// </summary>
        public void Normalize()
        {
            float magnitude = (float) Math.Sqrt(A * A + B * B + C * C);

            for (int i = 0; i < _plane_components.Length; i++)
                _plane_components[i] /= magnitude;
        }

        public float DistanceToPoint(Vector3 pt)
        {
            return (A * pt.X + B * pt.Y + C * pt.Z + D);
        }

        #region Implementation of IShape

        public ShapeType ShapeType
        {
            get { return ShapeType.Plane; }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            _plane_components = null;
        }

        #endregion

        #region Properties
        public float A
        {
            get
            {
                if (_plane_components != null)
                    return _plane_components[0];

                return 0;
            }
        }

        public float B
        {
            get
            {
                if (_plane_components != null)
                    return _plane_components[1];

                return 0;
            }
        }

        public float C
        {
            get
            {
                if (_plane_components != null)
                    return _plane_components[2];

                return 0;
            }
        }

        public float D
        {
            get
            {
                if (_plane_components != null)
                    return _plane_components[3];

                return 0;
            }
        }
        #endregion // properties

        protected float[] _plane_components;

    }
}
