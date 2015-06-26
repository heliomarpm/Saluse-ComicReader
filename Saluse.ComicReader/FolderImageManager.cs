using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader
{
	internal class FolderImageManager : IImageManager
	{
		internal static readonly string[] SUPPORTED_FILETYPES = { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" };

		private string _path;
		private int _initialIndex;
		private List<FileInfo> _entries;

		public FolderImageManager(string path)
		{
			string filename = string.Empty;
			if (Directory.Exists(path))
			{
				_path = path;
				_initialIndex = -1;
			}
			else
			{
				_path = Path.GetDirectoryName(path);
				filename = Path.GetFileName(path).ToLower();
			}

			LoadEntries(filename);
		}

		private void LoadEntries(string fileName)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(_path);
			_entries = directoryInfo.GetFiles()
				.Where(x => SUPPORTED_FILETYPES.Contains(Path.GetExtension(x.Name).ToLower()))
				.OrderBy(x => x.Name, Utility.NaturalStringComparer)
				.ToList();

			if (!(string.IsNullOrWhiteSpace(fileName)))
			{
				_initialIndex = _entries.FindIndex(x => x.Name.ToLower() == fileName);
			}
		}

		public void UseImage(int index, Action<Image, string> action)
		{
			this.UseImageStream(index, (fileStream, filename, displayName) =>
				{
					var image = new Bitmap(fileStream, true);
					action(image, filename);
				});
		}

		public void UseImageStream(int index, Action<Stream, string, string> action)
		{
			var fileInfo = _entries[index];
			using (var fileStream = fileInfo.OpenRead())
			{
				action(fileStream, fileInfo.Name, this.DisplayName);
			}
		}

		public void Dispose()
		{
			// Intentionally empty
		}

		public string Location
		{
			get {
				return _path;
			}
		}

		/// <summary>
		///		Returns the Folder name
		/// </summary>
		public string DisplayName
		{
			get {
				return Utility.PrettifyName(Path.GetFileNameWithoutExtension(_path));
				}
		}

		/// <summary>
		///		Initial index is only set if the path is not a folder
		/// </summary>
		public int InitialIndex
		{
			get
			{
				return _initialIndex;
			}
		}

		public int Count
		{
			get {
				return _entries.Count;
			}
		}
	}
}
