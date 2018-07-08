//
// This work is licensed under a Creative Commons Attribution 3.0 Unported License.
//
// Thomas Dideriksen (thomas@dideriksen.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Nikon;

//
// This demo shows how to capture an image and store it to disk
//

namespace demo_capture
{
    class DemoCapture
    {
        NikonDevice _device;
        AutoResetEvent _waitForDevice = new AutoResetEvent(false);
        AutoResetEvent _waitForCaptureComplete = new AutoResetEvent(false);

        public void Run()
        {
            // Create manager object - make sure you have the correct MD3 file for your Nikon DSLR (see https://sdk.nikonimaging.com/apply/)
            NikonManager manager = new NikonManager("Type0004.md3");
            try
            {
                // Listen for the 'DeviceAdded' event
                manager.DeviceAdded += manager_DeviceAdded;

                // Wait for a device to arrive
                _waitForDevice.WaitOne();

                // Hook up capture events
                _device.ImageReady += _device_ImageReady;
                _device.CaptureComplete += _device_CaptureComplete;

                // Capture
                _device.Capture();

                // Wait for the capture to complete
                _waitForCaptureComplete.WaitOne();

            }
            catch (NikonException ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Shutdown
            manager.Shutdown();
        }

        void _device_ImageReady(NikonDevice sender, NikonImage image)
        {
            string dts = DateTime.Now.ToString("HHmmss");
            string filename = string.Format("{0}_image{1}", dts, ((image.Type == NikonImageType.Jpeg) ? ".jpg" : ".nef"));
            Console.WriteLine("save {0}", filename);
            // Save captured image to disk
            //string filename = "image" + ((image.Type == NikonImageType.Jpeg) ? ".jpg" : ".nef");

            using (FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                s.Write(image.Buffer, 0, image.Buffer.Length);
            }
        }

        void _device_CaptureComplete(NikonDevice sender, int data)
        {
            Console.WriteLine("capture completed");
            // Signal the the capture completed
            _waitForCaptureComplete.Set();
        }

        void manager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
            Console.WriteLine("=> {0}, {1}",sender.Id, sender.Name); 

            if (_device == null)
            {
                // Save device
                _device = device;

                // Signal that we got a device
                _waitForDevice.Set();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            DemoCapture demo = new DemoCapture();
            demo.Run();
        }
    }
}
