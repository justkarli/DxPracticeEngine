using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.SceneManager
{
    public enum RenderPass
    {
        Pass0 = 0,
        Static = Pass0,
        Actor,
        Sky,
        Last
    }

    public enum AlphaType
    {
        AlphaOpaque,
        AlphaTexture,
        AlphaMaterial,
        AlphaVertex
    }

    public class SceneNode : ISceneNode
    {
        #region Constructor and Initialization
        public SceneNode(int actor_id, String name, RenderPass pass, Matrix? to_world, Matrix? from_world) 
        {
            _properties = new SceneNodeProperties();
            _properties.ActorId = actor_id;
            _properties.Name = name;
            _properties.RenderPass = pass;
            _properties.Alphatype = AlphaType.AlphaOpaque;

            SetTransform(to_world, from_world);
            SetRadius(0);

        }
        #endregion

        public void SetAlpha(float alpha)
        {
            _properties.SetAlpha(alpha);
        }

        //????????
        public Vector3 Position { get; set; }

        public void SetRadius(float radius)
        {
            _properties.Radius = radius;
        }

        public void SetMaterial(MaterialStructure material_structure)
        {
            _properties.Material = material_structure;
        }


        #region Implementation of ISceneNode

        public SceneNodeProperties SceneNodeProperties()
        {
            return _properties;
        }

        public void SetTransform(Matrix? toWorld, Matrix? fromWorld)
        {
            _properties.ToWorld = toWorld.Value;

            if (!fromWorld.HasValue)
            {
                _properties.FromWorld = _properties.ToWorld;
                _properties.FromWorld.Invert();
            } else
            {
                _properties.FromWorld = fromWorld.Value;
            }
        }

        public bool OnUpdate(Scene scene, float elapsed)
        {
            foreach (ISceneNode children in _children)
                children.OnUpdate(scene, elapsed);

            return true;
        }

        public bool OnRestore(Scene scene)
        {
            foreach(ISceneNode children in _children)
                children.OnRestore(scene);

            return true;
        }

        public bool PreRender(Scene scene)
        {
            // TODO call scene.pushandsetmatrix
            return true;
        }

        public bool Render(Scene scene)
        {
            // should set proper render states and material textures... but aehm.. bullshit
            return true;
        }

        public bool RenderChildren(Scene scene)
        {
            // iterate through all scene childrens and tell them to render
            foreach(ISceneNode children in _children)
            {
                if(children.PreRender(scene))
                {
                    // could short circuit rendering
                    // if an object returns efail from pre render
                    // Prerender

                    // don't render node if its not visible
                    if(children.IsVisible(scene))
                    {
                        float alpha = children.SceneNodeProperties().Material.Alpha;
                        if(alpha == 1.0f)
                        {
                            // opaque
                            children.Render(scene);
                        } else
                        {
                            // it's a transparent node
                            AlphaSceneNode asn = new AlphaSceneNode();
                            asn.Node = children as SceneNode;
                            //asn.Concat = scene.gettopmatrix

                            //Vector4 worldPos = asn.Concat.getposition;
                            //Matrix fromWorld = scene.getcamera.get.fromworld
                            //Vector4 screenpos = fromworld.xform(worldpos);
                            //asn.ScreenZ = screenpos.z;
                            
                            //scene.addAlhpascenenode(asn);

                        }
                    }

                    children.RenderChildren(scene);

                }
                children.PostRender(scene);
            }
            
            return true;
        }

        public bool PostRender(Scene scene)
        {
            // TODO call scene.popmatrix
            return true;
        }

        public bool IsVisible(Scene scene)
        {
            // transform the location of this node into camera space to check if it's in camera frustum
            // TODO further scene implementation
            throw new NotImplementedException();
        }

        public bool AddChild(ISceneNode kid)
        {
            _children.Add(kid);

            // radius of sphere shoud be fixed right here
            //Vector3 kidPos = kid.SceneNodeProperties().ToWorld.getposition;
            //Vector3 dir = kidPos - _properties.ToWorld.getposition;
            //float newRadius = dir.length + kid.SceneNodeProperties().Radius;
            //if (newRadius > _properties.Radius)
            //    _properties = newRadius;

            return true;
        }

        #endregion
        
        #region Implementation of IDisposable

        public void Dispose()
        {
            _properties = null;

            if (_children != null || _children.Count >= 0)
            {
                _children.Clear();
                _children = null;
            }

        }

        #endregion

        #region Attributes
        public List<AlphaSceneNode> AlphaSceneNodes;
        
        protected List<ISceneNode> _children;
        protected SceneNode _parent;
        protected SceneNodeProperties _properties;
        #endregion
    }
    

}
