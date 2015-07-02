using System;
using System.Windows;

namespace AcerFanControl
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnButton_Click(object sender, RoutedEventArgs e)
        {
            try { AcerFanControlLib.TurnOn(); }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }

        private void OffButton_Click(object sender, RoutedEventArgs e)
        {
            try { AcerFanControlLib.TurnOff(); }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
    }
}
