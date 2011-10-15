using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CoreEngine.Utilities;


namespace CoreEngine
{
    public class EventHub
    {
        public void RegisterMouseEvents(Form form)
        {
            form.MouseMove += (form_MouseMove);
            form.MouseLeave += (form_MouseLeave);
            form.MouseDown += (form_MouseDown);
            form.MouseUp += (form_MouseUp);
        }

        public void RegisterKeyEvents(Form form)
        {
            form.KeyDown += (form_KeyDown);
            form.KeyUp += (form_KeyUp);
        }

        void form_KeyUp(object sender, KeyEventArgs e)
        {
            e.Raise(sender, ref OnKeyUpEvent);
            
        }

        void form_KeyDown(object sender, KeyEventArgs e)
        {
            e.Raise(sender, ref OnKeyDownEvent);
        }

        void form_MouseUp(object sender, MouseEventArgs e)
        {
            e.Raise(sender, ref OnMouseUpEvent);
        }

        void form_MouseDown(object sender, MouseEventArgs e)
        {
            e.Raise(sender, ref OnMouseDownEvent);
        }

        void form_MouseLeave(object sender, EventArgs e)
        {
            e.Raise(sender, ref OnMouseLeaveEvent);
        }

        void form_MouseMove(object sender, MouseEventArgs e)
        {
            e.Raise(sender, ref OnMouseMoveEvent);
        }

        public event EventHandler<KeyEventArgs> OnKeyUpEvent;
        public event EventHandler<KeyEventArgs> OnKeyDownEvent;
        public event EventHandler<MouseEventArgs> OnMouseUpEvent;
        public event EventHandler<MouseEventArgs> OnMouseDownEvent;
        public event EventHandler<MouseEventArgs> OnMouseMoveEvent;
        public event EventHandler<EventArgs> OnMouseLeaveEvent;


    }
}
