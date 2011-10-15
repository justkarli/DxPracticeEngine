using System;
using System.Diagnostics;
using SlimDX;

namespace CoreEngine.Camera
{
    public class Camera : IDisposable
    {
        public const float TWO_PI = 6.283185307179586476925286766559f;
        public const float DEG_TO_RAD = 0.01745329251994329576923690768489f;
        
        public Camera()
        {
            
            _eye = Vector3.Zero;
            _view = new Vector3(0, 0, 1);
            _up = new Vector3(0, 1, 0);
            _forward = new Vector3(0, 0, 1);
            _strafe_right = new Vector3(1, 0, 0);
            _heading = 0;
            _pitch = 0;

            _view_matrix = Matrix.Identity;
            _projection_matrix = Matrix.Identity;
            _timer = new Stopwatch();
            _timer.Start();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Sets the projection matrix. 
        /// </summary>
        /// <param name="fov">field of view in degrees</param>
        /// <param name="aspect_ratio"></param>
        /// <param name="z_near"></param>
        /// <param name="z_far"></param>
        public void SetPerspectiveProjection(float fov, float aspect_ratio, float z_near, float z_far)
        {
            // fov from degrees to radians, 640:480 -> 4:3 -> 1.3333...
            fov = fov * DEG_TO_RAD;
            Matrix.PerspectiveFovRH(fov, aspect_ratio, z_near, z_far, out _projection_matrix);
        }

        /// <summary>
        /// For kinect 3dimensional viewing.
        /// </summary>
        /// <param name="fov"></param>
        /// <param name="aspect_ratio"></param>
        /// <param name="z_near"></param>
        /// <param name="z_far"></param>
        public void SetPerspectiveOffCenter(float fov, float aspect_ratio, float z_near, float z_far)
        {
            // fov from degrees to radians, 640:480 -> 4:3 -> 1.3333...
            fov = fov * DEG_TO_RAD;
            //Matrix.PerspectiveFovRH(fov, aspect_ratio, z_near, z_far, out _projectionMatrix);
            float left = z_near * (-0.5f * aspect_ratio + _eye.X) / _eye.Z;
            float right = z_near * (0.5f * aspect_ratio + _eye.X) / _eye.Z;
            float bottom = z_near * (-0.5f - _eye.Y) / _eye.Z;
            float top = z_near * (0.5f - _eye.Y) / _eye.Z;
            Matrix.PerspectiveOffCenterRH(left, right, bottom, top, z_near, z_far, out _projection_matrix);
        }

        public void SetPositionAndView(float x, float y, float z, float hDeg, float pDeg)
        {
            _eye.X = x;
            _eye.Y = y;
            _eye.Z = z;

            // set heading and pitch
            _heading = hDeg * DEG_TO_RAD;
            _pitch = pDeg * DEG_TO_RAD;

            // update view
            UpdateView();
        }

        public void AdjustHeadingPitch(float h_rad, float p_rad)
        {
            _heading += h_rad;
            _pitch += p_rad;

            // value clamping, keap heading and pitch between 0 and 2pi
            if (_heading > TWO_PI) _heading -= TWO_PI;
            else if (_heading < 0) _heading = TWO_PI + _heading;

            if (_pitch > TWO_PI) _pitch -= TWO_PI;
            else if (_pitch < 0) _pitch = TWO_PI + _pitch;
        }

        public void SetMovementToggle(int i, int v)
        {
            _movement_toggles[i] = v;
        }

        private void UpdateView()
        {
            // create rotation matrix
            Matrix.RotationYawPitchRoll(_heading, _pitch, 0, out _rotation_matrix);

            // create new view and up vectors
            Vector3.TransformCoordinate(ref _view_vector, ref _rotation_matrix, out _view);
            Vector3.TransformCoordinate(ref _up_vector, ref _rotation_matrix, out _up);

            // create new forward and strafe vectors
            Vector3.Normalize(ref _view, out _forward);
            Vector3.Cross(ref _up, ref _view, out _strafe_right);
            _strafe_right.Normalize();

            // take into account eye position
            _view = _eye + _view;
            //_view = new Vector3(_eye.X, _eye.Y, 0);
            // update vie matrix
            Matrix.LookAtRH(ref _eye, ref _view, ref _up, out _view_matrix);
        }

        public void Update()
        {
            float t = (float) _timer.ElapsedMilliseconds / 1000;

            // update position - 1.5 unit per second
            _eye += t*(_movement_toggles[0] + _movement_toggles[1])*1.5f*_forward +
                    t*(_movement_toggles[2] + _movement_toggles[3])*1.5f*_strafe_right;

            UpdateView();
            _timer.Reset();
            _timer.Start();
        }

        public void Update(Vector3 eye)
        {
            _eye = eye;
            UpdateView();
            SetPerspectiveProjection(45, 4/3, 0.05f, 100);
        }

        public Matrix ViewMatrix { get { return _view_matrix; } }
        public Matrix ProjectionMatrix { get { return _projection_matrix; } }

        public Vector3 Eye { get { return _eye; } }
        
        // view parameters in radians
        private float _heading; 
        private float _pitch;

        // matrices
        private Matrix _view_matrix;
        private Matrix _projection_matrix;
        private Matrix _rotation_matrix;

        // view vectors (default view and up vectors)
        private Vector3 _view_vector = new Vector3(0, 0, -1);
        private Vector3 _up_vector = new Vector3(0, 1, 0);

        private Vector3 _eye, _view, _up;

        // movement vectors and movement toogles
        private Vector3 _forward, _strafe_right;
        private readonly int[] _movement_toggles = new int[4]; // fwrd, back, strfLeft, strfRight

        private readonly Stopwatch _timer;
    }
}
