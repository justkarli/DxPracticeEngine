using System;
using System.Windows.Forms;
using CoreEngine;
using SlimDX.Direct3D10_1;
using SlimDX.Windows;

namespace DxFormApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _engine = new Engine();
            _engine.DeviceManager.SetWidthAndHeight(ClientSize.Width, ClientSize.Height);
            _engine.DeviceManager.Initialize(Handle, FeatureLevel.Level_10_1);
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            _engine.EventHub.RegisterKeyEvents(this);
            _engine.EventHub.RegisterMouseEvents(this);
        }

        public void Run()
        {
            _current_form_windowstate = WindowState;
            bool is_form_closed = false;
            bool is_form_resizing = false;

            // attach to form event handler
            //KeyDown += OnKeyDown;
            //KeyUp += OnKeyUp;

            Resize += (o, ev_args) => // resize delegate, just resize the form if the window state has changed
            {
                if(WindowState != _current_form_windowstate)
                    HandleResize(o, ev_args);

                _current_form_windowstate = WindowState;
            };

            ResizeBegin += (o, args) => { is_form_resizing = true; };

            ResizeEnd += (o, args) =>
            {
                is_form_resizing = false; // now do the resizing
                HandleResize(o, args);
            };

            // main render loop exit signal
            FormClosed += (o, args) => { is_form_closed = true; };

            MessagePump.Run(this, () =>
            {
                if (is_form_closed) return;

                if (!is_form_resizing)
                    _engine.FrameTick();
            });

            // dispose mechanism
        }

        private void HandleResize(object sender, EventArgs ev_args)
        {
            // tell device manager that the size of the window has been changed 
            _engine.DeviceManager.ResizeBuffer(ClientSize.Width, ClientSize.Height);
        }

        private FormWindowState _current_form_windowstate;
        private Engine _engine;

        #region Key event handling

        //void OnKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Left)
        //    {
        //        _engine.Camera.SetMovementToggle(2, -1);
        //    }
        //    else if (e.KeyCode == Keys.Right)
        //    {
        //        _engine.Camera.SetMovementToggle(3, 1);
        //    }
        //    else if (e.KeyCode == Keys.Up)
        //    {
        //        _engine.Camera.SetMovementToggle(0, -1);
        //    }
        //    else if (e.KeyCode == Keys.Down)
        //    {
        //        _engine.Camera.SetMovementToggle(1, 1);
        //    }
        //}

        //void OnKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Left)
        //    {
        //        _engine.Camera.SetMovementToggle(2, 1);
        //    }
        //    else if (e.KeyCode == Keys.Right)
        //    {
        //        _engine.Camera.SetMovementToggle(3, -1);
        //    }
        //    else if (e.KeyCode == Keys.Up)
        //    {
        //        _engine.Camera.SetMovementToggle(0, 1);
        //    }
        //    else if (e.KeyCode == Keys.Down)
        //    {
        //        _engine.Camera.SetMovementToggle(1, -1);
        //    }
        //}
       
        #endregion
    }
}
