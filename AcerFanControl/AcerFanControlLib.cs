using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AcerFanControl
{
    public static class AcerFanControlLib
    {
        public static void TurnOn() { checkError(on()); }
        public static void TurnOff() { checkError(off()); }
        private static void checkError(FanControlError result)
        {
            string error = string.Join(" + ", errors.Where(e => result.HasFlag(e.Key)).Select(e => e.Value));
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
        }

        [DllImport("AcerFanControlLib.dll")]
        private static extern FanControlError on();

        [DllImport("AcerFanControlLib.dll")]
        private static extern FanControlError off();

        [Flags]
        private enum FanControlError
        {
            None = 0,
            InvalidDevice = 1 << 0, //Invalid device handle
            ReadError = 1 << 1,     //"Unable to read from port.";
            WriteError = 1 << 2,    //"Unable to write to port.";
            Timeout = 1 << 3,       //"wait_until_bitmask_is_value timeout";
            CloseError = 1 << 4     //"Unable to close handle.";
        };

        private static readonly Dictionary<FanControlError, string> errors = new Dictionary<FanControlError, string>
        {
            {FanControlError.InvalidDevice, "Invalid device handle"},
            {FanControlError.ReadError, "Unable to read from port"},
            {FanControlError.WriteError, "Unable to write to port"},
            {FanControlError.Timeout, "wait_until_bitmask_is_value timeout"},
            {FanControlError.CloseError, "Unable to close handle"}
        };
    }
}