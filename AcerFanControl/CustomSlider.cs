using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AcerFanControl
{
    public class CustomSlider : Slider
    {
        public CustomSlider()
        {
            var m = Margin;
            m.Bottom += 15;
            Margin = m;
            Maximum = 1;
            TickPlacement = TickPlacement.BottomRight;
            SmallChange = 1;
            IsSnapToTickEnabled = true;
            Foreground = Brushes.DarkGray;
            Labels = new Dictionary<double, string>();
        }

        public Dictionary<double, string> Labels { get; private set; }
    }
}