using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;

namespace AcerFanControl
{
    public class FanControl : INotifyPropertyChanged
    {
        private readonly Computer pc = new Computer { CPUEnabled = true, GPUEnabled = true };
        private readonly Config config = new Config(nameof(AcerFanControl));
        private readonly Func<bool, bool> trigger;
        private DateTime next = DateTime.MinValue;

        ~FanControl() { pc.Close(); }
        public FanControl(Func<bool, bool> trigger)
        {
            this.trigger = trigger;
            pc.Open();

            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += timer_Tick;
            timer_Tick(timer, EventArgs.Empty);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            CPU.Update();
            OnPropertyChanged(nameof(CPUTemp));
            OnPropertyChanged(nameof(CPUMax));
            OnPropertyChanged(nameof(CPUMin));

            GPU.Update();
            OnPropertyChanged(nameof(GPUTemp));
            OnPropertyChanged(nameof(GPUMax));
            OnPropertyChanged(nameof(GPUMin));

            if (DateTime.Now >= next && trigger(CPUTemp > CritTemp || GPUTemp > CritTemp))
                next = DateTime.Now.AddSeconds(Interval);
        }

        public int CritTemp
        {
            get { return config.GetValue(nameof(CritTemp), 80); }
            set
            {
                config.SetValue(nameof(CritTemp), value);
                OnPropertyChanged(nameof(CritTemp));
            }
        }

        public int Interval
        {
            get { return config.GetValue(nameof(Interval), 30); }
            set
            {
                config.SetValue(nameof(Interval), value);
                OnPropertyChanged(nameof(Interval));
            }
        }

        public List<int> pica { get; } = new List<int>();

        public float CPUTemp => getTemp(CPU, s => s.Value);
        public float CPUMax => getTemp(CPU, s => s.Max);
        public float CPUMin => getTemp(CPU, s => s.Min);
        public float GPUTemp => getTemp(GPU, s => s.Value);
        public float GPUMax => getTemp(GPU, s => s.Max);
        public float GPUMin => getTemp(GPU, s => s.Min);

        private float getTemp(IHardware hw, Func<ISensor, float?> selector)
            => hw.Sensors.Where(s => s.SensorType == SensorType.Temperature).Max(selector) ?? 0;

        private IHardware CPU => pc.Hardware.Single(h => h.HardwareType == HardwareType.CPU);
        private IHardware GPU => pc.Hardware.Single(isGPU);
        private bool isGPU(IHardware hw) => hw.HardwareType == HardwareType.GpuAti || hw.HardwareType == HardwareType.GpuNvidia;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}