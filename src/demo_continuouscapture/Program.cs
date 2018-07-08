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
// This demo shows how to perform continuous capture and store the images to disk
//

namespace demo_continuouscapture
{
    class DemoContinuousCapture
    {
        NikonDevice _device;
        AutoResetEvent _waitForDevice = new AutoResetEvent(false);
        AutoResetEvent _waitForCaptureComplete = new AutoResetEvent(false);
        int _captureCount = 0;

        public void Run()
        {
            try
            {
                // Create manager object - make sure you have the correct MD3 file for your Nikon DSLR (see https://sdk.nikonimaging.com/apply/)
                NikonManager manager = new NikonManager("Type0003.md3");

                // Listen for the 'DeviceAdded' event
                manager.DeviceAdded += manager_DeviceAdded;

                // Wait for a device to arrive
                _waitForDevice.WaitOne();

                // Set shooting mode to 'continuous, highspeed'
                NikonEnum shootingMode = _device.GetEnum(eNkMAIDCapability.kNkMAIDCapability_ShootingMode);
                shootingMode.Index = (int)eNkMAIDShootingMode.kNkMAIDShootingMode_CH;
                _device.SetEnum(eNkMAIDCapability.kNkMAIDCapability_ShootingMode, shootingMode);

                // Set number of continuous captures - in this case we want 5
                _device.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_ContinuousShootingNum, 5);

                // Hook up capture events
                _device.ImageReady += _device_ImageReady;
                _device.CaptureComplete += _device_CaptureComplete;

                // Capture
                _device.Capture();

                // Wait for the capture to complete
                _waitForCaptureComplete.WaitOne();

                // Shutdown
                manager.Shutdown();
            }
            catch (NikonException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void _device_ImageReady(NikonDevice sender, NikonImage image)
        {
            // Save captured image to disk
            string filename = "image" + _captureCount.ToString() + ((image.Type == NikonImageType.Jpeg) ? ".jpg" : ".nef");
            _captureCount++;

            using (FileStream s = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                s.Write(image.Buffer, 0, image.Buffer.Length);
            }
        }

        void _device_CaptureComplete(NikonDevice sender, int data)
        {
            // Signal the the capture completed
            _waitForCaptureComplete.Set();
        }

        void manager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
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
            DemoContinuousCapture demo = new DemoContinuousCapture();
            demo.Run();
        }
    }
}
