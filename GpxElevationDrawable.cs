using Microsoft.Maui.Controls.Shapes;
using FrozenNorth.Gpx;

namespace FrozenNorth.Gpx.Maui
{
    public class GpxElevationDrawable : IDrawable
    {
        // instance variables
        private DateTime position = DateTime.MinValue;
        private DateTime? startTime = null;
        private DateTime? endTime = null;
        private double reductionTolerance = 0;
        private GpxPointList originalPoints = null;
		private GpxTrack originalTrack = null;
		private GpxTrackSegmentList segments = new GpxTrackSegmentList();
		private List<GpxPointList> points = new List<GpxPointList>();

        /// <summary>
        /// Color of the background.
        /// </summary>
        public Color BackgroundColor { get; set; } = Colors.Transparent;

		/// <summary>
		/// Space between the outer edge of the background and the graph itself.
		/// </summary>
		public int Padding { get; set; } = 10;

		/// <summary>
		/// Width of the lines in the graph.
		/// </summary>
		public int LineWidth { get; set; } = 2;

		/// <summary>
		/// Color of the lines being drawn.
		/// </summary>
		public Color LineColor { get; set; } = Color.FromRgb(255, 255, 0);

		/// <summary>
		/// Color of the position bar.
		/// </summary>
		public Color PositionBarColor { get; set; } = Color.FromRgb(255, 255, 0);

		/// <summary>
		/// Determines whether or not the position bar is displayed.
		/// </summary>
		public bool ShowPositionBar { get; set; } = false;

        /// <summary>
        /// Width of top and botton ends of the position bar.
        /// </summary>
        public int PositionBarEndWidth { get; set; } = 6;

        /// <summary>
        /// Minimum elevation range of the graph in meters.
        /// </summary>
		public int MinElevationRange { get; set; } = 0;

        /// <summary>
        /// True if there are some points to be drawn.
        /// </summary>
		public bool HasPoints => segments.HasPoints;

		/// <summary>
		/// Duration of the points to be drawn.
		/// </summary>
		public TimeSpan Duration => EndTime - StartTime;

		/// <summary>
		/// Start time of the points to be drawn.
		/// </summary>
		public DateTime StartTime
        {
            get => (startTime != null) ? startTime.Value : segments.StartTime;
            set => startTime = value;
        }

		/// <summary>
		/// End time of the points to be drawn.
		/// </summary>
		public DateTime EndTime
        {
            get => (endTime != null) ? endTime.Value : segments.EndTime;
            set => endTime = value;
        }

    /// <summary>
    /// Tolerance to be used when reducing the number of points using the Douglas Peucker algorithm.
    /// </summary>
    public double ReductionTolerance
        {
            get => reductionTolerance;
			set
            {
                double newTolerance = Math.Min(Math.Max(value, 0), 1);
                if (newTolerance != reductionTolerance)
                {
                    reductionTolerance = newTolerance;
					GetPoints();
				}
			}
        }

        /// <summary>
        /// Number of points.
        /// </summary>
        public int NumPoints
        {
            get
            {
                int count = 0;
				foreach (var segment in segments)
				{
					count += segment.Points.Count;
				}
				return count;
            }
        }

		/// <summary>
		/// Number of reduced points.
		/// </summary>
		public int NumReducedPoints
		{
			get
			{
				int count = 0;
				foreach (var list in points)
				{
					count += list.Count;
				}
				return count;
			}
		}

		/// <summary>
		/// Sets the points to be drawn.
		/// </summary>
		public GpxPointList Points
		{
            get => originalPoints;
            set
            {
                startTime = null;
                endTime = null;
                originalPoints = value;
                originalTrack = null;
                segments.Clear();
                if (originalPoints != null)
                {
                    var segment = new GpxTrackSegment();
                    segment.Points.AddRange(originalPoints);
                    segments.Add(segment);
                }
                GetPoints();
            }
		}

		/// <summary>
		/// Sets the points to be drawn from a track.
		/// </summary>
		public GpxTrack Track
        {
            get => originalTrack;
            set
            {
                startTime = null;
                endTime = null;
                originalTrack = value;
				originalPoints = null;
				segments.Clear();
                if (originalTrack != null)
                {
                    foreach (var segment in originalTrack.Segments)
                    {
                        segments.Add(segment);
                    }
                }
                GetPoints();
            }
		}

		/// <summary>
		/// Gets/sets the current time (i.e where the position bar will be drawn).
		/// </summary>
		public DateTime Position
        {
            get => position;
            set
            {
                position = value;
                if (position < StartTime)
                    position = StartTime;
                else if (position > EndTime)
                    position = EndTime;
            }
        }

        /// <summary>
        /// Gets the points to be drawn, all or reduced.
        /// </summary>
        private void GetPoints()
        {
			points.Clear();
            foreach (var segment in segments)
            {
                if (reductionTolerance != 0)
                {
                    points.Add(segment.Points.GetReducedElevationPoints(reductionTolerance));
                }
                else
                {
                    points.Add(segment.Points);
                }
            }
			Position = StartTime;
		}

		/// <summary>
		/// Draws the graph and position bar.
		/// </summary>
		/// <param name="canvas">Canvas to draw on.</param>
		/// <param name="dirtyRect">Rectangle to draw in.</param>
		public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // configure the canvas
            canvas.StrokeSize = LineWidth;
            canvas.StrokeColor = LineColor;

            // draw the background
            if (BackgroundColor != Colors.Transparent)
            {
                canvas.SetFillPaint(new SolidPaint(BackgroundColor), dirtyRect);
                canvas.FillRectangle(dirtyRect);
            }

			// draw the graph
			double w = dirtyRect.Width - Padding * 2;
            double h = dirtyRect.Height - Padding * 2;
            if (HasPoints)
            {
                foreach (var list in points)
                {
                    double x, y;
					double elevationRange = list.GetElevationRange(out double low, out double high);
                    if (elevationRange <= 0) elevationRange = 0;
					if (elevationRange < MinElevationRange)
                    {
						low -= (MinElevationRange - elevationRange) / 2;
                        elevationRange = MinElevationRange;
					}
                    DateTime startTime = StartTime;
                    DateTime endTime = EndTime;
                    double totalSeconds = (endTime - startTime).TotalSeconds;

                    int i = 0;
                    for (; i < list.Count && list[i].Time < startTime; i++);
                    if (i < list.Count - 1)
                    {
                        x = (list[i].TimeValue - startTime).TotalSeconds / totalSeconds;
                        y = (list[i].ElevationValue - low) / elevationRange;
                        PointF prev = new PointF((float)(Padding + x * w), (float)(Padding + y * h));
                        for (i++; i < list.Count && list[i].TimeValue < endTime; i++)
                        {
                            x = (list[i].TimeValue - startTime).TotalSeconds / totalSeconds;
                            y = 1 - (list[i].ElevationValue - low) / elevationRange;
                            PointF next = new PointF((float)(Padding + x * w), (float)(Padding + y * h));
                            canvas.DrawLine(prev, next);
                            prev = next;
                        }
                    }
                }
            }
            else
            {
                LineGeometry line = new LineGeometry();
                double y = Padding + h / 2.0;
                canvas.DrawLine(Padding, (float)y, (float)(Padding + w), (float)y);
            }

            // draw the position bar
            if (ShowPositionBar)
            {
                TimeSpan barOffset = Position - StartTime;
                float ox = Padding;
                if (HasPoints)
                {
                    ox += (float)barOffset.Ticks / Duration.Ticks * (float)w;
                }
				canvas.StrokeColor = PositionBarColor;
				canvas.DrawLine(ox, Padding, ox, (float)(Padding + h));
                float oy = Padding;
                float half = PositionBarEndWidth / 2;
                canvas.DrawLine(ox - half, oy, ox + half, oy);
                oy += (float)h;
                canvas.DrawLine(ox - half, oy, ox + half, oy);
            }
        }
    }
}
