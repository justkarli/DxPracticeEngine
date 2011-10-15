using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CoreEngine;
using SlimDX.Direct3D10_1;
using SlimDX.Windows;

namespace DxFormsApplication
{
    public partial class DxForm : Form
    {
        public DxForm()
        {
            InitializeComponent();

            _engine = new Engine();
            _engine.DeviceManager.SetWidthAndHeight(Width, Height);
            _engine.DeviceManager.Initialize(Handle, FeatureLevel.Level_10_1);

            _engine.EventHub.RegisterKeyEvents(this);
            _engine.EventHub.RegisterMouseEvents(this);
        }

        public void Run()
        {
            _current_form_window_state = WindowState;
            
            bool is_form_closed = false;
            bool is_form_resizing = false;

            Resize += (o, args) =>
            {
                if(WindowState != _current_form_window_state)
                    HandleResize();

                _current_form_window_state = WindowState;
            };

            ResizeBegin += (o, args) => { is_form_resizing = true; };
            ResizeEnd += (o, args) =>
            {
                is_form_resizing = false;
                HandleResize();
            };

            Closed += (o, args) => { is_form_closed = true; };

            MessagePump.Run(this, () =>
            {
                if (is_form_closed) return;

                if (!is_form_resizing)
                    _engine.FrameTick();
            });

        }

        private void HandleResize()
        {
            if (WindowState == FormWindowState.Minimized) return;

            // Dispose render target and all contexts connected to the device
            // resize swap chain 
            // reinitialize textures, states, viewports which are dependent on the swap chain size 
        }

        #region Attributes

        private FormWindowState _current_form_window_state;
        private Engine _engine;
        private bool _is_fullscreen = false;


        #endregion

    }
}
