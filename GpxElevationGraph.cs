using FrozenNorth.Gpx;

namespace FrozenNorth.Gpx.Maui
{
	public class GpxElevationGraph : GraphicsView
	{
		// instance variables
		private GpxElevationDrawable drawable = new GpxElevationDrawable();
		private bool userCanMovePositionBar = false;

		/// <summary>
		/// If ShowPositionBar is true, fired when the user interactively moves the position bar.
		/// </summary>
		public EventHandler<TouchEventArgs> PositionBarMoved;

		/// <summary>
		/// Creates a GraphicsView with a GpxTrackElevationDrawable drawable.
		/// </summary>
		public GpxElevationGraph()
			: base()
		{
			Drawable = drawable;
			BackgroundColor = Colors.Gray;
			HeightRequest = 60;
		}

		/// <summary>
		/// Color of the background.
		/// </summary>
		public new Color BackgroundColor
		{
			get => drawable.BackgroundColor;
			set
			{
				if (value != drawable.BackgroundColor)
				{
					drawable.BackgroundColor = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Space between the outer edge of the background and the graph itself.
		/// </summary>
		public int Padding
		{
			get => drawable.Padding;
			set
			{
				if (value != drawable.Padding)
				{
					drawable.Padding = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Width of the lines in the graph.
		/// </summary>
		public int LineWidth
		{
			get => drawable.LineWidth;
			set
			{
				if (value != drawable.LineWidth)
				{
					drawable.LineWidth = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Color of the lines being drawn.
		/// </summary>
		public Color LineColor
		{
			get => drawable.LineColor;
			set
			{
				if (value != drawable.LineColor)
				{
					drawable.LineColor = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Color of the position bar.
		/// </summary>
		public Color PositionBarColor
		{
			get => drawable.PositionBarColor;
			set
			{
				if (value != drawable.PositionBarColor)
				{
					drawable.PositionBarColor = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Determines whether or not the position bar is displayed.
		/// </summary>
		public bool ShowPositionBar
		{
			get => drawable.ShowPositionBar;
			set
			{
				if (value != drawable.ShowPositionBar)
				{
					drawable.ShowPositionBar = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Width of top and botton ends of the position bar.
		/// </summary>
		public int PositionBarEndWidth
		{
			get => drawable.PositionBarEndWidth;
			set
			{
				if (value != drawable.PositionBarEndWidth)
				{
					drawable.PositionBarEndWidth = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Minimum elevation range of the graph in meters.
		/// </summary>
		public int MinElevationRange
		{
			get => drawable.MinElevationRange;
			set
			{
				if (value != drawable.MinElevationRange)
				{
					drawable.MinElevationRange = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Sets the points to be drawn.
		/// </summary>
		public GpxPointList Points
		{
			get => drawable.Points;
			set
			{
				if (value != drawable.Points)
				{
					drawable.Points = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Sets the points to be drawn from a track.
		/// </summary>
		public GpxTrack Track
		{
			get => drawable.Track;
			set
			{
				if (value != drawable.Track)
				{
					drawable.Track = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Gets/sets the current time (i.e where the bar will be drawn).
		/// </summary>
		public DateTime Position
		{
			get => drawable.Position;
			set
			{
				if (value != drawable.Position)
				{
					drawable.Position = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Duration of the track.
		/// </summary>
		public TimeSpan Duration => drawable.Duration;

		/// <summary>
		/// Start time of the track.
		/// </summary>
		public DateTime StartTime
		{
			get => drawable.StartTime;
			set
            {
                if (value != drawable.StartTime)
                {
                    drawable.StartTime = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// End time of the track.
        /// </summary>
        public DateTime EndTime
        {
            get => drawable.EndTime;
            set
            {
                if (value != drawable.EndTime)
                {
                    drawable.EndTime = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Tolerance to be used when reducing the number of points using the Douglas Peucker algorithm.
        /// </summary>
        public double ReductionTolerance
		{
			get => drawable.ReductionTolerance;
			set
			{
				if (value != drawable.ReductionTolerance)
				{
					drawable.ReductionTolerance = value;
					Invalidate();
				}
			}
		}

		/// <summary>
		/// Number of points.
		/// </summary>
		public int NumPoints => drawable.NumPoints;

		/// <summary>
		/// Number of reduced points.
		/// </summary>
		public int NumReducedPoints => drawable.NumReducedPoints;

		/// <summary>
		/// Enables/disables allowing the user to move the position bar.
		/// </summary>
		public bool UserCanMovePositionBar
		{
			get => userCanMovePositionBar;
			set
			{
				if (value != userCanMovePositionBar)
				{
					userCanMovePositionBar = value;
					if (userCanMovePositionBar)
					{
						StartInteraction += OnStartInteraction;
						DragInteraction += OnDragInteraction;
					}
					else
					{
						StartInteraction -= OnStartInteraction;
						DragInteraction -= OnDragInteraction;
					}
				}
			}
		}

		/// <summary>
		/// Moves the time bar.
		/// </summary>
		private void OnStartInteraction(object sender, TouchEventArgs e)
		{
			SetTimeBarPosition(e);
		}

		/// <summary>
		/// Moves the time bar.
		/// </summary>
		private void OnDragInteraction(object sender, TouchEventArgs e)
		{
			SetTimeBarPosition(e);
		}

		/// <summary>
		/// Sets the time bar and video position from a mouse position.
		/// </summary>
		/// <param name="e">Mouse event arguments.</param>
		private void SetTimeBarPosition(TouchEventArgs e)
		{
			if (drawable.HasPoints)
			{
				double w = Width - Padding * 2;
				double x = e.Touches[0].X - Padding;
				if (x < 0) x = 0;
				if (x > w) x = w;
				long position = (long)(Duration.Ticks * x / w);
				DateTime newTime = StartTime + TimeSpan.FromTicks(position);
				if (newTime != Position)
				{
					Position = newTime;
					PositionBarMoved?.Invoke(this, e);
				}
			}
		}
	}
}