using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TwinCat_Motion_ADS
{
    /// <summary>
    /// Interaction logic for AirAxisView.xaml
    /// </summary>
    public partial class AirAxisView : UserControl
    {
        readonly MainWindow windowData;
        public PneumaticAxis pneumaticAxis;
        public string selectedFolder = string.Empty;
        public AirTestSettings TestSettings = new();
        public bool TestRunning { get; private set; }

        public AirAxisView()
        {
            InitializeComponent();
            windowData = (MainWindow)Application.Current.MainWindow;
            SetupBinds();
        }

        public void SetupBinds()
        {
            XamlUI.CheckBoxBinding("Extended Limit", pneumaticExtended, pneumaticAxis, "ExtendedLimit", BindingMode.OneWay);
            XamlUI.CheckBoxBinding("Retracted Limit", pneumaticRetracted, pneumaticAxis, "RetractedLimit", BindingMode.OneWay);
            XamlUI.CheckBoxBinding("Actuator", CylinderAir, pneumaticAxis, "Cylinder", BindingMode.OneWay);
            XamlUI.CheckBoxBinding("Air on to extend", airOnLogic, pneumaticAxis, "AirOnToExtend", BindingMode.OneWay);
            XamlUI.TextboxBinding(cycles, TestSettings.Cycles, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.TextboxBinding(settlingReads, TestSettings.SettlingReads, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.TextboxBinding(readDelay, TestSettings.ReadDelayMs, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.TextboxBinding(extendDelay, TestSettings.DelayAfterExtend, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.TextboxBinding(retractDelay, TestSettings.DelayAfterRetract, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.TextboxBinding(extendTimeout, TestSettings.ExtendTimeout, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.TextboxBinding(retractTimeout, TestSettings.RetractTimeout, "UiVal", UpdateSourceTrigger.LostFocus);
            XamlUI.CheckBoxBinding("Paused", testPausedCheck, pneumaticAxis, "PauseTest", BindingMode.OneWay);
            XamlUI.CheckBoxBinding("Cancelled", testCancelledCheck, pneumaticAxis, "CancelTest", BindingMode.OneWay);
        }

        private void InitialisePneumatic_Click(object sender, RoutedEventArgs e)
        {
            if (windowData.Plc == null || !windowData.IsPlcConnected)
            {
                Console.WriteLine("Connect to the PLC in Settings before initialising the air axis.");
                return;
            }
            // Stop the previous polling loop before replacing the ADS handles.
            pneumaticAxis?.StopReading();
            pneumaticAxis = new PneumaticAxis(windowData.Plc);
            SetupBinds();
            pneumaticAxis.ReadStatuses();
        }

        private void toggleAirOnBehaviour_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis != null)
            {
                pneumaticAxis.ToggleAirBehaviour();
            }
        }

        private async void ExtendCylinderButton_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis != null)
            {
                if (await pneumaticAxis.ExtendCylinderAndWait() == false)
                {
                    Console.WriteLine("FAILED");
                }
            }
            
        }

        private async void RetractCylinderButton_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis != null)
            {
                if (await pneumaticAxis.RetractCylinderAndWait() == false)
                {
                    Console.WriteLine("FAILED");
                }
            }
            
        }

        private async void ShutterLimitToLimitTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis == null)
            {
                Console.WriteLine("Initialise the air axis first.");
                return;
            }
            if (string.IsNullOrWhiteSpace(pneumaticAxis.TestDirectory) || !System.IO.Directory.Exists(pneumaticAxis.TestDirectory))
            {
                Console.WriteLine("Select a valid save directory before running the air-axis test.");
                return;
            }
            windowData.mainWindowGrid.Focus();
            pneumaticAxis.CancelTest = false;
            pneumaticAxis.PauseTest = false;
            TestRunning = true;
            shutterEnd2End_button.IsEnabled = false;
            initPneumatic.IsEnabled = false;
            extendCylinder_button.IsEnabled = false;
            retractCylinder_button.IsEnabled = false;
            toggleAirOnBehaviour.IsEnabled = false;
            shutterTestFolderDir_button.IsEnabled = false;
            pauseTest.IsEnabled = true;
            cancelTest.IsEnabled = true;
            try
            {
                bool completed = await pneumaticAxis.End2EndTest(TestSettings, windowData.MeasurementDevices);
                if (!completed) Console.WriteLine("Air-axis test did not complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Air-axis test failed: {ex.Message}");
            }
            finally
            {
                pneumaticAxis.PauseTest = false;
                TestRunning = false;
                shutterEnd2End_button.IsEnabled = true;
                initPneumatic.IsEnabled = true;
                extendCylinder_button.IsEnabled = true;
                retractCylinder_button.IsEnabled = true;
                toggleAirOnBehaviour.IsEnabled = true;
                shutterTestFolderDir_button.IsEnabled = true;
                pauseTest.IsEnabled = false;
                pauseTest.Content = "Pause";
                cancelTest.IsEnabled = false;
            }
        }

        private void PauseTest_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis == null) return;
            pneumaticAxis.PauseTest = !pneumaticAxis.PauseTest;
            pauseTest.Content = pneumaticAxis.PauseTest ? "Resume" : "Pause";
        }

        private void CancelTest_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis == null) return;
            // Cooperative cancellation: this finishes a current cylinder movement
            // before stopping the test sequence. It is NOT an emergency stop.
            pneumaticAxis.CancelTest = true;
            pneumaticAxis.PauseTest = false;
            pauseTest.Content = "Pause";
            pauseTest.IsEnabled = false;
            cancelTest.IsEnabled = false;
        }

        private void SelectTestDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (pneumaticAxis == null)
            {
                Console.WriteLine("Initialise the air axis before selecting a test directory.");
                return;
            }
            var fbd = new VistaFolderBrowserDialog();
            if (fbd.ShowDialog() == true)
            {
                selectedFolder = fbd.SelectedPath;
                pneumaticAxis.TestDirectory = selectedFolder;
                Console.WriteLine($"Air-axis test directory: {selectedFolder}");
            }
        }

        
    }
}
