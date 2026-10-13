using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace TwinCat_Motion_ADS
{
    public class ListBoxStatusItem
    {
        public string timestamp { get; set; }
        public string statusMessage { get; set; }

        //Display helpers for the console list in the main window
        public string TimeShort { get; }
        public string Message { get; }
        public string Level { get; }

        public ListBoxStatusItem(string status)
        {
            statusMessage = status;
            timestamp = DateTime.Now.ToString();
            TimeShort = DateTime.Now.ToString("HH:mm:ss");
            Message = (status ?? string.Empty).TrimEnd('\r', '\n');
            Level = Classify(Message);
        }

        //Rough colour-coding of console lines by keyword
        private static string Classify(string m)
        {
            m = m.ToLowerInvariant();
            if (m.Contains("no route") || m.Contains("not open") || m.Contains("timeout") || m.Contains("timed out") || m.Contains("exception") || m.Contains("error"))
                return "Error";
            if (m.Contains("fail") || m.Contains("did not complete") || m.Contains("not running") || m.Contains("no axis") || m.Contains("initialise an axis") || m.Contains("no save directory") || m.Contains("no test selected") || m.Contains("warn") || m.Contains("invalid"))
                return "Warning";
            if (m.Contains("connected and running") || m.Contains("test complete"))
                return "Success";
            return "Info";
        }
    }

    public class ListBoxWriter : TextWriter
    {
        private readonly ListBox list;
        private readonly StringBuilder content = new StringBuilder();
        private readonly object sync = new object();

        public ListBoxWriter(ListBox list)
        {
            this.list = list;
        }

        public override void Write(char value)
        {
            string line = null;
            lock (sync)
            {
                content.Append(value);
                if (value == '\n')
                {
                    line = content.ToString();
                    content.Clear();
                }
            }
            if (line == null) return;

            // ADS callbacks can write from a worker thread. ObservableCollection
            // must only be changed on the WPF UI dispatcher.
            ListBoxStatusItem item = new ListBoxStatusItem(line);
            void AddLine()
            {
                if (list.ItemsSource is ObservableCollection<ListBoxStatusItem> items)
                    items.Add(item);
            }
            if (list.Dispatcher.CheckAccess()) AddLine();
            else list.Dispatcher.BeginInvoke((Action)AddLine);
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }

    class ListBoxBehavior
    {
        static readonly Dictionary<ListBox, Capture> Associations =
               new Dictionary<ListBox, Capture>();

        public static bool GetScrollOnNewItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollOnNewItemProperty);
        }

        public static void SetScrollOnNewItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollOnNewItemProperty, value);
        }

        public static readonly DependencyProperty ScrollOnNewItemProperty =
            DependencyProperty.RegisterAttached(
                "ScrollOnNewItem",
                typeof(bool),
                typeof(ListBoxBehavior),
                new UIPropertyMetadata(false, OnScrollOnNewItemChanged));

        public static void OnScrollOnNewItemChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            if (listBox == null) return;
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue) return;
            if (newValue)
            {
                listBox.Loaded += ListBox_Loaded;
                listBox.Unloaded += ListBox_Unloaded;
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.AddValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
                listBox.Unloaded -= ListBox_Unloaded;
                if (Associations.ContainsKey(listBox))
                    Associations[listBox].Dispose();
                var itemsSourcePropertyDescriptor = TypeDescriptor.GetProperties(listBox)["ItemsSource"];
                itemsSourcePropertyDescriptor.RemoveValueChanged(listBox, ListBox_ItemsSourceChanged);
            }
        }

        private static void ListBox_ItemsSourceChanged(object sender, EventArgs e)
        {
            var listBox = (ListBox)sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            Associations[listBox] = new Capture(listBox);
        }

        static void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (Associations.ContainsKey(listBox))
                Associations[listBox].Dispose();
            listBox.Unloaded -= ListBox_Unloaded;
        }

        static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var incc = listBox.Items as INotifyCollectionChanged;
            if (incc == null) return;
            listBox.Loaded -= ListBox_Loaded;
            Associations[listBox] = new Capture(listBox);
        }

        class Capture : IDisposable
        {
            private readonly ListBox listBox;
            private readonly INotifyCollectionChanged incc;

            public Capture(ListBox listBox)
            {
                this.listBox = listBox;
                incc = listBox.ItemsSource as INotifyCollectionChanged;
                if (incc != null)
                {
                    incc.CollectionChanged += incc_CollectionChanged;
                }
            }

            void incc_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    listBox.ScrollIntoView(e.NewItems[0]);
                }
            }

            public void Dispose()
            {
                if (incc != null)
                    incc.CollectionChanged -= incc_CollectionChanged;
            }
        }
    }
}
