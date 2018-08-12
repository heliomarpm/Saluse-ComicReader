
using Saluse.ComicReader.Application.Managers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using IOPath = System.IO.Path;
using System.Windows.Media;
using System.Windows.Controls;
using Saluse.ComicReader.Application.Models;

namespace Saluse.ComicReader.Application
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Private Constants

		private readonly string SNAPSHOT_PATH = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Comic Reader Snapshots");
		
		#endregion
		
		#region Private Variables

		private Engine _engine;
		private IImageManager _imageManager;
		private ICacheManager _cacheManager = null;
		private StorageManager _storageManager = null;
		private EffectManager _effectManager = null;
		private ViewManager _viewManager = null;
		private ExplorerManager _explorerManager = null;
		private SpeechManager _speechManager = null;
		private string _filePath = string.Empty;
		private Storyboard _messageStoryboard = null;
		private bool _informationIsShown = false;

		private InformationModel _informationModel = new InformationModel();

		#endregion

		#region Constructors
		
		public MainWindow()
		{
			_engine = new Engine();

			InitializeComponent();
		}

		#endregion

		#region Private Methods

		/// <summary>
		///		Certain objects call only be accessed once the current Window has loaded
		/// </summary>
		private void InitialiseWPFSystem()
		{
			_viewManager = new ViewManager(this);
			_viewManager.RotationCompleted += OnViewChangeCompleted;
			_viewManager.ZoomCompleted += OnViewChangeCompleted;
			
			// TODO: add gesture handling at a later date
			/*_viewManager.MouseDirection += _viewManager_MouseDirection;*/
		}

		/*
		void _viewManager_MouseDirection(ViewManager.Direction directionX, ViewManager.Direction directionY)
		{
			Debug.WriteLine(string.Format("X: {0} | Y {1}", directionX, directionY));

			if (IsComicLoaded())
			{
				if (directionX == ViewManager.Direction.Left || directionY == ViewManager.Direction.Down)
				{
					LoadNextImage();
				}
				else if (directionX == ViewManager.Direction.Right || directionY == ViewManager.Direction.Up)
				{
					LoadPreviousImage();
				}
			}
		}
		*/
		
		private bool IsComicLoaded()
		{
			if (_imageManager == null)
			{
				return false;
			}

			return (_imageManager.Count > 0);
		}

		private void PersistStorage()
		{
			var storageLocation = _storageManager.GetStorageLocation(_cacheManager.Location);
			storageLocation.Index = _cacheManager.CurrentIndex;
			storageLocation.Rotated = _viewManager.IsDisplayInPortraitMode();

			// Store the recent files and indexes
			_storageManager.Save();
		}

		private void SnapshotCurrentImage()
		{
			if (IsComicLoaded())
			{
				_cacheManager.UseCurrentImageStream((stream, imageName, containerName) =>
				{
                    var fileName = containerName + " - " + imageName;
                    if (!(Directory.Exists(SNAPSHOT_PATH)))
                    {
                        Directory.CreateDirectory(SNAPSHOT_PATH);
                    }

                    var shaderEffect = imageViewbox.Effect;
                    // If no effect is applied, then save source image directly to snapshot folder
                    // otherwise, use image with effect applied and save as jpeg to snapshot folder
                    if (shaderEffect == null)
                    {
                        var filePath = IOPath.Combine(SNAPSHOT_PATH, fileName);

                        // No shader effects are applied so write the source image stream directly to file
                        using (var fileStream = File.OpenWrite(filePath))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                    else
                    {
                        //TODO: this code does not apply the effect when saving. Investigage why
                        var imageSource = (BitmapSource)comicImage.Source;

                        // Create a Rectangle shape, fill its background with the current comic image
                        // and apply the same shader effect
                        var rectangle = new System.Windows.Shapes.Rectangle();
                        rectangle.Fill = new ImageBrush(imageSource);
                        rectangle.Effect = shaderEffect;

                        // Resize the Rectangle to full image size
                        var size = new Size(imageSource.PixelWidth, imageSource.PixelHeight);
                        rectangle.Measure(size);
                        rectangle.Arrange(new Rect(size));

                        //TODO: 96 is hardcoded for my monitor. Make it device dependent
                        //      or find a different method to save effect on image
                        var renderTargetBitmap = new RenderTargetBitmap(
                            imageSource.PixelWidth,
                            imageSource.PixelHeight,
                            96,
                            96,
                            PixelFormats.Pbgra32);

                        renderTargetBitmap.Render(rectangle);

                        var rerenderedFilename = IOPath.GetFileNameWithoutExtension(fileName);
                        var encoder = new JpegBitmapEncoder
                        {
                            QualityLevel = 95
                        };

                        var bitmapFrame = BitmapFrame.Create(renderTargetBitmap);
                        encoder.Frames.Add(bitmapFrame);

                        var filePath = IOPath.Combine(SNAPSHOT_PATH, rerenderedFilename + ".jpeg");
                        using (var fileStream = File.OpenWrite(filePath))
                        {
                            encoder.Save(fileStream);
                        }
                    }

                    DisplayMessage(string.Format("Snapshot '{0}'", fileName));
				});
			}
		}

		/// <summary>
		///		Calls a custom function to return a cacheitem containing Drawing Image which is then
		///		converted and loaded into WPF image control and paging is updated.
		/// </summary>
		/// <param name="getCacheItemFunction"></param>
		private void LoadImage(Func<CacheItem> getCacheItemFunction)
		{
			var cacheItem = getCacheItemFunction();
			if (cacheItem != null)
			{
				comicImage.Source = cacheItem.Image;

				if (this.IsComicLoaded())
				{
					UpdateNumbering();

					_viewManager.OnImageLoaded(cacheItem.Index, _cacheManager.Count);
					
					UpdateInformation(true);
				}
				else
				{
					numberingBorder.Visibility = System.Windows.Visibility.Hidden;
				}
			}
		}

		private void UpdateNumbering()
		{
			pageLabel.Content = string.Format("{0}", (_cacheManager.CurrentIndex + 1));
			if (numberingBorder.Visibility == System.Windows.Visibility.Hidden)
			{
				numberingBorder.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void LoadNextImage()
		{
			if (IsComicLoaded())
			{
				LoadImage(() => _cacheManager.GetNextItem());
			}
		}

		private void LoadPreviousImage()
		{
			if (IsComicLoaded())
			{
				LoadImage(() => _cacheManager.GetPreviousItem());
			}
		}

		private void OnViewChangeCompleted()
		{
			UpdateInformation();
		}

		private void DisplayComicDetails(IImageManager imageManager)
		{
			DisplayMessage(imageManager.DisplayName);
            this.Title = $"Comic Reader - {imageManager.DisplayName}";
		}

		/// <summary>
		/// Updates information.
		/// </summary>
		/// <param name="updateIsForced">If true, then the information is updated regardless if the information panel is visible or not</param>
		private void UpdateInformation(bool updateIsForced = false)
		{
			if (updateIsForced)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(_imageManager.DisplayName);
				stringBuilder.AppendLine(_cacheManager.GetItemName(_cacheManager.CurrentIndex));
				stringBuilder.AppendLine("Images:\t\t" + _cacheManager.Count);
				stringBuilder.AppendLine("Page:\t\t" + (_cacheManager.CurrentIndex + 1));

				var imageSource = (BitmapSource)comicImage.Source;
				if (imageSource == null)
				{
					stringBuilder.AppendLine("Image Size:\tn/a");
					stringBuilder.AppendLine("Image Scale:\t n/a");
				}
				else
				{
					stringBuilder.AppendLine(string.Format("Image Size:\t{0} x {1} pixels", imageSource.PixelWidth, imageSource.PixelHeight));
					stringBuilder.AppendLine(string.Format("Image Scale:\t{0:0.00} %", _viewManager.GetImageScalePercentage() * 100));
				}

				stringBuilder.AppendLine("Effect:\t\t" + _effectManager.GetEffectName((ShaderEffect)imageViewbox.Effect));

				using (var process = Process.GetCurrentProcess())
				{
					stringBuilder.AppendLine(string.Format("Cache Fill:\t{0:0.00} %", _cacheManager.GetCacheProgress() * 100));
					stringBuilder.Append(string.Format("Memory Usage:\t{0:000,000} MB", (process.PrivateMemorySize64 / 1024)));
				}

				/*informationTextBlock.Text = stringBuilder.ToString();*/
				_informationModel.CurrentPageNumber = (_cacheManager.CurrentIndex + 1);
				_informationModel.PageTotal = _cacheManager.Count;
				_informationModel.InformationText = stringBuilder.ToString();
			}
		}

		private void ToggleInformation()
		{
			if (IsComicLoaded())
			{
				_informationIsShown = !_informationIsShown;

				/*
				//HACK: Border is hidden on startup because the UI still shows even though Opacity is 0.
				if (informationBorder.Visibility == System.Windows.Visibility.Hidden)
				{
					informationBorder.Visibility = System.Windows.Visibility.Visible;
				}

				if (informationPanel.Visibility == System.Windows.Visibility.Hidden)
				{
					informationPanel.Visibility = System.Windows.Visibility.Visible;
				}
				*/

				if (_informationIsShown)
				{
					informationPanel.Visibility = System.Windows.Visibility.Visible;
				}
				else
				{
					informationPanel.Visibility = System.Windows.Visibility.Hidden;
				}

				/*
				string storyboardName;
				if (_informationIsShown)
				{
					UpdateInformation();
					storyboardName = "informationShowStoryboard";
				}
				else
				{
					storyboardName = "informationHideStoryboard";
				}

				Storyboard storyBoard = (Storyboard)TryFindResource(storyboardName);
				storyBoard.Begin();
				*/
			}
		}

		/// <summary>
		///		Syncs the Windows Explorer that first launched Comic Reader.
		///		As the user chooses different comics, Explorer selected item is kept in sync.
		/// </summary>
		/// <param name="imageManager"></param>
		private void SyncExplorer(IImageManager imageManager)
		{
			_explorerManager.SyncExplorer(imageManager.Location);
		}

		/// <summary>
		///		Fill the border around the page numbering to show cache progress
		///		<remarks>
		///			Add function to Cachemanager constructor call
		///		</remarks>
		/// </summary>
		/// <param name="index"></param>
		/// <param name="progressPercentage"></param>
		private void CacheCallback(int index, double progressPercentage)
		{
			Dispatcher.Invoke(() =>
				{
#if DEBUG
					DisplayMessage("Loaded Cache Image " + (index + 1));
#endif
					blackGradientStop.Offset = progressPercentage;
					whiteGradientStop.Offset = progressPercentage;

					UpdateInformation();
				});
		}

		private async void LoadComic(IImageManager imageManager)
		{
			CacheFactory.DestoryCacheManager(_cacheManager);

			_imageManager = imageManager;
			if (this.IsComicLoaded())
			{
				var storageLocation = _storageManager.GetStorageLocation(imageManager.Location);
				_cacheManager = CacheFactory.GetCacheManager(imageManager, storageLocation.Index, CacheCallback);

				await Dispatcher.InvokeAsync(() =>
					{
						if (storageLocation.Rotated)
						{
							if (!(_viewManager.IsDisplayInPortraitMode()))
							{
								_viewManager.RotateDisplay(true);
							}
						}
						else
						{
							if (_viewManager.IsDisplayInPortraitMode())
							{
								_viewManager.RotateDisplay(false);
							}
						}

						// Use the Image manager's initial index if it is set, otherwise use the StorageLocation's Index
						int index = imageManager.InitialIndex >= 0 ? imageManager.InitialIndex : storageLocation.Index;
						LoadImage(() => _cacheManager.GetItem(index));

						DisplayComicDetails(_imageManager);
					},
					System.Windows.Threading.DispatcherPriority.Background);
			}
			else
			{
				// Reset image
				LoadImage(() => null);

				//TODO: find a better message or not show a message at all. imageManager is null here
				//DisplayMessage(string.Format("'{0}' does not contain any images", imageManager.DisplayName));
			}
		}

		private void LoadComic(string filePath)
		{
			_imageManager = _engine.Load(filePath);
			LoadComic(_imageManager);
		}

		private void LoadDifferentComic(Func<IImageManager, IImageManager> comicLoader, string failureMessage)
		{
			if (IsComicLoaded())
			{
				var imageManager = comicLoader(_imageManager);
				if (imageManager == null)
				{
					DisplayMessage(failureMessage);
				}
				else
				{
					PersistStorage();
					LoadComic(imageManager);
					SyncExplorer(imageManager);
				}
			}
		}

		private void LoadPreviousComic()
		{
			LoadDifferentComic(_engine.GetPreviousImageManager, "No previous comics");
		}

		private void LoadNextComic()
		{
			LoadDifferentComic(_engine.GetNextImageManager, "No further comics");
		}

		private void LoadLastLoadedComic()
		{
			var lastUpdatedStorageLocation = _storageManager.GetLastUpdatedStorageLocation();			
			if (lastUpdatedStorageLocation == null)
			{
				DisplayMessage("Cannot locate last loaded file");
			}
			else
			{
				LoadComic(lastUpdatedStorageLocation.Location);
			}
		}

		private void LoadEffect(ShaderEffect shaderEffect)
		{
			imageViewbox.Effect = shaderEffect;
			DisplayMessage(_effectManager.GetEffectName(shaderEffect));
			UpdateInformation();
		}

		private void LoadNextEffect()
		{
			ShaderEffect shaderEffect = _effectManager.GetNextEffect();
			LoadEffect(shaderEffect);
		}

		private void LoadPreviousEffect()
		{
			ShaderEffect shaderEffect = _effectManager.GetPreviousEffect();
			LoadEffect(shaderEffect);
		}

		private void ResetEffect()
		{
			// Set to previous effect to facilate, switch current effect on and off.
			// If user presses "Effect Off", then when they select the "Next Effect", the effect
			// that was applied previously will be the current Effect.
			_effectManager.GetPreviousEffect();
			LoadEffect(null);
		}

		#endregion

		#region Event Handlers

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			InitialiseWPFSystem();
		}

		private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			_viewManager.ToggleZoom(e.Delta > 0);
		}

		private void Window_MouseMove(object sender, MouseEventArgs e)
		{
			_viewManager.PanZoomedView(e.GetPosition(this));
		}

		private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			LoadNextImage();
		}

		private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			LoadPreviousImage();
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (_viewManager.IsImageZoomed())
			{
				switch (e.Key)
				{
					case Key.Up:
					case Key.Down:
					case Key.Right:
					case Key.Left:
						_viewManager.PanZoomedView(ViewManager.GetDirection(e.Key));
						break;
				}
			}
		}

		private void Window_KeyUp(object sender, KeyEventArgs e)
		{
			// Holding down a key will not cause any of this code to run
			if (!e.IsRepeat)
			{
				switch (e.Key)
				{
					case Key.End:
						LoadImage(() => _cacheManager.GetLastItem());
						break;

					case Key.Home:
						LoadImage(() => _cacheManager.GetFirstItem());
						break;

					case Key.E:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
						{
							ResetEffect();
						}
						else if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
						{
							LoadPreviousEffect();
						}
						else
						{
							LoadNextEffect();
						}
						break;

					case Key.Escape:
						this.Close();
						break;

					case Key.W:
						_viewManager.ToggleScreenSize();
						break;

					case Key.B:
						LoadPreviousComic();
						break;

					case Key.N:
						LoadNextComic();
						break;

					case Key.I:
						ToggleInformation();
						break;

					case Key.R:
						_viewManager.ToggleRotation();
						break;

					case Key.S:
						SnapshotCurrentImage();
						break;

					case Key.Z:
						_viewManager.ToggleZoom();
						break;

					case Key.V:
						ToggleSpeechRecognition();
						break;

					case Key.L:
						LoadLastLoadedComic();
						break;

					case Key.Right:
					case Key.Down:
					case Key.PageDown:
						if ((e.Key == Key.Down || e.Key == Key.Right) && (_viewManager.IsImageZoomed()))
						{
							break;
						}

						LoadNextImage();
						break;

					case Key.Left:
					case Key.Up:
					case Key.PageUp:
						if ((e.Key == Key.Up || e.Key == Key.Left) && (_viewManager.IsImageZoomed()))
						{
							break;
						}

						LoadPreviousImage();
						break;

					case Key.Space:
						if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
						{
							LoadPreviousImage();
						}
						else
						{
							LoadNextImage();
						}

						break;
				}
			}
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateInformation();
		}

		private void Window_Drop(object sender, DragEventArgs e)
		{
			//TODO: encapsulate this functioanality
			var filePaths = (string[])e.Data.GetData("FileDrop");
			if (filePaths.Length > 0)
			{
				LoadComic(filePaths[0]);
			}
		}

		private void Window_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetFormats().Select(x => x.ToLower()).Contains("filedrop"))
			{
				e.Effects = DragDropEffects.Copy;
			}
			else
			{
				//TODO: this effect does not work
				e.Effects = DragDropEffects.None;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.IsComicLoaded())
			{
				PersistStorage();
			}

			_viewManager.ResetMouse();
		}

		private void ToggleSpeechRecognition()
		{
			var isRunning = _speechManager.ToogleRecognition();
			if (isRunning)
			{
				DisplayMessage("Speech recognition is running");
			}
			else
			{
				DisplayMessage("Speech recognition is off");
			}
		}

		private void SpeechCommandRecognized(string concept, string command)
		{
			if (concept.Equals("General", StringComparison.InvariantCultureIgnoreCase))
			{
				switch (command)
				{
					case "zoom":
						_viewManager.ToggleZoom();
						break;

					case "next":
						LoadNextImage();
						break;

					case "back":
						LoadPreviousImage();
						break;

					case "first":
						LoadImage(() => _cacheManager.GetFirstItem());
						break;

					case "last":
						LoadImage(() => _cacheManager.GetLastItem());
						break;

					case "window":
						_viewManager.ToggleScreenSize();
						break;

					case "rotate":
						_viewManager.ToggleRotation();
						break;

					case "info":
						ToggleInformation();
						break;
				}
			}
			else if (concept.Equals("PageGoto", StringComparison.InvariantCultureIgnoreCase))
			{
				if (int.TryParse(command, out int pageIndex))
				{
					// Voice command is 1-based, page index is 0-based
					pageIndex--;
					if (pageIndex < 0)
					{
						pageIndex = 0;
					}
					
					LoadImage(() => _cacheManager.GetItem(pageIndex));
				}
			}
		}

		#endregion

		#region Public Properties

		public InformationModel InformationViewModel
		{
			get
			{
				return _informationModel;
			}
		}

		#endregion

		#region Public Methods

		public void DisplayMessage(string message)
		{
			messageLabel.Content = message;

			//debug: for debugging release version
			//File.AppendAllText(@"c:\temp\comicreader.log", message + "\r\n");
			
			_messageStoryboard.Begin();
		}
		
		public void Initialise(string filePath)
		{
			UtilityManager.Dispatcher = Dispatcher;
			UtilityManager.DisplayMessage = this.DisplayMessage;

			_filePath = filePath;
			_messageStoryboard = (Storyboard)TryFindResource("messageStoryboard");

			// HACK: these controls must be hidden at run time but are required visible at design time
			numberingBorder.Visibility = System.Windows.Visibility.Hidden;
			imageScrollBar.Visibility = System.Windows.Visibility.Hidden;
			progressScrollBar.Opacity = 0;

			_explorerManager = new ExplorerManager(filePath);

			_speechManager = new SpeechManager()
			{
				CommandRecognized = SpeechCommandRecognized
			};

			Task.Run(() =>
				{
					_effectManager = new EffectManager();
				});

			Task.Run(() =>
				{
					_storageManager = new StorageManager();
					LoadComic(_filePath);

                    if (_imageManager == null)
                    {
                        Dispatcher.InvokeAsync(() =>
                        {
                            //TODO: move this elsewhere as it relies on knowing that the window already has a specific state/style
                            //TODO: Move to a better place than on first run. Might be the case for whenever anything fails to restore the window
                            // otherwise the user will see a fullscreen blackness
                            this.WindowStyle = WindowStyle.SingleBorderWindow;
                            this.WindowState = WindowState.Normal;
                        });
                    }
                });
		}

		#endregion
	}
}
