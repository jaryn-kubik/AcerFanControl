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
        private DateTime next = DateTime.MinValue;
        private DateTime sensorNext = DateTime.MinValue;

        private Action onTimer;
        public event Action OnTimer
        {
            add { onTimer += value; }
            remove { onTimer -= value; }
        }

        private Func<bool, bool> onTrigger;
        public event Func<bool, bool> OnTrigger
        {
            add { onTrigger += value; }
            remove { onTrigger -= value; }
        }

        ~FanControl() { pc.Close(); }
        public FanControl()
        {
            pc.Open();
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += timer_Tick;
            timer_Tick(timer, EventArgs.Empty);
            timer.Start();
        }

        private bool checkSensors()
        {
            bool changed = false;
            if (!getSensors(CPU).Any())
            {
                pc.CPUEnabled = false;
                pc.CPUEnabled = true;
                changed = true;
            }
            if (!getSensors(GPU).Any())
            {
                pc.GPUEnabled = false;
                pc.GPUEnabled = true;
                changed = true;
            }
            return changed;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now >= sensorNext && checkSensors())
                sensorNext = DateTime.Now.AddSeconds(60);

            CPU.Update();
            OnPropertyChanged(nameof(CPUTemp));
            OnPropertyChanged(nameof(CPUMax));
            OnPropertyChanged(nameof(CPUMin));

            GPU.Update();
            OnPropertyChanged(nameof(GPUTemp));
            OnPropertyChanged(nameof(GPUMax));
            OnPropertyChanged(nameof(GPUMin));

            if (DateTime.Now >= next)
            {
                bool? result = onTrigger?.Invoke(CPUTemp > CritTemp || GPUTemp > CritTemp);
                if (result.HasValue && result.Value)
                    next = DateTime.Now.AddSeconds(Interval);
            }
            onTimer?.Invoke();
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

        public float CPUTemp => getTemp(CPU, s => s.Value);
        public float CPUMax => getTemp(CPU, s => s.Max);
        public float CPUMin => getTemp(CPU, s => s.Min);
        public float GPUTemp => getTemp(GPU, s => s.Value);
        public float GPUMax => getTemp(GPU, s => s.Max);
        public float GPUMin => getTemp(GPU, s => s.Min);

        private IEnumerable<ISensor> getSensors(IHardware hw) => hw.Sensors.Where(s => s.SensorType == SensorType.Temperature);
        private float getTemp(IHardware hw, Func<ISensor, float?> selector) => getSensors(hw).Max(selector) ?? 0;

        private IHardware CPU => pc.Hardware.Single(h => h.HardwareType == HardwareType.CPU);
        private IHardware GPU => pc.Hardware.Single(isGPU);
        private bool isGPU(IHardware hw) => hw.HardwareType == HardwareType.GpuAti || hw.HardwareType == HardwareType.GpuNvidia;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}