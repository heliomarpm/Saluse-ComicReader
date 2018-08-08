using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader
{
	internal class CompressedFileImageManager : IImageManager
	{

		internal static readonly string[] SUPPORTED_FILETYPES = { ".cbr", ".rar", ".cbz", ".zip" };

		//TODO: move image capabilities to a separate class/utilities as this is based on .NET and not the actual folder/file structures
		private static readonly string[] SUPPORTED_IMAGETYPES = FolderImageManager.SUPPORTED_FILETYPES;

        private IArchive _archive;
		private IList<IArchiveEntry> _entries;
		private string _filePath;

		public CompressedFileImageManager(string filePath)
		{
			_filePath = filePath;
			var fileExtension = Path.GetExtension(filePath).ToLower();
			_archive = ArchiveFactory.Open(filePath);

			LoadEntries();
		}

		~CompressedFileImageManager()
		{
			if (_archive != null)
			{
				_archive.Dispose();
			}
		}

		public void Dispose()
		{
			if (_archive != null)
			{
				_archive.Dispose();
			}
		}

		private void LoadEntries()
		{
			// Entries are stored linearly (not hierarchical).
			// Entries are sorted by natural sort where string numbers in the filename are treated as integer numbers
			_entries = _archive.Entries
				.Where(x => !x.IsDirectory)
				.Where(x => SUPPORTED_IMAGETYPES.Contains(Path.GetExtension(x.Key).ToLower()))
				.OrderBy(x => x.Key, Utility.NaturalStringComparer)
				.ToArray();
		}

		/// <summary>
		///		Initial index is never set
		/// </summary>
		public int InitialIndex
		{
			get
			{
				return -1;
			}
		}

		public int Count
		{
			get
			{
				return _entries.Count;
			}
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
			var entry = _entries[index];
			using (var stream = entry.OpenEntryStream())
			{
				action(stream, Path.GetFileName(entry.Key), this.DisplayName);
			}
		}
	}
}
