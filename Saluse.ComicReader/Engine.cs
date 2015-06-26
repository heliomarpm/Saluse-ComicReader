using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Saluse.ComicReader
{
		public class Engine
		{
			private enum DirectionType
			{
				Previous,
				Next
			}

			#region Private Methods

			private IImageManager GetDirectionalImageManager(IImageManager imageManager, DirectionType directionType)
			{
				//TODO: Compressed and PDF files need to be able to find each other for the case of a folder containing both .pdf and .cbr

				IImageManager nextImageManager = null;
				var path = imageManager.Location;
				int directionOffset = directionType == DirectionType.Next ? 1 : -1;

				//TODO: code for CompressedFileImageManager and FolderImageManager is virtually identical. Consolidate!
				if ((imageManager is CompressedFileImageManager) || (imageManager is PDFImageManager))
				{
					var fileName = Path.GetFileName(path).ToLower();
					var fileInfo = new FileInfo(path);
					var supportFileTypes = imageManager is CompressedFileImageManager
						? CompressedFileImageManager.SUPPORTED_FILETYPES
						: PDFImageManager.SUPPORTED_FILETYPES;

					var fileInfos = fileInfo.Directory.GetFiles()
						.Where(x => supportFileTypes.Contains(x.Extension.ToLower()))
						.OrderBy(x => x.Name, Utility.NaturalStringComparer)
						.ToList();

					var foundFileIndex = fileInfos.FindIndex(x => x.Name.ToLower() == fileName) + directionOffset;
					if ((foundFileIndex < fileInfos.Count) && (foundFileIndex > -1))
					{
						imageManager.Dispose();
						nextImageManager = this.Load(fileInfos[foundFileIndex].FullName);
					}
				}
				else if (imageManager is FolderImageManager)
				{
					DirectoryInfo directoryInfo = new DirectoryInfo(path);
					var directoryName = directoryInfo.Name.ToLower();
					var directoryInfos = directoryInfo.Parent.GetDirectories()
						.OrderBy(x => x.Name, Utility.NaturalStringComparer)
						.ToList();

					var foundDirectoryIndex = directoryInfos.FindIndex(x => x.Name.ToLower() == directoryName) + directionOffset;
					if ((foundDirectoryIndex < directoryInfos.Count) && (foundDirectoryIndex > -1))
					{
						imageManager.Dispose();
						nextImageManager = this.Load(directoryInfos[foundDirectoryIndex].FullName);
					}
				}

				return nextImageManager;
			}

			#endregion

			#region Public Methods
			
			public IImageManager Load(string resource)
			{
				IImageManager imageManager = null;
				
				if (!(string.IsNullOrWhiteSpace(resource)))
				{
					// See if the resource is a Folder
					if (Directory.Exists(resource))
					{
						imageManager = new FolderImageManager(resource);
					}
					else if (File.Exists(resource))
					{
						var fileExtension = Path.GetExtension(resource).ToLower();
						if (CompressedFileImageManager.SUPPORTED_FILETYPES.Contains(fileExtension))
						{
							imageManager = new CompressedFileImageManager(resource);
						}
						else if (PDFImageManager.SUPPORTED_FILETYPES.Contains(fileExtension))
						{
							imageManager = new PDFImageManager(resource);
						}
						else if (FolderImageManager.SUPPORTED_FILETYPES.Contains(fileExtension))
						{
							imageManager = new FolderImageManager(resource);
						}
					}
				}

				return imageManager;
			}

			public IImageManager GetNextImageManager(IImageManager imageManager)
			{
				return GetDirectionalImageManager(imageManager, DirectionType.Next);
			}

			public IImageManager GetPreviousImageManager(IImageManager imageManager)
			{
				return GetDirectionalImageManager(imageManager, DirectionType.Previous);
			}
		}
		#endregion
	}
