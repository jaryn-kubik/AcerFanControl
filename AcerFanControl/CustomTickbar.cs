using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AcerFanControl
{
    public class CustomTickbar : TickBar
    {
        protected override void OnRender(DrawingContext dc)
        {
            double halfReservedSpace = ReservedSpace * 0.5;
            double range = Maximum - Minimum;
            double tickSize = (ActualWidth - ReservedSpace) / range;

            for (double i = 0; i <= range; i += TickFrequency)
            {
                string label;
                if (!getSlider(this).Labels.TryGetValue(i + Minimum, out label))
                    label = (i + Minimum).ToString();

                var text = new FormattedText(label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    new Typeface("Verdana"), 9, Brushes.Black);

                double x = i * tickSize + halfReservedSpace - text.Width / 2;
                dc.DrawText(text, new Point(x, ActualHeight * 2));
            }

            base.OnRender(dc);
        }

        private static CustomSlider getSlider(DependencyObject child)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            CustomSlider parent = parentObject as CustomSlider;
            return parent ?? getSlider(parentObject);
        }
    }
}