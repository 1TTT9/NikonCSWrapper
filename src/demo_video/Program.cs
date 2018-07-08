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
// This demo shows how record video and save it to disk
//

namespace demo_video
{
    class DemoVideo
    {
        NikonDevice _device;
        AutoResetEvent _waitForDevice = new AutoResetEvent(false);
        AutoResetEvent _waitForVideoCompleted = new AutoResetEvent(false);
        FileStream _fileStream = null;

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

                // Hook up video events
                _device.VideoFragmentReady += _device_VideoFragmentReady;
                _device.VideoRecordingInterrupted += _device_VideoRecordingInterrupted;

                // Start video recording
                _device.LiveViewEnabled = true;
                _device.StartRecordVideo();

                // Record for a while...
                Thread.Sleep(TimeSpan.FromSeconds(4.0));

                // Stop video recording
                _device.StopRecordVideo();
                _device.LiveViewEnabled = false;

                // Wait for the video download to complete
                _waitForVideoCompleted.WaitOne();

                // Shutdown
                manager.Shutdown();
            }
            catch (NikonException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void _device_VideoRecordingInterrupted(NikonDevice sender, int error)
        {
            // Video recording was interrupted - signal that we're done
            _waitForVideoCompleted.Set();
        }

        void _device_VideoFragmentReady(NikonDevice sender, NikonVideoFragment fragment)
        {
            // Open the filestream when we receive the first video fragment
            if (fragment.IsFirst)
            {
                _fileStream = new FileStream(fragment.Filename, FileMode.Create, FileAccess.Write);
            }

            // Save video fragments to file
            _fileStream.Write(fragment.Buffer, 0, fragment.Buffer.Length);

            // When we recive the last fragment, close the file and signal that we're done
            if (fragment.IsLast)
            {
                _fileStream.Close();
                _waitForVideoCompleted.Set();
            }
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
            DemoVideo demo = new DemoVideo();
            demo.Run();
        }
    }
}
