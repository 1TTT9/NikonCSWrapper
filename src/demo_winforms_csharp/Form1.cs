//
// This work is licensed under a Creative Commons Attribution 3.0 Unported License.
//
// Thomas Dideriksen (thomas@dideriksen.com)
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Nikon;
using System.Net.NetworkInformation;

namespace demo_winforms_csharp
{
    public partial class Form1 : Form
    {
        private NikonManager manager;
        private Dictionary<int, NikonDevice> devices;
        //private Timer liveViewTimer;

        private int DeviceCounter = 0;
        //AutoResetEvent _waitForCaptureComplete = new AutoResetEvent(false);

        public Form1()
        {
            InitializeComponent();

            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
            Console.WriteLine("{0}_{1}", username, macAddr);


            devices = new Dictionary<int, NikonDevice>() { { 1, null }, { 2, null } };

            // Disable buttons
            ToggleButtons(false);

            // Initialize live view timer
            //liveViewTimer = new Timer();
            //liveViewTimer.Tick += new EventHandler(liveViewTimer_Tick);
            //liveViewTimer.Interval = 1000 / 30;

            // Initialize Nikon manager
            manager = new NikonManager("Type0004.md3");
            manager.DeviceAdded += new DeviceAddedDelegate(manager_DeviceAdded);
            manager.DeviceRemoved += new DeviceRemovedDelegate(manager_DeviceRemoved);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            /*
            // Disable live view (in case it's enabled)
            if (devices.Count>0)
            {
                foreach(NikonDevice device in devices.Values)
                {
                    if (device == null) continue;

                    device.LiveViewEnabled = false;
                }
            }
            */
            devices[1] = null;
            devices[2] = null;
            // Shut down the Nikon manager
            manager.Shutdown();

            base.OnClosing(e);

        }

        void manager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
            DeviceCounter += 1;
            device.DeviceID = DeviceCounter;
            devices[DeviceCounter] = device;
            //this.device = device;
        
            // Set the device name
            label_name.Text = device.Name;

            // Enable buttons
            ToggleButtons(true);

            // Hook up device capture events
            device.ImageReady += new ImageReadyDelegate(device_ImageReady);
            device.CaptureComplete += new CaptureCompleteDelegate(device_CaptureComplete);


            Console.WriteLine("{0} - >", devices[DeviceCounter].DeviceID);
        }

        void manager_DeviceRemoved(NikonManager sender, NikonDevice device)
        {
            devices[device.DeviceID] = null;
            //this.device = null;

            // Stop live view timer
            //liveViewTimer.Stop();

            // Clear device name
            label_name.Text = "üoœ‡ôC";

            // Disable buttons
            ToggleButtons(false);

            // Clear live view picture
            pictureBox.Image = null;
        }

        /*
        void liveViewTimer_Tick(object sender, EventArgs e)
        {
            // Get live view image
            NikonLiveViewImage image = null;

            try
            {
                image = device.GetLiveViewImage();
            }
            catch (NikonException)
            {
                liveViewTimer.Stop();
            }

            // Set live view image on picture box
            if (image != null)
            {
                MemoryStream stream = new MemoryStream(image.JpegBuffer);
                pictureBox.Image = Image.FromStream(stream);
            }
        }
        */

        void device_ImageReady(NikonDevice sender, NikonImage image)
        {
            string image2save = string.Format("{0}_{1}.jpg", dt, sender.DeviceID);

            Console.WriteLine("save ... {0}", image2save);

            string imageFolder = tbFilePath.Text;

            using (FileStream stream = new FileStream(Path.Combine(imageFolder, image2save), FileMode.Create, FileAccess.Write))
            {
                stream.Write(image.Buffer, 0, image.Buffer.Length);
            }

            string fileImg1 = Path.Combine(imageFolder, string.Format("{0}_{1}.jpg", dt, 1)),
                fileImg2 = Path.Combine(imageFolder, string.Format("{0}_{1}.jpg", dt, 2)),
                fileImg3 = Path.Combine(imageFolder, string.Format("{0}_both.jpg", dt));

            tbOutput.Text = fileImg3;

            Image img1 = null, img2 = null;
            Bitmap img3;
            if (File.Exists(fileImg1) && File.Exists(fileImg2))
            {
                 img1 = Image.FromFile(fileImg1);
                 img2 = Image.FromFile(fileImg2);


            }
            else if (File.Exists(fileImg1))
            {
                 img1 = Image.FromFile(fileImg1);
                 img2 = img1;

            }
            else if (File.Exists(fileImg2))
            {
                 img2 = Image.FromFile(fileImg2);
                 img1 = img2;
            }

            if (File.Exists(fileImg1) || File.Exists(fileImg2))
            {
                int width = img1.Width + img2.Width;
                int height = Math.Max(img1.Height, img2.Height);

                Console.WriteLine("{0},{1}, {2},{3}", img1.Width, img1.Height, width, height);

                img3 = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(img3);

                g.Clear(Color.Yellow);
                g.DrawImage(img1, 0, 0, img1.Width, img1.Height);
                g.DrawImage(img2, img1.Width, 0, img2.Width, img2.Height);

                g.Dispose();
                img1.Dispose();
                img2.Dispose();

                img3.Save(fileImg3, System.Drawing.Imaging.ImageFormat.Jpeg);
                //img3.Dispose();
                pictureBox.Image = img3;
            }


            // Signal the the capture completed
            //_waitForCaptureComplete.Set();

            /*
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = (image.Type == NikonImageType.Jpeg) ?
                "Jpeg Image (*.jpg)|*.jpg" :
                "Nikon NEF (*.nef)|*.nef";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (FileStream stream = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(image.Buffer, 0, image.Buffer.Length);
                }
            }
            */
        }


        void device_CaptureComplete(NikonDevice sender, int data)
        {
            // Re-enable buttons when the capture completes
            ToggleButtons(true);
        }

        void ToggleButtons(bool enabled)
        {
            this.button_capture.Enabled = enabled;
        }

        string dt = "OFF";
        private void button_capture_Click(object sender, EventArgs e)
        {

            ToggleButtons(false);

            dt = DateTime.Now.ToString("yyyyMMddHHmmss");
            try
            {
                foreach(NikonDevice device in devices.Values)
                {
                    if (device == null)
                        continue;

                    device.Capture();
                    // Wait for the capture to complete
                    //_waitForCaptureComplete.WaitOne();
                }

            }
            catch (NikonException ex)
            {
                MessageBox.Show(ex.Message);
                ToggleButtons(true);
            }

            pictureBox.Image = null;
        }

        /*
        private void button_toggleliveview_Click(object sender, EventArgs e)
        {
            if (device == null)
            {
                return;
            }
            if (device.LiveViewEnabled)
            {
                device.LiveViewEnabled = false;
                liveViewTimer.Stop();
                pictureBox.Image = null;
            }
            else
            {
                device.LiveViewEnabled = true;
                liveViewTimer.Start();
            }
        }
        */
    }
}
