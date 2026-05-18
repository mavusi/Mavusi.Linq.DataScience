using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mavusi.DataScience.Visualizations
{
    public class ChartData
    {
        public string DataLabel { get; set; }
        public List<double> yValues { get; set; } = new();
        public List<string> xLabels { get; set; } = new();
    }
}
