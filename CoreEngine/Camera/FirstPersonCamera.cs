using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;

namespace CoreEngine.Camera
{
    class FirstPersonCamera : CameraV2 
    {
        public enum ControlKeys
        {
            ForwardKey = 0,
            BackKey,
            LeftKey,
            RightKey,
            UpKey,
            DownKey,
            SpeedUpKey,

            NumControlKeys
        }

        public FirstPersonCamera(EventHub event_hub)
        {
            _event_hub = event_hub;
            _pressed_keys = new bool[(int)ControlKeys.NumControlKeys];
            _view = new Vector3(0, 0, 1);
            _up = new Vector3(0, 1, 0);

            _event_hub.OnKeyDownEvent += new EventHandler<KeyEventArgs>(_event_hub_OnKeyDownEvent);
            _event_hub.OnKeyUpEvent += new EventHandler<KeyEventArgs>(_event_hub_OnKeyUpEvent);

            _event_hub.OnMouseMoveEvent += new EventHandler<MouseEventArgs>(_event_hub_OnMouseMoveEvent);
            _event_hub.OnMouseLeaveEvent += new EventHandler<EventArgs>(_event_hub_OnMouseLeaveEvent);
            _event_hub.OnMouseDownEvent += new EventHandler<MouseEventArgs>(_event_hub_OnMouseDownEvent);
            _event_hub.OnMouseUpEvent += new EventHandler<MouseEventArgs>(_event_hub_OnMouseUpEvent);

            _timer = new Stopwatch();
            _timer.Start();
        }

        void _event_hub_OnMouseUpEvent(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
                _last_mouse_pos = new Vector2(InvalidMousePos, InvalidMousePos);
        }

        void _event_hub_OnMouseDownEvent(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
                _last_mouse_pos = new Vector2(InvalidMousePos, InvalidMousePos);
        }

        void _event_hub_OnMouseLeaveEvent(object sender, EventArgs e)
        {
            _last_mouse_pos = new Vector2(InvalidMousePos, InvalidMousePos);
        }

        void _event_hub_OnMouseMoveEvent(object sender, MouseEventArgs e)
        {
            _mouse_delta = Vector2.Zero;
            int mouse_x = e.X;
            int mouse_y = e.Y;

            // only rotate the camera when dragging the right mouse
            if(e.Button == MouseButtons.Right && _last_mouse_pos != new Vector2(InvalidMousePos, InvalidMousePos))
                _mouse_delta = new Vector2(mouse_x - _last_mouse_pos.X, mouse_y - _last_mouse_pos.Y);

            _last_mouse_pos = new Vector2(mouse_x, mouse_y);
        }

        void _event_hub_OnKeyUpEvent(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < (int) ControlKeys.NumControlKeys; i++ )
            {
                if (e.KeyCode == KeyBindings[i])
                    _pressed_keys[i] = false;
            }
        }

        void _event_hub_OnKeyDownEvent(object sender, KeyEventArgs e)
        {
            for (int i = 0; i < (int)ControlKeys.NumControlKeys; i++)
            {
                if (e.KeyCode == KeyBindings[i])
                    _pressed_keys[i] = true;
            }
        }

        /// <summary>
        /// Modulo the range of a given angle such that -Pi <= Angle < Pi
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float WrapAngle(float angle)
        {
            float tmp;

            // normalize the range from 0.0f to 2Pi
            angle = (float) (angle + Math.PI);

            // perform modulo unsigned
            tmp = Math.Abs(angle);
            tmp = (float)(tmp - (Math.PI * 2 * (int)(tmp / (Math.PI * 2))));

            // restore number to the range of -pi to pi - epsilon
            tmp = (float) (tmp - Math.PI);

            if (angle < 0f)
                tmp = -tmp;

            return tmp;
        }

        public override void Update()
        {
            float time_delta = _timer.ElapsedMilliseconds / 1000f;

            float cam_move_speed = 10f * time_delta;
            float cam_rot_speed = 15f * time_delta;

            if (_pressed_keys[(int)ControlKeys.SpeedUpKey])
                cam_move_speed *= 3f;
            
            // move the camera with the board
            if(_pressed_keys[(int)ControlKeys.RightKey])
                _eye += _right_vector * cam_move_speed;
            else if(_pressed_keys[(int)ControlKeys.LeftKey])
                _eye += -(_right_vector) * cam_move_speed;

            if(_pressed_keys[(int)ControlKeys.DownKey])
                _eye += _up_vector * cam_move_speed;
            else if(_pressed_keys[(int) ControlKeys.UpKey])
                _eye += -(_up_vector) * cam_move_speed;

            if(_pressed_keys[(int)ControlKeys.ForwardKey])
                _eye += _view_vector * cam_move_speed;
            else if(_pressed_keys[(int)ControlKeys.BackKey])
                _eye += -(_view_vector) * cam_move_speed;

            // rotate the camera with the mouse
            _rotation.X += _mouse_delta.X * cam_rot_speed;
            _rotation.Y += _mouse_delta.Y * cam_rot_speed;
            
            _mouse_delta = Vector2.Zero;
            if (_rotation.X < -(Math.PI * 0.5f))
                _rotation.X = (float)(-(Math.PI * 0.5f));
            else if (_rotation.X > (Math.PI * 0.5f))
                _rotation.X = (float) (Math.PI * 0.5f);


            _rotation.Y = WrapAngle(_rotation.Y);

            // Make a rotaiton matrix from X/Y rotation
            Matrix.RotationYawPitchRoll(_rotation.X, _rotation.Y, 0, out _rotation_matrix);

            // create new view and up vectors
            Vector3.TransformCoordinate(ref _view_vector, ref _rotation_matrix, out _view);
            Vector3.TransformCoordinate(ref _up_vector, ref _rotation_matrix, out _up);
            
            // take into account eye position
            _view = _eye + _view;

            // update vie matrix
            Matrix.LookAtRH(ref _eye, ref _view, ref _up, out _view_matrix);

            _timer.Reset();
            _timer.Start();
            
        }
        

        private EventHub _event_hub;
        private Stopwatch _timer;
        private bool[] _pressed_keys;

        private Vector2 _last_mouse_pos;
        private Vector2 _mouse_delta;
        private Vector2 _rotation;

        private Vector3 _up, _view;

        // view vectors (default view and up vectors)
        private Vector3 _view_vector = new Vector3(0, 0, -1);
        private Vector3 _up_vector = new Vector3(0, 1, 0);
        private Vector3 _right_vector = new Vector3(1, 0, 0);

        public static Keys[] KeyBindings = 
        { 
            Keys.W, 
            Keys.S,
            Keys.A, 
            Keys.D,
            Keys.Q,
            Keys.E, 
            Keys.Space
        };

        public const int InvalidMousePos = -99999;
        
    }
}
