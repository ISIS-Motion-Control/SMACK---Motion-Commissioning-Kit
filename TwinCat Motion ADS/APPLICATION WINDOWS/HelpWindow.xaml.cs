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
using System.Windows.Shapes;
using TwinCat_Motion_ADS.Application_Windows;

namespace TwinCat_Motion_ADS
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpScreen_EndToEnd helpScreen_EndToEnd = new();
        public HelpScreen_Unidirectional helpScreen_Unidirectional = new();
        public HelpScreen_Bidirectional helpScreen_Bidirectional = new();
        public HelpScreen_Scaling helpScreen_Scaling = new();
        public HelpScreen_Backlash helpScreen_Backlash = new();
        public HelpScreen_AxisSetup helpScreen_AxisSetup = new();

        public HelpWindow()
        {
            InitializeComponent();
            helpWindow.Content = helpScreen_AxisSetup;
        }

        private void MenuSelect_Click(object sender, RoutedEventArgs e)
        {
            if(((Button)sender) == EndToEnd)
            {
                helpWindow.Content = helpScreen_EndToEnd;
            }
            if (((Button)sender) == Unidirectional)
            {
                helpWindow.Content = helpScreen_Unidirectional;
            }
            if (((Button)sender) == Bidirectional)
            {
                helpWindow.Content = helpScreen_Bidirectional;
            }
            if (((Button)sender) == Scaling)
            {
                helpWindow.Content = helpScreen_Scaling;
            }
            if (((Button)sender) == Backlash)
            {
                helpWindow.Content = helpScreen_Backlash;
            }
            if (((Button)sender) == AxisSetup)
            {
                helpWindow.Content = helpScreen_AxisSetup;
            }
        }
    }
}
