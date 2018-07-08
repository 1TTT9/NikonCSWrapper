//
// This work is licensed under a Creative Commons Attribution 3.0 Unported License.
//
// Thomas Dideriksen (thomas@dideriksen.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading;
using Nikon;

namespace testapp
{
    // Log
    public delegate void LogChangedDelegate();

    class Log
    {
        string _logText;
        static Log _instance;

        public event LogChangedDelegate LogChanged;

        public static Log GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Log();
            }

            return _instance;
        }

        public void WriteLine(string message)
        {
            _logText += message + "\n";
            OnLogChanged();
        }

        protected virtual void OnLogChanged()
        {
            if (LogChanged != null)
            {
                LogChanged();
            }
        }

        private Log()
        {
            _logText = "";
        }

        public void Clear()
        {
            _logText = "";
            OnLogChanged();
        }

        public string LogText
        {
            get { return _logText; }
        }
    }

    // View model base class
    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    // Log view model
    class LogModel : ViewModelBase
    {
        Log _log;

        public LogModel()
        {
            _log = Log.GetInstance();
            _log.LogChanged += new LogChangedDelegate(_log_LogChanged);
        }

        void _log_LogChanged()
        {
            OnPropertyChanged("LogText");
        }

        public string LogText
        {
            get { return _log.LogText; }
        }

        public void ClearLog()
        {
            _log.Clear();
        }
    }

    // Capability view model
    class CapModel : ViewModelBase
    {
        NikonBase _object;
        NkMAIDCapInfo _cap;

        public CapModel(NikonBase obj, NkMAIDCapInfo cap)
        {
            _object = obj;
            _cap = cap;

            NikonDevice device = _object as NikonDevice;

            if (device != null)
            {
                device.CapabilityValueChanged += new CapabilityChangedDelegate(device_CapabilityValueChanged);
            }
        }

        void device_CapabilityValueChanged(NikonDevice sender, eNkMAIDCapability capability)
        {
            if (_cap.ulID == capability)
            {
                OnPropertyChanged("CapValue");

                switch (_cap.ulType)
                {
                    case eNkMAIDCapType.kNkMAIDCapType_Enum:
                        OnPropertyChanged("EnumSelectedIndex");
                        break;

                    case eNkMAIDCapType.kNkMAIDCapType_Range:
                        OnPropertyChanged("RangeValue");
                        break;
                }
            }
        }

        public object CapValue
        {
            get
            {
                if (!_cap.CanGet())
                {
                    return null;
                }

                try
                {
                    return _object.Get(_cap.ulID);
                }
                catch (NikonException ex)
                {
                    Log.GetInstance().WriteLine("Failed to get " + _cap.ulID.ToString() + ", " + ex.ToString());
                    return null;
                }
            }
            set
            {
                if (!_cap.CanSet())
                {
                    return;
                }

                try
                {
                    _object.Set(_cap.ulID, value);
                    OnPropertyChanged("CapValue");
                }
                catch (NikonException ex)
                {
                    Log.GetInstance().WriteLine("Failed to set " + _cap.ulID.ToString() + " to " + value.ToString() + ", " + ex.ToString());
                }
            }
        }

        public double RangeValue
        {
            get
            {
                NikonRange r = CapValue as NikonRange;
                return r.Value;
            }

            set
            {
                NikonRange r = CapValue as NikonRange;
                r.Value = value;
                CapValue = r;
                OnPropertyChanged("RangeValue");
            }
        }

        public double RangeMin
        {
            get
            {
                NikonRange r = CapValue as NikonRange;
                return r.Min;
            }
        }

        public double RangeMax
        {
            get
            {
                NikonRange r = CapValue as NikonRange;
                return r.Max;
            }
        }

        public int EnumSelectedIndex
        {
            get
            {
                NikonEnum e = CapValue as NikonEnum;

                if (e != null)
                {
                    return e.Index;
                }
                else
                {
                    return -1;
                }
            }

            set
            {
                NikonEnum e = CapValue as NikonEnum;

                if (e != null && value >= 0 && value < e.Length)
                {
                    e.Index = value;
                    CapValue = e;
                }
            }
        }

        public object[] EnumValues
        {
            get
            {
                NikonEnum e = CapValue as NikonEnum;

                if (e != null)
                {
                    object[] result = new object[e.Length];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = e[i];
                    }

                    return result;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool CanSet
        {
            get { return _cap.CanSet(); }
        }

        public bool CanStart
        {
            get { return _cap.CanStart(); }
        }

        public eNkMAIDCapType CapType
        {
            get { return _cap.ulType; }
        }

        public string CapTypeFriendlyName
        {
            get
            {
                string result = _cap.ulType.ToString();

                if (result.StartsWith("kNkMAIDCapType_"))
                {
                    result = result.Substring(15);
                }

                return result;
            }
        }

        public string CapId
        {
            get
            {
                string result = _cap.ulID.ToString();

                if (result.StartsWith("kNkMAIDCapability_"))
                {
                    result = result.Substring(18);
                }

                return result;
            }
        }

        public int CapNumericalId
        {
            get
            {
                return (int)_cap.ulID;
            }
        }

        public string CapTitle
        {
            get { return _cap.GetDescription(); }
        }

        public void Start()
        {
            try
            {
                _object.Start(_cap.ulID);
            }
            catch (NikonException ex)
            {
                Log.GetInstance().WriteLine("Failed to start " + _cap.ulID.ToString() + ", " + ex.ToString());
            }
        }
    }

    // View model for NikonDevice and NikonManager (classes that inherit from NikonBase)
    class ObjectModel : ViewModelBase
    {
        NikonBase _object;
        ObservableCollection<CapModel> _caps;
        DispatcherTimer _timer;
        BitmapSource _liveViewImage;
        FileStream _videoFile;
        bool _doThumbnail;
        bool _doPreview;
        bool _doLowResPreview;

        public ObjectModel(NikonBase obj)
        {
            _caps = new ObservableCollection<CapModel>();
            _object = obj;
            _doThumbnail = false;
            _doPreview = false;
            _doLowResPreview = false;
            _videoFile = null;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(33.0);
            _timer.Tick += new EventHandler(_timer_Tick);

            NikonDevice device = _object as NikonDevice;

            if (device != null)
            {
                device.CapabilityChanged += new CapabilityChangedDelegate(device_CapabilityChanged);

                device.CaptureComplete += new CaptureCompleteDelegate(device_CaptureComplete);
                device.ImageReady += new ImageReadyDelegate(device_ImageReady);

                // Note: Disable thumbnails and previews by default

                //device.PreviewReady += device_PreviewReady;
                //device.LowResolutionPreviewReady += device_LowResolutionPreviewReady;
                //device.ThumbnailReady += device_ThumbnailReady;
                //_doPreview = true;
                //_doLowResPreview = true;
                //_doThumbnail = true;

                device.VideoFragmentReady += new VideoFragmentReadyDelegate(device_VideoFragmentReady);
                device.VideoRecordingInterrupted += new VideoRecordingInterruptedDelegate(device_VideoRecordingInterrupted);
            }

            RefreshCaps();
        }

        void device_VideoRecordingInterrupted(NikonDevice sender, int error)
        {
            Log.GetInstance().WriteLine("Video Recording Interrupted (" + error.ToString() + ")");
        }

        void Save(byte[] buffer, string file)
        {
            string path = Path.Combine(
                System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                file);

            Log.GetInstance().WriteLine("Saving: " + path);

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Log.GetInstance().WriteLine("Failed to save file: " + path + ", " + ex.Message);
            }
        }

        void device_VideoFragmentReady(NikonDevice sender, NikonVideoFragment fragment)
        {
            string path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), fragment.Filename);

            if (fragment.IsFirst)
            {
                Debug.Assert(_videoFile == null);
                _videoFile = new FileStream(path, FileMode.Create, FileAccess.Write);
                Log.GetInstance().WriteLine("Downloading Video...");
            }

            Log.GetInstance().WriteLine(fragment.PercentComplete.ToString(".0") + "%");

            Debug.Assert(_videoFile != null);
            _videoFile.Write(fragment.Buffer, 0, fragment.Buffer.Length);

            if (fragment.IsLast)
            {
                _videoFile.Close();
                _videoFile = null;
                Log.GetInstance().WriteLine("Saved Video: " + path + " (" + fragment.VideoWidth + "x" + fragment.VideoHeight + ")");
            }
        }

        void device_ThumbnailReady(NikonDevice sender, NikonThumbnail thumbnail)
        {
            // We're expecting to get RGB24 data, make sure that the format is correct
            if (thumbnail.Stride / thumbnail.Width < 3)
            {
                return;
            }

            // Note: Thumbnail pixels are uncompressed RGB24 - here's an example of how to jpeg encode it

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 90;

            BitmapSource source = BitmapFrame.Create(
                thumbnail.Width,
                thumbnail.Height,
                96.0,
                96.0,
                System.Windows.Media.PixelFormats.Rgb24,
                null,
                thumbnail.Pixels,
                thumbnail.Stride);

            BitmapFrame frame = BitmapFrame.Create(source);
            encoder.Frames.Add(frame);

            MemoryStream stream = new MemoryStream();
            encoder.Save(stream);

            Save(stream.GetBuffer(), "thumbnail.jpg");
        }

        void device_LowResolutionPreviewReady(NikonDevice sender, NikonPreview preview)
        {
            Save(preview.JpegBuffer, "preview_lowres.jpg");
        }

        void device_PreviewReady(NikonDevice sender, NikonPreview preview)
        {
            Save(preview.JpegBuffer, "preview.jpg");
        }

        void device_ImageReady(NikonDevice sender, NikonImage image)
        {
            Save(image.Buffer, "image" + ((image.Type == NikonImageType.Jpeg) ? ".jpg" : ".nef"));
        }

        void device_CaptureComplete(NikonDevice sender, int data)
        {
        }

        void device_CapabilityChanged(NikonDevice sender, eNkMAIDCapability capability)
        {
            RefreshCaps();
        }

        void RefreshCaps()
        {
            _caps.Clear();

            NkMAIDCapInfo[] caps = _object.GetCapabilityInfo();

            foreach (NkMAIDCapInfo cap in caps)
            {
                _caps.Add(new CapModel(_object, cap));
            }
        }

        public NikonBase Object
        {
            get { return _object; }
        }

        public string ObjectName
        {
            get { return _object.Name; }
        }

        public ObservableCollection<CapModel> Caps
        {
            get { return _caps; }
        }

        public bool PreviewReadyIsAttached
        {
            get { return _doPreview; }
            set
            {
                NikonDevice device = _object as NikonDevice;

                Debug.Assert(value != _doPreview);
                Debug.Assert(device != null);

                if (value)
                {
                    device.PreviewReady += device_PreviewReady;
                    _doPreview = true;
                }
                else
                {
                    device.PreviewReady -= device_PreviewReady;
                    _doPreview = false;
                }

                OnPropertyChanged("PreviewReadyIsAttached");
            }
        }

        public bool LowResolutionPreviewReadyIsAttached
        {
            get { return _doLowResPreview; }
            set
            {
                NikonDevice device = _object as NikonDevice;

                Debug.Assert(value != _doLowResPreview);
                Debug.Assert(device != null);

                if (value)
                {
                    device.LowResolutionPreviewReady += device_LowResolutionPreviewReady;
                    _doLowResPreview = true;
                }
                else
                {
                    device.LowResolutionPreviewReady -= device_LowResolutionPreviewReady;
                    _doLowResPreview = false;
                }

                OnPropertyChanged("LowResolutionPreviewReadyIsAttached");
            }
        }

        public bool ThumbnailReadyIsAttached
        {
            get { return _doThumbnail; }
            set
            {
                NikonDevice device = _object as NikonDevice;

                Debug.Assert(value != _doThumbnail);
                Debug.Assert(device != null);

                if (value)
                {
                    device.ThumbnailReady += device_ThumbnailReady;
                    _doThumbnail = true;
                }
                else
                {
                    device.ThumbnailReady -= device_ThumbnailReady;
                    _doThumbnail = false;
                }

                OnPropertyChanged("ThumbnailReadyIsAttached");
            }
        }

        public bool SupportsCapture
        {
            get { return _object.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_Capture); }
        }

        public bool SupportsPreview
        {
            get { return _object.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_GetPreviewImageNormal); }
        }

        public bool SupportsLowResPreview
        {
            get { return _object.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_GetPreviewImageLow); }
        }

        public bool SupportsLiveView
        {
            get { return _object.SupportsCapability(eNkMAIDCapability.kNkMAIDCapability_LiveViewStatus); }
        }

        public void StartLiveView()
        {
            try
            {
                NikonDevice device = _object as NikonDevice;

                if (device != null)
                {
                    device.LiveViewEnabled = true;
                    _timer.Start();
                }
            }
            catch (NikonException ex)
            {
                Log.GetInstance().WriteLine("Failed to start live view: " + ex.ToString());
            }
        }

        public void StopLiveView()
        {
            try
            {
                NikonDevice device = _object as NikonDevice;

                if (device != null)
                {
                    _timer.Stop();
                    device.LiveViewEnabled = false;

                    SetLiveViewImage(null);
                }
            }
            catch (NikonException ex)
            {
                Log.GetInstance().WriteLine("Failed to stop live view: " + ex.ToString());
            }
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            NikonDevice device = _object as NikonDevice;
            Debug.Assert(device != null);

            NikonLiveViewImage liveViewImage = null;

            try
            {
                liveViewImage = device.GetLiveViewImage();
            }
            catch (NikonException ex)
            {
                Log.GetInstance().WriteLine("Failed to get live view image: " + ex.ToString());
            }

            if (liveViewImage == null)
            {
                _timer.Stop();
                return;
            }

            // Note: Decode the live view jpeg image on a seperate thread to keep the UI responsive

            ThreadPool.QueueUserWorkItem(new WaitCallback((o) =>
            {
                Debug.Assert(liveViewImage != null);

                JpegBitmapDecoder decoder = new JpegBitmapDecoder(
                    new MemoryStream(liveViewImage.JpegBuffer),
                    BitmapCreateOptions.None,
                    BitmapCacheOption.OnLoad);

                Debug.Assert(decoder.Frames.Count > 0);
                BitmapFrame frame = decoder.Frames[0];

                Dispatcher.CurrentDispatcher.Invoke((Action)(() =>
                {
                    SetLiveViewImage(frame);
                }));
            }));
        }

        void SetLiveViewImage(BitmapSource image)
        {
            _liveViewImage = image;
            OnPropertyChanged("LiveViewImage");
        }

        public BitmapSource LiveViewImage
        {
            get { return _liveViewImage; }
        }
    }

    // Main view model
    class Model : ViewModelBase
    {
        List<NikonManager> _managers;
        ObservableCollection<ObjectModel> _objects;

        public Model()
        {
            _objects = new ObservableCollection<ObjectModel>();
            _managers = new List<NikonManager>();

            string[] md3s = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.md3", SearchOption.AllDirectories);

            if (md3s.Length == 0)
            {
                Log.GetInstance().WriteLine("Couldn't find any MD3 files in " + Directory.GetCurrentDirectory());
                Log.GetInstance().WriteLine("Download MD3 files from Nikons SDK website: https://sdk.nikonimaging.com/apply/");
            }

            foreach (string md3 in md3s)
            {
                const string requiredDllFile = "NkdPTP.dll";

                string requiredDllPath = Path.Combine(Path.GetDirectoryName(md3), requiredDllFile);

                if (!File.Exists(requiredDllPath))
                {
                    Log.GetInstance().WriteLine("Warning: Couldn't find " + requiredDllFile + " in " + Path.GetDirectoryName(md3) + ". The library will not work properly without it!");
                }

                Log.GetInstance().WriteLine("Opening " + md3);

                NikonManager manager = new NikonManager(md3);
                manager.DeviceAdded += new DeviceAddedDelegate(_manager_DeviceAdded);
                manager.DeviceRemoved += new DeviceRemovedDelegate(_manager_DeviceRemoved);

                _objects.Add(new ObjectModel(manager));
                _managers.Add(manager);
            }
        }

        public void Shutdown()
        {
            foreach (ObjectModel model in _objects)
            {
                model.StopLiveView();
            }

            foreach (NikonManager manager in _managers)
            {
                manager.Shutdown();
            }
        }

        void _manager_DeviceRemoved(NikonManager sender, NikonDevice device)
        {
            ObjectModel deviceModelToRemove = null;

            foreach (ObjectModel deviceModel in _objects)
            {
                if (deviceModel.Object == device)
                {
                    deviceModelToRemove = deviceModel;
                }
            }

            _objects.Remove(deviceModelToRemove);
            OnPropertyChanged("NewestIndex");
        }

        void _manager_DeviceAdded(NikonManager sender, NikonDevice device)
        {
            _objects.Add(new ObjectModel(device));
            OnPropertyChanged("NewestIndex");
        }

        public ObservableCollection<ObjectModel> Objects
        {
            get { return _objects; }
        }

        public int NewestIndex
        {
            get { return _objects.Count - 1; }
        }
    }
}
