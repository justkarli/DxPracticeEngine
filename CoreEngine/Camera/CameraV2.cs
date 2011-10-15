using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace CoreEngine.Camera
{
    public class CameraV2
    {
        public CameraV2()
        {
            _near_plane = 0.1f;
            _far_plane = 100f;
            _aspect_ratio = 1280 / 800;
            _fov = (float) (Math.PI / 4);

            _eye = new Vector3(0, 0, 10);
            _proj_matrix = Matrix.Identity;
            _view_matrix = Matrix.Identity;
            _rotation_matrix = Matrix.Identity;
        }

        public void SetProjectionParams(float zn, float zf, float aspect, float fov)
        {
            _near_plane = zn;
            _far_plane = zf;
            _aspect_ratio = aspect;
            _fov = fov;

            // calculate and set the projection matrix 
            Matrix.PerspectiveFovRH(_fov, _aspect_ratio, _near_plane, _far_plane, out _proj_matrix);
        }

        public virtual void Update() { }

        public Matrix ProjMatrix { get { return _proj_matrix; } }
        public Matrix ViewMatrix { get { return _view_matrix; } }

        protected Matrix _proj_matrix;
        protected Matrix _view_matrix;
        protected Matrix _rotation_matrix;

        protected Vector3 _eye;

        protected float _near_plane;
        protected float _far_plane;
        protected float _aspect_ratio;
        protected float _fov;
    }
}
