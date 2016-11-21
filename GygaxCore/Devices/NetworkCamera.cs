using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using GygaxCore.DataStructures;
using GygaxCore.Interfaces;
using NLog;

namespace GygaxCore.Devices
{
    public class NetworkCamera : Streamable, ICamera
    {
        private bool _stop;
        private String camIp = "http://";
        private readonly Thread _thread;

        private readonly Stopwatch sw = new Stopwatch();

        private double _framerate;
        public double Framerate
        {
            get { return _framerate; }
            set
            {
                _framerate = value;
                OnPropertyChanged(nameof(Framerate));
            }
        }

        public NetworkCamera(String Ip)
        {
            LogManager.GetCurrentClassLogger().Info("Opening network camera");

            camIp += Ip;
            _thread = new Thread(new ThreadStart(WorkThreadFunction));

            _thread.Name = "NetworkCamera " + Ip;

            _thread.Start();
        }

        private void WorkThreadFunction()
        {
            var i = 0;

            sw.Start();

            while (true)
            {
                Get_Frame();
                i++;

                if (sw.ElapsedMilliseconds > 1000)
                {
                    Framerate = i;

                    sw.Restart();

                    i = 0;
                }
            }
        }

        public static bool SetAllowUnsafeHeaderParsing20()
        {
            //Get the assembly that contains the internal class
            Assembly aNetAssembly = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
            if (aNetAssembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type aSettingsType = aNetAssembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (aSettingsType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created allready the property will create it for us.
                    object anInstance = aSettingsType.InvokeMember("Section",
                        BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });

                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework is unsafe header parsing should be allowed or not
                        FieldInfo aUseUnsafeHeaderParsing = aSettingsType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Get_Frame()
        {
            String CameraIPAddr = camIp; //Use a common string...Do not hardcode it
            String Cam_Frame = CameraIPAddr + "/cgi-bin/nph-" + "image.jpg";
            HttpWebRequest HttpWReq = (HttpWebRequest)WebRequest.Create(Cam_Frame);
            HttpWReq.KeepAlive = false;
            HttpWReq.ProtocolVersion = HttpVersion.Version10;
            SetAllowUnsafeHeaderParsing20();

            try
            {
                HttpWebResponse HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
                HttpStatusCode TStat = HttpWResp.StatusCode;
                //  StreamReader sr = ExtractResponse(HttpWResp);
                //  ExtractResponseSave(HttpWResp);
                // sr.ReadToEnd();
                System.IO.Stream receiveStream = HttpWResp.GetResponseStream();
                System.Drawing.Image image = (System.Drawing.Image.FromStream(receiveStream));
                Bitmap bmp = (Bitmap)image;

                CvSource = new Image<Bgr, Byte>(bmp);

                //********************************************************************************************************************
                DateTime dt = DateTime.Now;

                int milliseconds = dt.Millisecond;
                int seconds = dt.Second;
                int minutes = dt.Minute;
                int hours = dt.Hour;
                //String s = Date_Frame();
                //MessageBox.Show(((minutes*60000)+(seconds*1000)+milliseconds).ToString());
                //timeStamp = (minutes * 60000) + (seconds * 1000) + milliseconds;
                //**********************************************************************************************************************
                
                HttpWResp.Close();


            }
            catch (Exception e1)
            {
                Console.WriteLine("Error: {0}", e1.ToString());
                LogManager.GetCurrentClassLogger().Warn(e1,"Can't open network camera");
            }
        }

        public override void Close()
        {
            _stop = true;
        }
    }
}
