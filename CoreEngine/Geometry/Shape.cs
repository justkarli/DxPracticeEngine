using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreEngine.Geometry
{
    public enum ShapeType
    {
        Sphere, 
        Box, 
        Triangle, 
        Ray, 
        Plane, 
        Frustum, 
        Line
    }

    public interface IShape
    {
        ShapeType ShapeType { get; }
    }
}
