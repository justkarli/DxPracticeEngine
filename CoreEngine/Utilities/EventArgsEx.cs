using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoreEngine.Utilities
{
    public static class EventArgsEx
    {
        public static void Raise<TEventArgs>(this TEventArgs e, 
            Object sender, ref EventHandler<TEventArgs> event_delegate) where TEventArgs : EventArgs
        {
            // for thread safety copy the delegate field into a temporary
            EventHandler<TEventArgs> temp = Interlocked.CompareExchange(ref event_delegate, null, null);

            if (temp != null) temp(sender, e);
        }
    }
}
