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
// This demo shows how to control manual focus
//

namespace demo_manualfocus
{
    class DemoManualFocus
    {
        NikonDevice _device;
        AutoResetEvent _waitForDevice = new AutoResetEvent(false);
        AutoResetEvent _waitForCaptureComplete = new AutoResetEvent(false);

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

                // Enable live view (required for manual focus)
                _device.LiveViewEnabled = true;

                // Get the current manual focus 'drive step'
                NikonRange driveStep = _device.GetRange(eNkMAIDCapability.kNkMAIDCapability_MFDriveStep);

                // Set the drive step to max
                driveStep.Value = driveStep.Max;
                _device.SetRange(eNkMAIDCapability.kNkMAIDCapability_MFDriveStep, driveStep);

                // Drive all the way to 'closest'
                DriveManualFocus(eNkMAIDMFDrive.kNkMAIDMFDrive_InfinityToClosest);

                // Set the drive step to something small
                driveStep.Value = 200.0;
                _device.SetRange(eNkMAIDCapability.kNkMAIDCapability_MFDriveStep, driveStep);

                // Drive manual focus towards infinity in small steps
                for (int i = 0; i < 10; i++)
                {
                    DriveManualFocus(eNkMAIDMFDrive.kNkMAIDMFDrive_ClosestToInfinity);
                }

                // Disable live view
                _device.LiveViewEnabled = false;

                // Shutdown
                manager.Shutdown();
            }
            catch (NikonException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void DriveManualFocus(eNkMAIDMFDrive direction)
        {
            // Start driving the manual focus motor
            _device.SetUnsigned(eNkMAIDCapability.kNkMAIDCapability_MFDrive, (uint)direction);

            // Keep looping here until drive is complete
            bool isDriving;

            do
            {
                // Get the most recent live view image
                NikonLiveViewImage image = _device.GetLiveViewImage();

                // NOTE: For Type0003.md3, the drive state flag is located at index 30 - this might
                //       not be the case for other MD3 files. Please double check your SDK documentation.
                const int driveStateIndex = 30;

                isDriving = (image.HeaderBuffer[driveStateIndex] > 0);
            }
            while (isDriving);
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
            DemoManualFocus demo = new DemoManualFocus();
            demo.Run();
        }
    }
}
