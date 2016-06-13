using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DirectShowLib;
using Emgu.CV;
using GygaxCore;
using GygaxCore.Interfaces;

namespace GygaxCore.Devices
{
    public class UsbCamera : Streamable, ICamera
    {
        private readonly Capture _capture = null;

        private bool _stop;

        public UsbCamera(int cameraIndex)
        {
            _capture = new Capture(cameraIndex);

            var thread = new Thread(WorkThreadFunction);
            thread.Name = "UsbCamera " + cameraIndex;
            thread.Start();
        }

        public static List<string> GetDevices()
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            return devices.Select(t => t.Name).ToList();
        }

        public override void Close()
        {
            _stop = true;
        }

        private void WorkThreadFunction()
        {
            if (_capture == null) return;

            while (!_stop)
            {
                CvSource = _capture.QueryFrame();
            }

        }
    }
}
