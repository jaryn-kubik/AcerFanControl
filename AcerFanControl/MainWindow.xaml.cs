using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AcerFanControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            addTicks(mainSlider);
            addTicks(tempSlider);
            mainSlider.Tag = new Dictionary<double, string> { { 0, "Off" }, { 1, "On" } };
            mainSlider.ValueChanged += mainSlider_ValueChanged;

            tempSlider.Value = Config.Temperature;
            tempSlider.ValueChanged += tempSlider_ValueChanged;

            DataContext = new FanControl(onFanControl);
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

        private void tempSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Config.Temperature = (int)e.NewValue;
        }
    }
}
