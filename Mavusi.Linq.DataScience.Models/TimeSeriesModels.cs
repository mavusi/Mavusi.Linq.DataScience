using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mavusi.Linq.DataScience.Models
{
    /// <summary>
    /// Represents a time-series data point with a timestamp and value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public class TimeSeriesPoint<T>
    {
        public DateTime Timestamp { get; init; }
        public T Value { get; init; }

        public TimeSeriesPoint(DateTime timestamp, T value)
        {
            Timestamp = timestamp;
            Value = value;
        }
    }
}
