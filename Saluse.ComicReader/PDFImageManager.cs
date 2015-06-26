using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader
{
	internal class PDFImageManager : IImageManager
	{
		internal static readonly string[] SUPPORTED_FILETYPES = { ".pdf" };
		private readonly int DPIX = 96;
		private readonly int DPIY = 96;
		private const float RESOLUTION_RATIO = 2.0f;

		private object _lock = new object();
		private string _filePath;
		private int _imageCount = 0;
		private GhostscriptRasterizer _ghostscriptRasterizer;

		public PDFImageManager(string filePath)
		{
			_filePath = filePath;
			var fileExtension = Path.GetExtension(filePath).ToLower();

			GhostscriptVersionInfo ghostscriptVersionInfo = GhostscriptVersionInfo.GetLastInstalledVersion(GhostscriptLicense.GPL, GhostscriptLicense.GPL);
			_ghostscriptRasterizer = new GhostscriptRasterizer();
			_ghostscriptRasterizer.Open(filePath, ghostscriptVersionInfo, false);
			_imageCount = _ghostscriptRasterizer.PageCount;

			var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
			DPIX = (int)(graphics.DpiX * RESOLUTION_RATIO);
			DPIY = (int)(graphics.DpiY * RESOLUTION_RATIO);
		}

		public void Dispose()
		{
			if (_ghostscriptRasterizer != null)
			{
				lock (_lock)
				{
					_ghostscriptRasterizer.Dispose();
				}
			}
		}

		public void UseImage(int index, Action<Image, string> action)
		{
			var pageIndex = index + 1;
			lock (_lock)
			{
				Image image = _ghostscriptRasterizer.GetPage(DPIX, DPIY, pageIndex);
				action(image, "Page " + pageIndex);
			}
		}

		public void UseImageStream(int index, Action<Stream, string, string> action)
		{
			this.UseImage(index, (image, filename) =>
			{
				using (var memoryStream = new MemoryStream())
				{
					image.Save(memoryStream, ImageFormat.Png);
					memoryStream.Seek(0, SeekOrigin.Begin);
					action(memoryStream, Path.ChangeExtension(filename, ".png"), this.DisplayName);
				}
			});
		}

		public string Location
		{
			get
			{
				return _filePath;
			}
		}

		public string DisplayName
		{
			get
			{
				return Utility.PrettifyName(Path.GetFileNameWithoutExtension(_filePath));
			}
		}

		public int Count
		{
			get {
				return _imageCount;
			}
		}

		public int InitialIndex
		{
			get {
				return -1;
			}
		}
	}
}
