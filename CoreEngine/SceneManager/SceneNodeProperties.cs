using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.SceneManager
{
    /// <summary>
    /// Properties of a scene node, will be configured in scene node class. (internal?)
    /// </summary>
    public class SceneNodeProperties
    {
        // sets the alpha value of the material
        public void SetAlpha(float alpha)
        {
            Alphatype = AlphaType.AlphaMaterial;
            //Material.Alpha = alpha;
        }

        public void Transform(Matrix? toWorld, Matrix? fromWorld)
        {
            if (toWorld.HasValue)
                ToWorld = toWorld.Value;

            if (fromWorld.HasValue)
                FromWorld = fromWorld.Value;
        }

        public bool HasAlpha()
        {
            //return (Material.Diffuse.Alpha != 1.0f);
            return true;
        }

        public virtual float Alpha()
        {
            return Material.Alpha;
        }

        public int ActorId { get; set; }
        public String Name { get; set; }
        public Matrix ToWorld { get; set; }
        public Matrix FromWorld { get; set; }
        public float Radius { get; set; }
        public RenderPass RenderPass { get; set; }
        public MaterialStructure Material { get; set; }
        public AlphaType Alphatype { get; set; }
    }
}
