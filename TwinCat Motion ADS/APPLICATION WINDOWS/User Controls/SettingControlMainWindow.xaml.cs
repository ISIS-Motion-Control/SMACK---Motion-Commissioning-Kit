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
        public int BoxWidth { get; set; } = 200; // Minimum editor width in a standard row
        // Dependency property so the compact Grid layout updates reliably when XAML sets it.
        public static readonly DependencyProperty CompactProperty =
            DependencyProperty.Register(nameof(Compact), typeof(bool), typeof(SettingControlMainWindow),
                                        new PropertyMetadata(false));
        public bool Compact
        {
            get => (bool)GetValue(CompactProperty);
            set => base.SetValue(CompactProperty, value);
        }
        public string strTests { get; set; }
        public int TextWidth { get; set; } = 0; ///Width of the lable for the box
        public Thickness LabelMargin { get; set; } = new Thickness(0, 0, 16, 0);
        public string BoxToolTip { get; set; }
    }
}
