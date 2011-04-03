using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DxApplication
{
    public interface IRenderEngine
    {
        void OnDeviceCreated(object sender, EventArgs e);
        void OnDeviceDestroyed(object sender, EventArgs e);
        void OnDeviceLost(object sender, EventArgs e);
        void OnDeviceReset(object sender, EventArgs e);
        void OnMainLoop(object sender, EventArgs e);
    }
}
