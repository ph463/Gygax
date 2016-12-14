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
using GygaxCore.DataStructures;
using GygaxCore.Interfaces;
using HelixToolkit.Wpf.SharpDX.Core;

namespace GygaxCore.Devices
{
    public class UsbCamera : Streamable, ICamera
    {
        private Capture _capture = null;
        
        private bool _stop;

        private static List<int> _openCameras;
        private int _cameraIndex;

        public UsbCamera(int cameraIndex)
        {
            if (_openCameras == null)
                _openCameras = new ExposedArrayList<int>();

            _cameraIndex = cameraIndex;

            _openCameras.Add(_cameraIndex);

            _capture = new Capture(cameraIndex);

            var thread = new Thread(WorkThreadFunction);

            Location = "UsbCamera " + cameraIndex;
            Name = Location;

            thread.Name = Location;
            thread.Start();
        }

        public static bool IsCameraOpened(int cameraIndex)
        {
            if (_openCameras != null && _openCameras.Contains(cameraIndex))
                return true;

            return false;
        }

        public static List<string> GetDevices()
        {
            var devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            return devices.Select(t => t.Name).ToList();
        }

        public override void Close()
        {
            base.Close();
            _stop = true;
        }

        private void WorkThreadFunction()
        {
            if (_capture == null) return;

            while (!_stop)
            {
                try
                {
                    CvSource = _capture.QueryFrame();
                }
                catch (Exception)
                {
                }
            }
            
             _capture.Dispose();

            _openCameras.Remove(_cameraIndex);

        }
    }
}
