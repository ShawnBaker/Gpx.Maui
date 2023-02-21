using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace FrozenNorth.Gpx.Maui
{
    public class GpxMap : Microsoft.Maui.Controls.Maps.Map
    {
        // instance variables
        private Gpx gpx = new ();
        private bool showRoutes = true;
        private bool showWaypoints = true;
        private bool showTracks = true;
        private Color routeColor = Colors.Blue;
        private Color trackColor = Colors.Red;
        private double reductionTolerance = 0;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GpxMap()
            : base()
        {
            MapType = MapType.Hybrid;
        }

        /// <summary>
        /// Gpx object to be displayed.
        /// </summary>
        public Gpx Gpx
        {
            get => gpx;
            set
            {
                gpx = value;
                Refresh();
                SetRegion();
            }
        }

        /// <summary>
        /// Route to be displayed.
        /// </summary>
        public GpxRoute Route
        {
            get => gpx.Routes[0];
            set
            {
                gpx = new Gpx();
                gpx.Routes.Add(value);
                Refresh();
                SetRegion();
            }
        }

        /// <summary>
        /// Waypoints to be displayed.
        /// </summary>
        public GpxPointList Waypoints
        {
            get => gpx.Waypoints;
            set
            {
                gpx = new Gpx();
                gpx.Waypoints.AddRange(value);
                Refresh();
                SetRegion();
            }
        }

        /// <summary>
        /// Track to be displayed.
        /// </summary>
        public GpxTrack Track
        {
            get => gpx.Tracks[0];
            set
            {
                gpx = new Gpx();
                gpx.Tracks.Add(value);
                Refresh();
                SetRegion();
            }
        }

        /// <summary>
        /// Determines if routes are shown or not.
        /// </summary>
        public bool ShowRoutes
        {
            get => showRoutes;
            set
            {
                if (value != showRoutes)
                {
                    showRoutes = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Determines if waypoints are shown or not.
        /// </summary>
        public bool ShowWaypoints
        {
            get => showWaypoints;
            set
            {
                if (value != showWaypoints)
                {
                    showWaypoints = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Determines if tracks are shown or not.
        /// </summary>
        public bool ShowTracks
        {
            get => showTracks;
            set
            {
                if (value != showTracks)
                {
                    showTracks = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Color of the routes.
        /// </summary>
        public Color RouteColor
        {
            get => routeColor;
            set
            {
                if (value != routeColor)
                {
                    routeColor = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Color of the tracks.
        /// </summary>
        public Color TrackColor
        {
            get => trackColor;
            set
            {
                if (value != trackColor)
                {
                    trackColor = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Tolerance to be used when reducing the number of points using the Douglas Peucker algorithm.
        /// </summary>
        public double ReductionTolerance
        {
            get => reductionTolerance;
            set
            {
                if (value != reductionTolerance)
                {
                    reductionTolerance = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Number of points.
        /// </summary>
        public int NumTrackPoints { get; private set; } = 0;

        /// <summary>
        /// Number of reduced points.
        /// </summary>
        public int NumReducedTrackPoints { get; private set; } = 0;

        /// <summary>
        /// Refreshes the routes, waypoints and tracks.
        /// </summary>
        private void Refresh()
        {
            MapElements.Clear();
            Pins.Clear();
            if (gpx != null)
            {
                if (ShowRoutes)
                {
                    foreach (var route in gpx.Routes)
                    {
                        var segLine = new Polyline();
                        segLine.StrokeColor = routeColor;
                        foreach (var point in route.Points)
                        {
                            segLine.Add(new Location(point.Latitude, point.Longitude));
                        }
                        MapElements.Add(segLine);
                    }
                }
                if (ShowWaypoints)
                {
                    foreach (var point in gpx.Waypoints)
                    {
                        Pin pin = new()
                        {
                            Label = string.IsNullOrEmpty(point.Name) ? (string.IsNullOrEmpty(point.Description) ? "Unknown" : point.Description) : point.Name,
                            Address = point.Description,
                            Type = PinType.Place,
                            Location = new Location(point.Latitude, point.Longitude)
                        };
                        Pins.Add(pin);
                    }
                }
                NumTrackPoints = 0;
                NumReducedTrackPoints = 0;
                if (ShowTracks)
                {
                    foreach (var track in gpx.Tracks)
                    {
                        foreach (var segment in track.Segments)
                        {
                            var segLine = new Polyline();
                            segLine.StrokeColor = trackColor;
                            GpxPointList points = segment.Points;
                            NumTrackPoints += points.Count;
                            if (reductionTolerance != 0)
                            {
                                points = points.GetReducedLocationPoints(reductionTolerance);
                            }
                            NumReducedTrackPoints += points.Count;
                            foreach (var point in points)
                            {
                                segLine.Add(new Location(point.Latitude, point.Longitude));
                            }
                            MapElements.Add(segLine);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the region based on the GPX data.
        /// </summary>
        private void SetRegion()
        {
            var min = new GpxPoint(double.MaxValue, double.MaxValue);
            var max = new GpxPoint(double.MinValue, double.MinValue);
            if (gpx != null)
            {
                if (ShowRoutes)
                {
                    foreach (var route in gpx.Routes)
                    {
                        foreach (var point in route.Points)
                        {
                            if (point.Latitude < min.Latitude) min.Latitude = point.Latitude;
                            if (point.Longitude < min.Longitude) min.Longitude = point.Longitude;
                            if (point.Latitude > max.Latitude) max.Latitude = point.Latitude;
                            if (point.Longitude > max.Longitude) max.Longitude = point.Longitude;
                        }
                    }
                }
                if (ShowWaypoints)
                {
                    foreach (var point in gpx.Waypoints)
                    {
                        if (point.Latitude < min.Latitude) min.Latitude = point.Latitude;
                        if (point.Longitude < min.Longitude) min.Longitude = point.Longitude;
                        if (point.Latitude > max.Latitude) max.Latitude = point.Latitude;
                        if (point.Longitude > max.Longitude) max.Longitude = point.Longitude;
                    }
                }
                if (ShowTracks)
                {
                    foreach (var track in gpx.Tracks)
                    {
                        foreach (var segment in track.Segments)
                        {
                            foreach (var point in segment.Points)
                            {
                                if (point.Latitude < min.Latitude) min.Latitude = point.Latitude;
                                if (point.Longitude < min.Longitude) min.Longitude = point.Longitude;
                                if (point.Latitude > max.Latitude) max.Latitude = point.Latitude;
                                if (point.Longitude > max.Longitude) max.Longitude = point.Longitude;
                            }
                        }
                    }
                }
            }

            if (min.Latitude != double.MaxValue)
            {
                var center = new Location((min.Latitude + max.Latitude) / 2, (min.Longitude + max.Longitude) / 2);
                var distance = new Distance(GpxPointList.DistanceBetweenPoints(min, max) / 2 * 1500);
                var region = MapSpan.FromCenterAndRadius(center, distance);
                MoveToRegion(region);
            }
            else
            {
                var center = new Location(45.082207, 6.054350);
                var region = new MapSpan(center, 0.04, 0.04);
                MoveToRegion(region);
            }
        }
    }
}
