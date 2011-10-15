using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.SceneManager
{
    public struct AlphaSceneNode
    {
        public SceneNode Node;
        public Matrix Concat;
        public float ScreenZ;

        public static bool operator <(AlphaSceneNode left, AlphaSceneNode right)
        {
            return left.ScreenZ < right.ScreenZ;
        }

        public static bool operator >(AlphaSceneNode left, AlphaSceneNode right)
        {
            return left.ScreenZ > right.ScreenZ;
        }

    }
}
