using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Mavusi.DataScience.Visualizations
{
    public static class QuickCharts
    {
        public static string GenerateScatterPlot(List<ChartData> data, int width = 600, int height = 300, bool darkMode = false)
        {
            ScottPlot.Plot myPlot = new();

            foreach (var dataItem in data)
            {
                var scatter1 = myPlot.Add.Scatter(dataItem.xLabels, dataItem.yValues);

                scatter1.LegendText = dataItem.DataLabel;
                scatter1.LineWidth = 3;
            }

            myPlot.ShowLegend(Edge.Right);
            
            myPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericFixedInterval(1);

            myPlot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            myPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;

            myPlot.Legend.Orientation = Orientation.Vertical;

            //set dark mode if requested
            if (darkMode)
            {
                SetDarkMode(myPlot);
            }

            // determine the width of the largest tick label
            return myPlot.GetSvgHtml(width, height);
            //return "quickstart.png";
        }

        private static void SetDarkMode(Plot plot)
        {
            plot.FigureBackground.Color = Color.FromHex("#181818");
            plot.DataBackground.Color = Color.FromHex("#1f1f1f");
            plot.Axes.Color(Color.FromHex("#d7d7d7"));
            plot.Grid.MajorLineColor = Color.FromHex("#404040");
            plot.Legend.BackgroundColor = Color.FromHex("#404040");
            plot.Legend.FontColor = Color.FromHex("#d7d7d7");
            plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");
        }
    }
}
