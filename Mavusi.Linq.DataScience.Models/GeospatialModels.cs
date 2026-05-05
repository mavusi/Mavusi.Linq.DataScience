using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mavusi.Linq.DataScience.Models
{
    /// <summary>
    /// Represents a geographical coordinate with latitude and longitude.
    /// </summary>
    public record GeoCoordinate(double Latitude, double Longitude);

    /// <summary>
    /// Represents a result from a distance calculation between two points.
    /// </summary>
    public record GeoDistance(GeoCoordinate From, GeoCoordinate To, double DistanceKm, double DistanceMiles);
}
