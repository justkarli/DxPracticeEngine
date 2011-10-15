using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.SceneManager
{
    public interface ISceneNode : IDisposable
    {
        SceneNodeProperties SceneNodeProperties();
        void SetTransform(Matrix? toWorld, Matrix? fromWorld);
        
        bool OnUpdate(Scene scene, float elapsed);
        bool OnRestore(Scene scene);
        bool PreRender(Scene scene);
        bool Render(Scene scene);
        bool RenderChildren(Scene scene);
        bool PostRender(Scene scene);

        bool IsVisible(Scene scene);
        bool AddChild(ISceneNode kid);
    }
}
