using Abt.Controls.SciChart;
using Abt.Controls.SciChart.ChartModifiers;
using Abt.Controls.SciChart.Visuals;
using Abt.Controls.SciChart.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MotorController.Helpers
{
    public class AxisMouseWheelModifier : ChartModifierBase
    {
        private static readonly DependencyProperty AxisIdProperty = DependencyProperty.Register("AxisId", typeof(string), typeof(AxisMouseWheelModifier), new PropertyMetadata(AxisBase.DefaultAxisId));

        public string AxisId
        {
            get { return (string)GetValue(AxisIdProperty); }
            set { SetValue(AxisIdProperty, value); }
        }

        public static readonly DependencyProperty IsPanProperty = DependencyProperty.Register(
            "IsPan", typeof(bool), typeof(AxisMouseWheelModifier), new PropertyMetadata(default(bool)));

        public bool IsPan
        {
            get { return (bool)GetValue(IsPanProperty); }
            set { SetValue(IsPanProperty, value); }
        }

        public override void OnModifierMouseWheel(ModifierMouseArgs e)
        {
            base.OnModifierMouseWheel(e);

            // Get the YAxis. TODO: You could extend this to apply to all axis, or just one
            var yAxis = GetYAxis(AxisId);
            var xAxis = GetXAxis(AxisId);
            // Check if the point is within bounds of the axis
            bool isOnYAxis = IsPointWithinBounds(e.MousePoint, yAxis);
            bool isOnXAxis = IsPointWithinBounds(e.MousePoint, xAxis);
            if(isOnYAxis)
            {
                if(IsPan)
                {
                    // Scrolling or panning on the axis
                    double numPixels = 0.5 * e.Delta;
                    yAxis.Scroll(numPixels, ClipMode.None);
                }
                else
                {
                    // Zooming on the axis
                    double zoomFactor = 0.5 * e.Delta / 120d;
                    yAxis.ZoomBy(zoomFactor, zoomFactor);
                }
            }
            else if(isOnXAxis)
            {
                if(IsPan)
                {
                    // Scrolling or panning on the axis
                    double numPixels = 0.5 * e.Delta;
                    xAxis.Scroll(numPixels, ClipMode.None);
                }
                else
                {
                    // Zooming on the axis
                    double zoomFactor = 0.5 * e.Delta / 120d;
                    xAxis.ZoomBy(zoomFactor, zoomFactor);
                }
            }
            else if(!isOnYAxis && !isOnXAxis)
            {
                
                // If zooming in
                if(e.Delta > 0)
                    xAxis.ZoomBy(-0.1, -0.1);
                // If zooming out
                else if(e.Delta < 0)
                    xAxis.ZoomBy(0.1, 0.1);
                /*
                // Now don't do anything else
                //e.Handled = true;
                if(e.Delta > 0)
                    yAxis.ZoomBy(-0.1, -0.1);
                else if(e.Delta < 0)
                    yAxis.ZoomBy(0.1, 0.1);

                //e.Handled = true;
                */
            }
        }
    }
}
