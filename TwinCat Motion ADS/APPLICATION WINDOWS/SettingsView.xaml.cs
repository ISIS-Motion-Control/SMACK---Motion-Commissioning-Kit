using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TwinCAT.Ads;



namespace TwinCat_Motion_ADS
{
    /// <summary>
    /// Interaction logic for NcAxisView.xaml
    /// </summary>
    public partial class SettingsView : UserControl, INotifyPropertyChanged
    {
        #region Properties
        readonly MainWindow windowData;
        public string selectedFolder = string.Empty;

        public SettingString AmsNetIdSetting { get; set; } = new("amsNetID");
        public SettingString Xl80FileSetting { get; set; } = new("XL80exe");

        //Properties.Settings.Default.amsNetID;



        #endregion


        public SettingsView()
        {
            InitializeComponent();
            windowData = (MainWindow)Application.Current.MainWindow; 
            SetupBinds();

        }

        
        public void SetupBinds()
        {
            XamlUI.TextboxBinding(SettingAmsNetId.SettingValue, AmsNetIdSetting, "UiVal", UpdateSourceTrigger.PropertyChanged);
            XamlUI.TextboxBinding(SettingXl80Path.SettingValue, Xl80FileSetting, "UiVal", UpdateSourceTrigger.PropertyChanged);

        }

        private void ConnectToPlc_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Connecting to PLC...");
            windowData.Plc = new PLC(AmsNetIdSetting.Val, 852);
            windowData.Plc.setupPLC();
            if (windowData.Plc.AdsState == AdsState.Invalid)
            {
                Console.WriteLine("Ads state is invalid");
            }
            else if (windowData.Plc.AdsState == AdsState.Stop)
            {
                Console.WriteLine("Device connected but PLC not running");
            }
            else if (windowData.Plc.AdsState == AdsState.Run)
            {
                Console.WriteLine("Device connected and running");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void selectXl80Path_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog fbd = new();
            fbd.Filter = "*.exe|*.EXE*";
            string selectedFile = "";
            if (fbd.ShowDialog() == true)
            {
                selectedFile = fbd.FileName;
            }

            //If no file selected return
            if (String.IsNullOrEmpty(selectedFile))
            {
                return;
            }

            Console.WriteLine(selectedFile);
            Xl80FileSetting.UiVal = selectedFile;
        }
        //Generic method for handling commands to the axis

    }
}
