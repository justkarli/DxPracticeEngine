using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine
{
    public static class MatrixEx
    {
        public static string AsString(this Matrix matrix)
        {
            string matrix_as_string = matrix.M11 + " " + matrix.M12 + " " + matrix.M13 + " " + matrix.M14 + "\n";
            matrix_as_string += matrix.M21 + " " + matrix.M22 + " " + matrix.M23 + " " + matrix.M24 + "\n";
            matrix_as_string += matrix.M31 + " " + matrix.M32 + " " + matrix.M33 + " " + matrix.M34 + "\n";
            matrix_as_string += matrix.M41 + " " + matrix.M42 + " " + matrix.M43 + " " + matrix.M44 + "\n";
            
            return matrix_as_string;
        }
    }
}
