using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Controls;

namespace AcerFanControl
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = fanControl;
            fanControl.OnTrigger += onFanControl;
            fanControl.OnTimer += onTimer;

            addTicks(mainSlider);
            addTicks(tempSlider);
            addTicks(intervalSlider);

            mainSlider.Labels.Add(0, "Off");
            mainSlider.Labels.Add(1, "On");
            mainSlider.ValueChanged += mainSlider_ValueChanged;

            graphics = Graphics.FromImage(bitmap);
            graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
            trayIcon.Text = Title;
            trayIcon.DoubleClick += (sender, args) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Topmost = true;
                Topmost = false;
            };
            StateChanged += (sender, args) => { if (WindowState == WindowState.Minimized) Hide(); };
            Closing += (sender, args) => trayIcon.Dispose();
            Loaded += (sender, args) => WindowState = WindowState.Minimized;
        }

        private readonly FanControl fanControl = new FanControl();
        private readonly System.Windows.Forms.NotifyIcon trayIcon = new System.Windows.Forms.NotifyIcon { Visible = true };
        private readonly Bitmap bitmap = new Bitmap(16, 16);
        private readonly Font drawFont = new Font("Calibri", 12.5f, System.Drawing.FontStyle.Bold);
        private readonly Graphics graphics;

        private Brush getBrush(float temp)
        {
            if (temp > 80)
                return Brushes.Yellow;
            if (temp > 70)
                return Brushes.LawnGreen;
            return Brushes.White;
        }

        private void onTimer()
        {
            float temp = Math.Max(fanControl.CPUTemp, fanControl.GPUTemp);
            graphics.Clear(Color.Transparent);
            graphics.DrawString(((int)temp).ToString(), drawFont, getBrush(temp), -4f, -2);
            trayIcon.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
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
