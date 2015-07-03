using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using OpenHardwareMonitor.Hardware;

namespace AcerFanControl
{
    public class FanControl : INotifyPropertyChanged
    {
        private readonly Computer pc;
        private readonly Action<bool> trigger;

        public FanControl(Action<bool> trigger)
        {
            this.trigger = trigger;
            pc = new Computer { CPUEnabled = true, GPUEnabled = true };
            pc.Open();

            DispatcherTimer t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            t.Tick += t_Tick;
            t.Start();
        }

        private void t_Tick(object sender, EventArgs e)
        {
            CPU.Update();
            OnPropertyChanged("CPUTemp");
            OnPropertyChanged("CPUMax");
            OnPropertyChanged("CPUMin");

            GPU.Update();
            OnPropertyChanged("GPUTemp");
            OnPropertyChanged("GPUMax");
            OnPropertyChanged("GPUMin");

            trigger(Math.Max(CPUTemp, GPUTemp) > Config.Temperature);
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