using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;

namespace AcerFanControl
{
    public class FanControl : INotifyPropertyChanged
    {
        private readonly Computer pc = new Computer { CPUEnabled = true, GPUEnabled = true };
        private readonly Config config = new Config("AcerFanControl");
        private readonly Func<bool, bool> trigger;
        private DateTime next = DateTime.MinValue;

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
            OnPropertyChanged("CPUTemp");
            OnPropertyChanged("CPUMax");
            OnPropertyChanged("CPUMin");

            GPU.Update();
            OnPropertyChanged("GPUTemp");
            OnPropertyChanged("GPUMax");
            OnPropertyChanged("GPUMin");

            if (DateTime.Now >= next && trigger(CPUTemp > CritTemp || GPUTemp > CritTemp))
                next = DateTime.Now.AddSeconds(Interval);
        }

        public int CritTemp
        {
            get { return config.GetValue("CritTemp", 80); }
            set
            {
                config.SetValue("CritTemp", value);
                OnPropertyChanged("CritTemp");
            }
        }

        public int Interval
        {
            get { return config.GetValue("Interval", 30); }
            set
            {
                config.SetValue("Interval", value);
                OnPropertyChanged("Interval");
            }
        }

        public float CPUTemp { get { return getTemp(CPU, s => s.Value); } }
        public float CPUMax { get { return getTemp(CPU, s => s.Max); } }
        public float CPUMin { get { return getTemp(CPU, s => s.Min); } }
        public float GPUTemp { get { return getTemp(GPU, s => s.Value); } }
        public float GPUMax { get { return getTemp(GPU, s => s.Max); } }
        public float GPUMin { get { return getTemp(GPU, s => s.Min); } }

        private float getTemp(IHardware hw, Func<ISensor, float?> selector)
        { return hw.Sensors.Where(s => s.SensorType == SensorType.Temperature).Max(selector) ?? 0; }

        private IHardware CPU { get { return pc.Hardware.Single(h => h.HardwareType == HardwareType.CPU); } }
        private IHardware GPU { get { return pc.Hardware.Single(isGPU); } }
        private bool isGPU(IHardware hw) { return hw.HardwareType == HardwareType.GpuAti || hw.HardwareType == HardwareType.GpuNvidia; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler eventHandler = PropertyChanged;
            if (eventHandler != null)
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}