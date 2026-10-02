using System;
using System.Windows;
using System.Windows.Controls;


namespace TwinCat_Motion_ADS
{
    /// <summary>
    /// Interaction logic for SettingControl.xaml
    /// </summary>
    public partial class SettingControlMainWindow : UserControl
    {
        public SettingControlMainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public new string SetValue { get; set; }
        public string SetName { get; set; }
        public int BoxWidth { get; set; } = 200; ///Width of the editable box
        public string strTests { get; set; }
        public int TextWidth { get; set; } = 260; ///Width of the lable for the box
        public Thickness LabelMargin { get; set; } = new Thickness(0, 0, 20, 0);
        public string BoxToolTip { get; set; }
    }
}
