using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Saluse.ComicReader.Application.Managers
{
	/// <summary>
	///		Manages the image viewer for things such as rotation and zoom
	/// </summary>
	class ViewManager
	{
		[DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
		[return: MarshalAsAttribute(UnmanagedType.Bool)]
		private static extern bool SetCursorPos(int x, int y);

		#region Enumerations

		public enum Direction
		{
			Unknown,
			Up,
			Down,
			Left,
			Right
		}

		#endregion

		#region Static members

		public static void Swap(ref double x, ref double y)
		{
			double temp = x;
			x = y;
			y = temp;
		}

		public static void Swap(ref Point point)
		{
			double temp = point.X;
			point.X = point.Y;
			point.Y = temp;
		}

		public static Direction GetDirection(Key key)
		{
			Direction direction = Direction.Unknown;
			switch (key)
			{
				case Key.Up:
					direction = Direction.Up;
					break;

				case Key.Down:
					direction = Direction.Down;
					break;

				case Key.Left:
					direction = Direction.Left;
					break;

				case Key.Right:
					direction = Direction.Right;
					break;
			}

			return direction;
		}

		#endregion

		#region Events/Delegates

		public delegate void RotationCompletedDelegate();
		public event RotationCompletedDelegate RotationCompleted;

		public delegate void ZoomCompletedDelegate();
		public event ZoomCompletedDelegate ZoomCompleted;

		/*
		public delegate void MouseDirectionDelegate(Direction directionX, Direction directionY);
		public event MouseDirectionDelegate MouseDirection;
		*/

		#endregion

		#region Private Variables

		private const double PAN_RATIO = 0.5;
		private readonly Duration ZERO_DURATION = new Duration(TimeSpan.FromTicks(0));
		private readonly Duration TRANSLATE_DURATION;

		private MainWindow _window;
		private Grid _mainContainer;
		private ScrollBar _imageScrollBar;
		private ScrollBar _progressScrollBar;
		private RotateTransform _rotateTransform;
		private TranslateTransform _imageTranslateTransform;
		private Image _comicImage;
		private Viewbox _imageViewbox;
		private Border _numberingContainer;
		private Point _lastPoint = new Point();

		// Used for converting from current Windows DPI to Pixels
		private Matrix _pixelMatrix;

		private Storyboard _portraitStoryboard = null;
		private Storyboard _landscapeStoryboard = null;

		private Storyboard _translateXStoryboard = null;
		private Storyboard _translateYStoryboard = null;
		private DoubleAnimation _translateXDoubleAnimation = null;
		private DoubleAnimation _translateYDoubleAnimation = null;

		private DoubleAnimation _imageScrollBarAnimation = null;

		#endregion

		public ViewManager(MainWindow window)
		{
			_window = window;
			_mainContainer = (Grid)window.FindName("mainGrid");
			_imageScrollBar = (ScrollBar)window.FindName("imageScrollBar");
			_progressScrollBar = (ScrollBar)window.FindName("progressScrollBar");
			_rotateTransform = (RotateTransform)window.FindName("displayRotateTransform");
			_imageTranslateTransform = (TranslateTransform)window.FindName("imageTranslateTransform");

			_comicImage = (Image)window.FindName("comicImage");

			/*
			// NOTE: keep this code as an example on how to monitor property changes
			// This code is required because WPF Image control does not have a ImageLoaded event.
			DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(Image.SourceProperty, typeof(Image));
			dpd.AddValueChanged(_comicImage, OnImageChanged);
			*/

			_imageViewbox = (Viewbox)window.FindName("imageViewbox");
			_imageViewbox.SizeChanged += OnImageViewboxSizeChanged;

			_pixelMatrix = PresentationSource.FromVisual(_window).CompositionTarget.TransformToDevice;

			_portraitStoryboard = (Storyboard)_window.TryFindResource("portraitStoryboard");
			_landscapeStoryboard = (Storyboard)_window.TryFindResource("landscapeStoryboard");
			_portraitStoryboard.Completed += OnRotationStoryboardCompleted;
			_landscapeStoryboard.Completed += OnRotationStoryboardCompleted;

			_translateXStoryboard = (Storyboard)_window.TryFindResource("translateXStoryboard");
			_translateYStoryboard = (Storyboard)_window.TryFindResource("translateYStoryboard");
			_translateXDoubleAnimation = (DoubleAnimation)_translateXStoryboard.Children[0];
			_translateYDoubleAnimation = (DoubleAnimation)_translateYStoryboard.Children[0];
			
			// Assumes both X and Y double animation have the same duration
			TRANSLATE_DURATION = _translateXDoubleAnimation.Duration;

			_numberingContainer = (Border)window.FindName("numberingBorder");

			_imageScrollBarAnimation = new DoubleAnimation();
			_imageScrollBarAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));
			_imageScrollBarAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };


			//TODO: add gesture handling
			/*_window.PreviewMouseMove += _window_MouseMove;*/
		}

		#region Event Handlers

		private void OnRotationStoryboardCompleted(object sender, EventArgs e)
		{
			UpdateZoomState();
			ToggleScrollBar(true);

			_progressScrollBar.Visibility = Visibility.Visible;

			//TODO: animate to visibility
			_numberingContainer.Visibility = Visibility.Visible;
			
			if (RotationCompleted != null)
			{
				RotationCompleted();
			}
		}

		void OnImageViewboxSizeChanged(object sender, EventArgs e)
		{
			ResetScrollBar();

			// TODO: This is currently the only place when the scrollbar direction can be set. This is due to that
			//       the actual size of the image and containers are not set when an image is loaded. There is no ImageLoaded event.
			//       The side-effect of this hack is that when the display is rotating, the image size is adjusted and this code
			//       makes the image scrollbars visible when it should be hidden during rotation.
			ToggleScrollBar(true);
		}

		//TODO: add gesture handling
		//NOTE: does not work because the mouse reaches the end of the screen and stops.
		/*
		void _window_MouseMove(object sender, MouseEventArgs e)
		{
			if (!(IsImageZoomed()))
			{
				var startPoint = _lastPoint;
				var endPoint = e.GetPosition(_window);
				var deltaPoint = endPoint - startPoint;
				
				Debug.WriteLine("length: " + deltaPoint.Length);
				if (deltaPoint.Length > 30)
				{
					Direction directionX = Direction.Unknown;
					if (deltaPoint.X > 0)
					{
						directionX = Direction.Right;
					}
					else if (deltaPoint.X < 0)
					{
						directionX = Direction.Left;
					}

					Direction directionY = Direction.Unknown;
					if (deltaPoint.Y > 0)
					{
						directionY = Direction.Down;
					}
					else if (deltaPoint.Y < 0)
					{
						directionY = Direction.Up;
					}

					if (this.MouseDirection != null)
					{
						this.MouseDirection(directionX, directionY);
					}
				}
			}
		}
		*/

		#endregion

		#region Private Methods

		/// <summary>
		///		Converts WPF independent units to Pixel dependent units
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private Point TransformToPixelCoordinates(double x, double y)
		{
			return _pixelMatrix.Transform(new Point(x, y));
		}

		/// <summary>
		///		
		///		<remarks>
		///			- Does not check if the image is currently zoomed.
		///			- Only works if the imageViewbox and relevant controls have been loaded and measured by WPF system
		///		</remarks>
		/// </summary>
		/// <returns></returns>
		private bool IsPanningVertical()
		{
			return  Math.Floor(_imageViewbox.ActualHeight) > Math.Floor(_mainContainer.ActualHeight);
		}

		/// <summary>
		/// Set values for calculation based on the display rotate. Depending on the rotation,
		/// either calculate the width offset or the height. This assumes that the image is stretched using
		/// UniformToFill.
		/// </summary>
		/// <param name="clientDistance"></param>
		/// <param name="containerDistance"></param>
		/// <param name="imageDistance"></param>
		/// <param name="translateDistance"></param>
		/// <param name="ratio"></param>
		private void GetDistancesBasedOnRotation(
			out double clientDistance,
			out double containerDistance,
			out double imageDistance,
			out double translateDistance,
			out double ratio)
		{
			if (this.IsPanningVertical())
			{
				clientDistance = _mainContainer.ActualHeight;
				containerDistance = _imageViewbox.ActualHeight;
				imageDistance = _comicImage.ActualHeight;
				translateDistance = _imageTranslateTransform.Y;
			}
			else
			{
				clientDistance = _mainContainer.ActualWidth;
				containerDistance = _imageViewbox.ActualWidth;
				imageDistance = _comicImage.ActualWidth;
				translateDistance = _imageTranslateTransform.X;
			}

			// Ratio of original image squeezed into its Viewbox.
			// Ratio is required because Translate RenderTransform is based on the original image size
			ratio = imageDistance / containerDistance;
		}

		private double GetZoomRatio()
		{
			var clientDistance = 0.0;
			var containerDistance = 0.0;
			var imageDistance = 0.0;
			var translateDistance = 0.0;
			double ratio = 0.0;

			GetDistancesBasedOnRotation(out clientDistance, out containerDistance, out imageDistance, out translateDistance, out ratio);

			return ratio;
		}

		private void SetImageTranslateX(double x, Duration duration)
		{
			UpdateImageScrollBar(x);

			_translateXDoubleAnimation.Duration = duration;
			_translateXDoubleAnimation.To = x;
			_translateXStoryboard.Begin();
		}

		private void SetImageTranslateY(double y, Duration duration)
		{
			UpdateImageScrollBar(y);

			_translateYDoubleAnimation.Duration = duration;
			_translateYDoubleAnimation.To = y;
			_translateYStoryboard.Begin();
		}

		private void ResetImageTransformations()
		{
			SetImageTranslateX(0.0, TRANSLATE_DURATION);
			SetImageTranslateY(0.0, TRANSLATE_DURATION);
		}

		private void ResetScrollBar()
		{
			if (IsImageZoomed())
			{
				var clientDistance = 0.0;
				var containerDistance = 0.0;
				var imageDistance = 0.0;
				var translateDistance = 0.0;
				var ratio = 0.0;

				GetDistancesBasedOnRotation(out clientDistance, out containerDistance, out imageDistance, out translateDistance, out ratio);

				var maximum = (containerDistance - clientDistance) * ratio;
				_imageScrollBar.Maximum = maximum;
			}
		}

		private void ToggleScrollBar(bool scrollBarIsShown)
		{
			if (scrollBarIsShown)
			{
				if (IsImageZoomed())
				{
					if (IsPanningVertical())
					{
						_imageScrollBar.Orientation = Orientation.Vertical;
					}
					else
					{
						_imageScrollBar.Orientation = Orientation.Horizontal;
					}

					_imageScrollBar.Visibility = Visibility.Visible;
				}
			}
			else
			{
				_imageScrollBar.Visibility = Visibility.Hidden;
			}
		}

		private void UpdateImageScrollBar(double value)
		{
			//TODO: tie this animation into the Translate RenderTransform properties changes
			_imageScrollBarAnimation.From = _imageScrollBar.Value;
			_imageScrollBarAnimation.To = Math.Abs(value);
			_imageScrollBar.BeginAnimation(ScrollBar.ValueProperty, _imageScrollBarAnimation);

			//_imageScrollBar.Value = Math.Abs(value);
		}

		private void UpdateProgressScrollBar(double value)
		{
			_imageScrollBarAnimation.From = _progressScrollBar.Value;
			_imageScrollBarAnimation.To = value;
			_progressScrollBar.BeginAnimation(ScrollBar.ValueProperty, _imageScrollBarAnimation);
		}

		#endregion

		#region Public Methods

		public void ToggleScreenSize()
		{
			if (_window.WindowStyle == System.Windows.WindowStyle.None)
			{
				_window.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
				_window.WindowState = System.Windows.WindowState.Normal;
				_window.Cursor = Cursors.Arrow;
			}
			else
			{
				_window.WindowStyle = System.Windows.WindowStyle.None;
				_window.WindowState = System.Windows.WindowState.Maximized;
				_window.Cursor = Cursors.None;
			}

			UpdateZoomState();
		}

		public void ResetMouse()
		{
			SetCursorPos((int)(_window.ActualWidth / 2), (int)(_window.ActualHeight / 2));
		}

		public bool IsDisplayInPortraitMode()
		{
			return (Math.Floor(_rotateTransform.Angle) == 0);
		}

		/// <summary>
		///  Default display is rotated to lanscape (90 degrees on its side)
		/// </summary>
		public void RotateDisplay(bool rotateFromDefault)
		{
			Storyboard rotateStoryboard;
			if (rotateFromDefault)
			{
				rotateStoryboard = _portraitStoryboard;
			}
			else
			{
				rotateStoryboard = _landscapeStoryboard;
			}

			// The numbering UI does not move smoothly during rotation.
			// Hide it here and then reveal it once the storyboard has completed
			_numberingContainer.Visibility = Visibility.Hidden;
			_progressScrollBar.Visibility = Visibility.Hidden;
			ToggleScrollBar(false);

			rotateStoryboard.Begin();
		}

		public void ToggleRotation()
		{
			RotateDisplay(!(IsDisplayInPortraitMode()));
		}

		public bool IsImageZoomed()
		{
			return (_imageViewbox.Stretch != Stretch.Uniform);
		}

		public double GetImageScalePercentage()
		{
			double scalePercentage = (_imageViewbox.ActualWidth * _imageViewbox.ActualHeight) / (_comicImage.ActualWidth * _comicImage.ActualHeight);
			return scalePercentage;
		}

		public void UpdateZoomState()
		{	
			if (IsImageZoomed())
			{
				ResetImageTransformations();
				var x = 0.0;
				var y = 0.0;

				if (IsPanningVertical())
				{
					x = _window.Left;
					y = _window.ActualHeight;
				}
				else
				{
					x = _window.ActualWidth;
					y = _window.ActualHeight;
				}

				var correctedPoint = _pixelMatrix.Transform(new Point(x, y));
				SetCursorPos((int)(correctedPoint.X), (int)(correctedPoint.Y));
			}
		}

		/// <summary>
		///		Either zooms in or scales out to fit the image
		/// </summary>
		/// <param name="zoomIsForced">
		///		Sets the zoom manually 
		/// </param>
		public void ToggleZoom(bool? zoomIsForced = null)
		{
			bool zoomIsToggled = true;
			if (zoomIsForced.HasValue)
			{
				// TODO: too stoned to write this in a succint manner
				if (zoomIsForced.Value)
				{
					zoomIsToggled = !(IsImageZoomed());
				}
				else
				{
					zoomIsToggled = IsImageZoomed();
				}
			}

			if (zoomIsToggled)
			{
				if (IsImageZoomed())
				{
					ResetImageTransformations();

					ToggleScrollBar(false);

					_imageViewbox.Stretch = Stretch.Uniform;
					_imageViewbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					_imageViewbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				}
				else
				{
					_imageViewbox.Stretch = Stretch.UniformToFill;
					_imageViewbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
					_imageViewbox.VerticalAlignment = System.Windows.VerticalAlignment.Top;

					UpdateZoomState();

					ToggleScrollBar(true);
				}

				if (this.ZoomCompleted != null)
				{
					this.ZoomCompleted();
				}
			}
		}

		/// <summary>
		///		
		/// </summary>
		/// <param name="direction"></param>
		public void PanZoomedView(Direction direction)
		{
			if (this.IsImageZoomed() && (direction != Direction.Unknown))
			{
				// TODO:  consolidate this method code with its overload

				var clientDistance = 0.0;
				var containerDistance = 0.0;
				var imageDistance = 0.0;
				var translateDistance = 0.0;
				var ratio = 0.0;
				Action<double> setFunction;

				GetDistancesBasedOnRotation(out clientDistance, out containerDistance, out imageDistance, out translateDistance, out ratio);

				if (!(this.IsPanningVertical()))
				{
					// Note: ratio the variable is captured not the value. This function will use the value of 'ratio' set further below in code
					setFunction = (translateOffset) =>
					{
						SetImageTranslateX(ratio * translateOffset * (-1), TRANSLATE_DURATION);
					};
				}
				else
				{
					// Note: ratio the variable is captured not the value. This function will use the value of 'ratio' set further below in code
					setFunction = (translateOffset) =>
					{
						SetImageTranslateY(ratio * translateOffset * (-1), TRANSLATE_DURATION);
					};
				}

				var offset = Math.Abs(translateDistance / ratio);
				var maximumOffset = (containerDistance - clientDistance);
				var calculatedOffset = (clientDistance * PAN_RATIO);
				double finalOffset = 0.0;
				switch (direction)
				{
					case Direction.Down:
					case Direction.Right:
						finalOffset = offset + calculatedOffset;
						if (finalOffset > maximumOffset)
						{
							finalOffset = maximumOffset;
						}

						setFunction(finalOffset);
						break;

					case Direction.Up:
					case Direction.Left:
						finalOffset = offset - calculatedOffset;
						if (finalOffset < 0)
						{
							finalOffset = 0;
						}

						setFunction(finalOffset);
						break;
				}
			}
		}

		public void PanZoomedView(Point point)
		{
			// This check is required because this code is normally called from a MouseMove event
			// and the MouseMove event fires if an underlying control moves.
			if (_lastPoint != point)
			{
				_lastPoint = point;

				// Move zoomed image around
				if (IsImageZoomed())
				{

					var windowWidth = _window.ActualWidth;
					var windowHeight = _window.ActualHeight;
					var clientWidth = _mainContainer.ActualWidth;
					var clientHeight = _mainContainer.ActualHeight;

					// Swap height and width if the display is rotated
					if (!(IsDisplayInPortraitMode()))
					//if (this.IsPanningVertical())
					{
						Swap(ref point);
						Swap(ref windowWidth, ref windowHeight);
					}

					// values are set to -0.00001 as a flag to determine whether the x or y translation is needed
					var sourceX = -0.00001;
					var sourceY = -0.00001;
					if (windowWidth < _imageViewbox.ActualWidth)
					{
						var imageWidth = _imageViewbox.ActualWidth;

						// Ratio of the original image source and the current viewbox size is required because the Translate transform
						// works of the original image source.
						var ratioX = _comicImage.ActualWidth / imageWidth;
						sourceX = (1 - (point.X / clientWidth)) * (ratioX * (imageWidth - clientWidth));
					}

					if (windowHeight < _imageViewbox.ActualHeight)
					{
						var imageHeight = _imageViewbox.ActualHeight;

						// Ratio of the original image source and the current viewbox size is required because the Translate transform
						// works of the original image source.
						var ratioY = _comicImage.ActualHeight / imageHeight;
						sourceY = (1 - (point.Y / clientHeight)) * (ratioY * (imageHeight - clientHeight));
					}


					// By checking sourceX and sourceY, assumption is made that stretch is set to UniformToFill
					// where either sourceX or sourceY must be 0
					if (sourceX > 0)
					{
						SetImageTranslateX(sourceX * -1, ZERO_DURATION);
					}
					if (sourceY > 0)
					{
						SetImageTranslateY(sourceY * -1, ZERO_DURATION);
					}
				}
			}
		}

		public void OnImageLoaded(int index, int count)
		{
			_progressScrollBar.Maximum = count - 1;
			this.UpdateProgressScrollBar(index);

			this.UpdateZoomState();
		}

		#endregion
	}
}
