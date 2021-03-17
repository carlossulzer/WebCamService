using System;
using System.Collections.Generic;
using System.Text;
using WIALib;
using WIAVIDEOLib;
using System.Runtime.InteropServices;
namespace WebCamLibrary
{
    /// <summary>
    /// This Class will initialize the camera using Windows Image Acquisition (WIA).
    /// Also, provide Methods to view all connected cameras, capture single frame.
    /// </summary>
    public class WebCam
    {
        private static WebCam WebCamObj;
       /// <summary>
       ///  variables needed 
       /// </summary>
        private WIAVIDEOLib.WiaVideo WiaVideoObj;
        private WIALib.Wia WiaObj;
        private WIALib.DeviceInfo DefaultDevice;
        private WIALib.DeviceInfo[] Devices; 
        private bool DeviceIsOpen;
        /// <summary>
        /// Initialize The WebCam Class
        /// </summary>
        private WebCam()
        {
            DeviceIsOpen = false;
            Initialize();
            SetDefaultDevice();
            OpenDefaultDevice();
        }
        /// <summary>
        /// Please use this method to create a WebCam Object.
        /// </summary>
        /// <returns>WebCam Object</returns>
        public static WebCam NewWebCam()
        {
            if(WebCam.WebCamObj == null)
                WebCam.WebCamObj = new WebCam();

            return WebCam.WebCamObj;
            
        }
        private void Initialize()
        {
            if (this.WiaObj == null)
            {
                this.WiaObj = new WiaClass();
            }
            if (this.WiaVideoObj == null)
            {
                this.WiaVideoObj = new WiaVideoClass();
                this.WiaVideoObj.ImagesDirectory = System.IO.Path.GetTempPath();
            }        
        }
        /// <summary>
        /// Use to it to close any open resources.
        /// </summary>
        public void CloseResources()
        {
            try
            {
                if(WiaObj != null)
                    Marshal.FinalReleaseComObject(WiaObj);
                if(WiaVideoObj != null)
                    Marshal.FinalReleaseComObject(WiaVideoObj);
                if (DefaultDevice != null)
                    Marshal.FinalReleaseComObject(DefaultDevice);
                if (Devices != null)
                    Marshal.FinalReleaseComObject(Devices);

                DeviceIsOpen = false;
                GC.Collect();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

        }
        private void SetDefaultDevice()
        {
            CloseDefaultDevice();
                
            System.Collections.Generic.List<WIALib.DeviceInfoClass> devs = new List<DeviceInfoClass>();
            WIALib.Collection devsCol = this.WiaObj.Devices as WIALib.CollectionClass;
            foreach (object obj in devsCol)
            {
                WIALib.DeviceInfoClass dev = (WIALib.DeviceInfoClass)System.Runtime.InteropServices.Marshal.CreateWrapperOfType(obj, typeof(WIALib.DeviceInfoClass));
                if (dev.Type.Contains("Video") == true)
                {
                    devs.Add(dev);
                }
                else
                {
                    Marshal.FinalReleaseComObject(obj);
                    Marshal.FinalReleaseComObject(dev);
                }
            }
            Devices = devs.ToArray();
            if (Devices.Length > 0)
            {
                DefaultDevice = Devices[0];
            }
            else
            {
                throw new Exception("No Cameras Detected");
            }

            
            Marshal.FinalReleaseComObject(devsCol);
            GC.ReRegisterForFinalize(devs);          
        }
        /// <summary>
        /// Get Connected Cameras
        /// </summary>
        /// <returns>string[] contains names of connected cameras</returns>
        public string[] GetConnectedCameras()
        {
            string[] devs = new string[this.Devices.Length];
            for (int i = 0; i < devs.Length; i++)
            {
                devs[i] = this.Devices[i].Name;
            }
            return devs;
        }
        private void CloseDefaultDevice()
        {
            if (this.DeviceIsOpen == false)
                return;
            try
            {
                    if (this.WiaVideoObj != null)
                    {
                        this.WiaVideoObj.DestroyVideo();
                        Marshal.FinalReleaseComObject(this.DefaultDevice);
                        DeviceIsOpen = false;
                    }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            finally
            {
                DeviceIsOpen = false;
            }


        }
        /// <summary>
        /// use this method to set the camera to capture 
        /// from if you have more than one camera.
        /// </summary>
        /// <param name="name">Name of the camera device. Get name from GetConnectedCameras() </param>
        /// <returns></returns>
        public bool SetDefaultDevice(string name)
        {
            CloseDefaultDevice();

            for (int i = 0; i < Devices.Length; i++)
            {
                if (Devices[i].Name.Equals(name))
                {
                    DefaultDevice = Devices[i];
                    return true;
                }
            }
            return false;
        }
        private bool OpenDefaultDevice()
        {
            CloseDefaultDevice();

            try
            {
                this.WiaVideoObj.PreviewVisible = 0;
                this.WiaVideoObj.CreateVideoByWiaDevID(this.DefaultDevice.Id, IntPtr.Zero, 0, 0);
                System.Threading.Thread.CurrentThread.Join(3000);
                this.DeviceIsOpen = true;
                this.WiaVideoObj.Play();
                System.Threading.Thread.CurrentThread.Join(3000);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                CloseDefaultDevice();
                return false;
            }

            return true;
        }
        /// <summary>
        /// Use it to grab a frame. Use System.IO.MemoryStream to load image
        /// </summary>
        /// <returns>byte array of the captured image</returns>
        public byte[] GrabFrame()
        {
            string imagefile;
            this.WiaVideoObj.TakePicture(out imagefile);
            return ReadImageFile(imagefile);
        }
        private byte[] ReadImageFile(string img)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(img);
            byte[] buf = new byte[fileInfo.Length];
            System.IO.FileStream fs = new System.IO.FileStream(img, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            fs.Read(buf, 0, buf.Length);
            fs.Close();
            fileInfo.Delete();
            GC.ReRegisterForFinalize(fileInfo);
            GC.ReRegisterForFinalize(fs);
            return buf;
        }
    }
}
