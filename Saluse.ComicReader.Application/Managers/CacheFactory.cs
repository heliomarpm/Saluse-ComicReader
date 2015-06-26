using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrawingImage = System.Drawing.Image;

namespace Saluse.ComicReader.Application.Managers
{
	/// <summary>
	///		Factory for the creation and deletion of CacheManagers
	/// </summary>
	internal static class CacheFactory
	{
		public delegate void CacheProgressCallback(int index, double progress);

		/// <summary>
		///		Returns an appropriate CacheManager
		/// </summary>
		/// <param name="imageManager"></param>
		/// <param name="initialIndex"></param>
		/// <param name="cacheProgressCallback"></param>
		/// <returns></returns>
		public static ICacheManager GetCacheManager(IImageManager imageManager, int initialIndex, CacheProgressCallback cacheProgressCallback = null)
		{
			//return new NullCacheManager(imageManager, initialIndex, cacheProgressCallback);

			//return new FullCacheManager(imageManager, initialIndex, cacheProgressCallback);

			return new SlidingCacheManager(imageManager, initialIndex, cacheProgressCallback);
		}

		/// <summary>
		///		Disposes of the cachemanager and all of its resources
		/// </summary>
		/// <param name="cacheManager"></param>
		public static void DestoryCacheManager(ICacheManager cacheManager)
		{
			if (cacheManager != null)
			{
				cacheManager.Dispose();
			}
		}

		/// <summary>
		/// Converts a System.Drawing.Image to a BitmapImage which is compatible
		/// with WPF
		/// </summary>
		/// <param name="image">The image.</param>
		/// <returns></returns>
		//internal static BitmapSource GetColourCorrectedImageSource(DrawingImage image)
		internal static BitmapSource GetBitmapImage2(DrawingImage image)
		{
			var memoryStream = new MemoryStream();
			image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			var frame = BitmapFrame.Create(memoryStream);

			// Windows ICC profile location: C:\Windows\System32\spool\drivers\color
			// Adobe ICC profiles: C:\Program Files (x86)\Common Files\Adobe\Color
			var iccProfilePath = @"C:\Program Files (x86)\Common Files\Adobe\Color\MPProfiles\FilmTheaterK2395PD.icc";
			Uri destinationProfileUri = new Uri(iccProfilePath);
			ColorContext scc = new ColorContext(destinationProfileUri);

			// Using frame.Format is the same as loading the sRGB default
			ColorContext dcc = new ColorContext(frame.Format);
			//ColorContext scc = new ColorContext(new Uri(@"C:\Windows\System32\spool\drivers\color\sRGB Color Space Profile.icm"));

			// NOTE: the source and destination colorcontexes seem conceptually reversed.
			var colorConvertedBitmap = new ColorConvertedBitmap((BitmapSource)frame, scc, dcc, frame.Format);
			colorConvertedBitmap.Freeze();

			return colorConvertedBitmap;
		}

		/// <summary>
		/// Converts a System.Drawing.Image to a BitmapImage which is compatible
		/// with WPF
		/// </summary>
		/// <param name="image">The image.</param>
		/// <returns></returns>
		internal static BitmapImage GetBitmapImage(DrawingImage image)
		{
			BitmapImage bitmapImage = new BitmapImage();
			var memoryStream = new MemoryStream();
			image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

			bitmapImage.BeginInit();
			bitmapImage.CacheOption = BitmapCacheOption.OnDemand;
			bitmapImage.StreamSource = memoryStream;
			bitmapImage.EndInit();

			// Requires Freeze to be allowed to be accessed by WPF system as this BitmapImage may be created on a separate thread
			// such as the Look ahead cache filler.
			bitmapImage.Freeze();

			//TODO: check if image handles are kept opened.
			//NOTE: Disposing of image does not work with PDFImageManager
			//image.Dispose();

			return bitmapImage;
		}
	}
}
