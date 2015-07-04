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
        }

        private void addTicks(Slider slider)
        {
            for (double d = slider.Minimum; d <= slider.Maximum; d++)
                slider.Ticks.Add(d);
        }

        private void onFanControl(bool value)
        {
            bool state = ((int)mainSlider.Value) != 0;
            if (state == value)
                return;

            mainSlider.Value = value ? 1 : 0;
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
