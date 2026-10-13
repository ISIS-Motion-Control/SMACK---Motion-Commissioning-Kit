using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TwinCAT.Ads;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using TwinCat_Motion_ADS.MeasurementDevice;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace TwinCat_Motion_ADS
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Properties
        public PLC Plc;
        public string selectedFolder = string.Empty;
        public TestSuite TestSuiteWindow;
        public HelpWindow HelpWindow;
        public NcAxisView NcAxisView;
        public CsvHelperView CsvHelperView;
        public AirAxisView AirAxisView;
        public SettingsView SettingsView;
        public bool windowClosing = false;

        public MeasurementDevices MeasurementDevices = new();
        public List<MenuItem> measurementMenuItems = new();

        ListBoxWriter lbw;
        public ObservableCollection<ListBoxStatusItem> consoleStringList = new();

        private string _amsNetID;
        public string AmsNetID
        {
            get { return _amsNetID; }
            set
            {
                _amsNetID = value;
                Properties.Settings.Default.amsNetID = value;
                OnPropertyChanged();
                Properties.Settings.Default.Save();
            }
        }

        private string _pageTitle = "NC axis";
        public string PageTitle
        {
            get { return _pageTitle; }
            set { _pageTitle = value; OnPropertyChanged(); }
        }

        private bool _startupComplete = false;
        private bool _isPlcConnected = false;
        public bool IsPlcConnected
        {
            get { return _isPlcConnected; }
            set
            {
                _isPlcConnected = value;
                OnPropertyChanged();
                UpdateSettingsFlash();
                UpdateNcAxisFlash();
            }
        }

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            lbw = new(consoleListBox);
            consoleListBox.ItemsSource = consoleStringList;
            Console.SetOut(lbw);

            AmsNetID = Properties.Settings.Default.amsNetID;
            UpdateSettingsFlash();
            UpdateNcAxisFlash();
            if (!string.IsNullOrEmpty(AmsNetID))
            {
                Plc = new PLC(AmsNetID, 852);
                Plc.checkAdsState();
                if (Plc.AdsState == AdsState.Stop)
                {
                    Console.WriteLine("Device connected but PLC not running");

                }
                else if (Plc.AdsState == AdsState.Run)
                {
                    Console.WriteLine("Device connected and running");

                }
                IsPlcConnected = Plc.AdsState != AdsState.Invalid;
            }
            NcAxisView = new();
            AirAxisView = new();
            CsvHelperView = new();
            SettingsView = new();
            tabbedWindow.Content = NcAxisView;

            _startupComplete = true;
        }
        #endregion

        public static void SetFlashing(Control target, bool flashing)
        {
            if (flashing)
            {
                var brush = new SolidColorBrush(Color.FromRgb(0xF4, 0xB6, 0xB0));
                target.Background = brush;
                var animation = new ColorAnimation
                {
                    From = Color.FromRgb(0xF4, 0xB6, 0xB0),
                    To = Color.FromRgb(0xE5, 0x73, 0x6B),
                    Duration = TimeSpan.FromSeconds(1.0),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            else
            {
                target.ClearValue(Control.BackgroundProperty);
            }
        }

        private void UpdateSettingsFlash()
        {
            SetFlashing(SettingsScreen, !IsPlcConnected);
        }

        private void UpdateNcAxisFlash()
        {
            if (!_startupComplete) return;
            SetFlashing(NcAxis, IsPlcConnected);
        }

        #region Window methods
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (NcAxisView?.testAxis?.testRunning == true || AirAxisView?.TestRunning == true)
            {
                MessageBoxResult dialogResult = MessageBox.Show(
                    "A motion test is running. Close the application anyway?",
                    "Test in progress", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (dialogResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            //bit of a 'hacky' method. Due to the test suite window being hidden and not actually closed I need a way for that window to check if the whole application is closing
            windowClosing = true;
            AirAxisView?.pneumaticAxis?.StopReading();
            Plc?.Dispose();
            //Because I hide the window
            if (TestSuiteWindow != null)
            {
                TestSuiteWindow.Close();
            }
            //Disconnect and close any open measurement device windows
            foreach (var Window in App.Current.Windows)
            {
                if (Window is measurementDeviceWindow)
                {
                    ((measurementDeviceWindow)Window).MDevice.Disconnect();
                    ((measurementDeviceWindow)Window).Close();
                }
            }
            //Disconnect from any remaining connected devices (allows proper disposing)
            foreach (var md in this.MeasurementDevices.MeasurementDeviceList)
            {
                if (md.Connected)
                {
                    md.Disconnect();
                }
            }
        }


        #endregion

        #region Measurement Device Menu
        private void AddNewDevice(object sender, RoutedEventArgs e)
        {
            MeasurementDevices.AddDevice(DeviceTypes.NoneSelected);
            UpdateMeasurementDeviceMenu();
        }


        private void DeviceMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mI = sender as MenuItem;
            int i = 0;
            int deviceIndex = 0;
            foreach (var device in measurementMenuItems)
            {
                if (mI == device)
                {
                    deviceIndex = i;
                }
                i++;
            }

            measurementDeviceWindow newMeasureWindow = new(deviceIndex, MeasurementDevices.MeasurementDeviceList[deviceIndex]);
            newMeasureWindow.Show();
        }

        //update devices menu
        public void UpdateMeasurementDeviceMenu(bool suppress = false)
        {
            MenuItem newMenuItem = new();

            //This binding seems to be bugged, works fine but does not update. - I think due to the style used
            //int testInt = MeasurementDevices.NumberOfDevices - 1;


            newMenuItem.Click += new RoutedEventHandler(DeviceMenu_Click);
            MeasureDevicesMenu.Items.Add(newMenuItem);  //adding to the actual UI
            measurementMenuItems.Add(newMenuItem);      //Adding to internal list

            int deviceIndex = MeasurementDevices.NumberOfDevices - 1;

            if (!suppress)
            {
                measurementDeviceWindow newMeasureWindow = new(deviceIndex, MeasurementDevices.MeasurementDeviceList[deviceIndex]);
                newMeasureWindow.Show();
            }
        }
        public void RemoveMeasurementMenuItem(int index)
        {
            measurementMenuItems.RemoveAt(index); //interal list doesn't contain default items
            index += 4; //need to account for the 4 default items in the menu (Add, import, export, seperator)
            MeasureDevicesMenu.Items.RemoveAt(index);
        }

        private void ImportDevices_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog fbd = new();
            fbd.Filter = "*.XML|*.xml";
            string selectedFile;
            if (fbd.ShowDialog() == true)
            {
                selectedFile = fbd.FileName;
                int temp = MeasurementDevices.ImportDevicesXML(selectedFile);
                if (temp > 0)
                {
                    for (int i = 0; i < temp; i++)
                    {
                        UpdateMeasurementDeviceMenu(true);
                    }
                }
            }
        }

        private void MeasureDevicesMenu_Click(object sender, RoutedEventArgs e)
        {
            MeasureDevicesMenu.Items.Refresh();
            int counter = 0;
            foreach (MenuItem mi in measurementMenuItems)
            {
                mi.Header = MeasurementDevices.MeasurementDeviceList[counter].Name;
                counter++;
            }
            MeasureDevicesMenu.PlacementTarget = DevicesButton;
            MeasureDevicesMenu.Placement = PlacementMode.Right;
            MeasureDevicesMenu.IsOpen = true;
        }


        private void ExportDevices_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog fbd = new();
            fbd.AddExtension = true;
            fbd.DefaultExt = ".XML";
            string selectedFile;
            if (fbd.ShowDialog() == true)
            {
                selectedFile = fbd.FileName;
                MeasurementDevices.ExportDevicesXml(selectedFile);
            }
        }
        #endregion

        // The AMS address is owned by MainWindow and exposed via INotifyPropertyChanged.
        // No separate manual binding is required; the header binds directly to this property.



        private void TestSuiteMenu_Click(object sender, RoutedEventArgs e)
        {
            if (TestSuiteWindow == null)
            {
                TestSuiteWindow = new();
            }
            TestSuiteWindow.Show();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if (((RadioButton)sender) == NcAxis)
            {
                tabbedWindow.Content = NcAxisView;
                PageTitle = "NC axis";
                SetFlashing(NcAxis, false);
            }
            else if (((RadioButton)sender) == AirAxis)
            {
                tabbedWindow.Content = AirAxisView;
                PageTitle = "Air axis";
            }
            else if (((RadioButton)sender) == CsvHelper)
            {
                tabbedWindow.Content = CsvHelperView;
                PageTitle = "CSV helper";
            }
            else if (((RadioButton)sender) == SettingsScreen)
            {
                tabbedWindow.Content = SettingsView;
                PageTitle = "Settings";
            }
        }

        private void clearScreenButton_Click(object sender, RoutedEventArgs e)
        {
            consoleStringList.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void HelpMenu_Click(object sender, RoutedEventArgs e)
        {
            if (HelpWindow == null || !HelpWindow.IsLoaded)
                HelpWindow = new HelpWindow();
            HelpWindow.Show();
            HelpWindow.Activate();
        }
    }
}
