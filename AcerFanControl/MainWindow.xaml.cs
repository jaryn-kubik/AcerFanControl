using System;
using System.Windows;
using System.Windows.Controls;

namespace AcerFanControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new FanControl(onFanControl);

            addTicks(mainSlider);
            addTicks(tempSlider);
            addTicks(intervalSlider);

            mainSlider.Labels.Add(0, "Off");
            mainSlider.Labels.Add(1, "On");
            mainSlider.ValueChanged += mainSlider_ValueChanged;

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Shield,
                Visible = true,
                Text = Title
            };
            ni.DoubleClick += (sender, args) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Topmost = true;
                Topmost = false;
            };
            StateChanged += (sender, args) => { if (WindowState == WindowState.Minimized) Hide(); };
            Closing += (sender, args) => ni.Dispose();
            Loaded += (sender, args) => WindowState = WindowState.Minimized;
        }

        private void addTicks(Slider slider)
        {
            for (double d = slider.Minimum; d <= slider.Maximum; d++)
                slider.Ticks.Add(d);
        }

        private bool onFanControl(bool value)
        {
            bool state = ((int)mainSlider.Value) != 0;
            if (state == value)
                return false;

            mainSlider.Value = value ? 1 : 0;
            return true;
        }

        private void mainSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if ((int)e.NewValue == (int)e.OldValue)
                    return;
                if ((int)e.NewValue != 0)
                    AcerFanControlLib.TurnOn();
                else
                    AcerFanControlLib.TurnOff();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
    }
}
