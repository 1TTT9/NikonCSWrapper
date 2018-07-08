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
// This demo shows how to enumerate and retrieve device capabilities
//

namespace demo_capabilities
{
    class DemoCapabilities
    {
        NikonDevice _device;
        AutoResetEvent _waitForDevice = new AutoResetEvent(false);

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

                // Get 'info' struct for each supported capability
                NkMAIDCapInfo[] caps = _device.GetCapabilityInfo();

                // Iterate through all supported capabilities
                foreach (NkMAIDCapInfo cap in caps)
                {
                    // Print ID, description and type
                    Console.WriteLine(string.Format("{0, -14}: {1}", "Id", cap.ulID.ToString()));
                    Console.WriteLine(string.Format("{0, -14}: {1}", "Description", cap.GetDescription()));
                    Console.WriteLine(string.Format("{0, -14}: {1}", "Type", cap.ulType.ToString()));

                    // Try to get the capability value
                    string value = null;

                    // First, check if the capability is readable
                    if (cap.CanGet())
                    {
                        // Choose which 'Get' function to use, depending on the type
                        switch (cap.ulType)
                        {
                            case eNkMAIDCapType.kNkMAIDCapType_Unsigned:
                                value = _device.GetUnsigned(cap.ulID).ToString();
                                break;

                            case eNkMAIDCapType.kNkMAIDCapType_Integer:
                                value = _device.GetInteger(cap.ulID).ToString();
                                break;

                            case eNkMAIDCapType.kNkMAIDCapType_String:
                                value = _device.GetString(cap.ulID);
                                break;

                            case eNkMAIDCapType.kNkMAIDCapType_Boolean:
                                value = _device.GetBoolean(cap.ulID).ToString();
                                break;

                            // Note: There are more types - adding the rest is left
                            //       as an exercise for the reader.
                        }
                    }

                    // Print the value
                    if (value != null)
                    {
                        Console.WriteLine(string.Format("{0, -14}: {1}", "Value", value));
                    }

                    // Print spacing between capabilities
                    Console.WriteLine();
                    Console.WriteLine();
                }

                // Shutdown
                manager.Shutdown();
            }
            catch (NikonException ex)
            {
                Console.WriteLine(ex.Message);
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
            DemoCapabilities demo = new DemoCapabilities();
            demo.Run();
        }
    }
}
