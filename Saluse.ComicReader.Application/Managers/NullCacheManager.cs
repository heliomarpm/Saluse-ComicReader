using Saluse.ComicReader.Application.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Saluse.ComicReader.Application.Managers
{
	/// <summary>
	///		Does not perform any caching
	///		TODO: make an AbstractCacheManager for common functionality
	/// </summary>
	class NullCacheManager : ICacheManager
	{
		private IImageManager _imageManager;
		private int _currentIndex;
		private CacheFactory.CacheProgressCallback _cacheCallback;

		public NullCacheManager(IImageManager imageManager, int index, CacheFactory.CacheProgressCallback cacheCallback = null)
		{
			_cacheCallback = cacheCallback;
			_currentIndex = index;
			_imageManager = imageManager;
		}

		public int Count
		{
			get { return _imageManager == null ? 0 : _imageManager.Count; }
		}

		public int CurrentIndex
		{
			get { return _currentIndex; }
		}

		public string Location
		{
			get { return _imageManager == null ? string.Empty : _imageManager.Location; }
		}

		public CacheItem GetItem(int index)
		{
			CacheItem cacheItem = null;
			_imageManager.UseImage(
							index,
							(image, imageName) =>
							{
								var bitmapImage = CacheFactory.GetBitmapImage(image);
								cacheItem = new CacheItem(index, bitmapImage, imageName);
							});

			return cacheItem;
		}

		public string GetItemName(int index)
		{
			string name = string.Empty;
			_imageManager.UseImage(
							index,
							(image, imageName) =>
							{
								name = imageName;
							});

			return name;
		}

		public CacheItem GetNextItem()
		{
			if (++_currentIndex >= _imageManager.Count)
			{
				_currentIndex = _imageManager.Count - 1;
			}

			return this.GetItem(_currentIndex);
		}

		public CacheItem GetPreviousItem()
		{
			if (--_currentIndex < 0)
			{
				_currentIndex = 0;
			}

			return this.GetItem(_currentIndex);
		}

		public CacheItem GetFirstItem()
		{
			_currentIndex = 0;
			return this.GetItem(_currentIndex);
		}

		public CacheItem GetLastItem()
		{
			_currentIndex = (_imageManager.Count - 1);
			return this.GetItem(_currentIndex);
		}

		public void UseCurrentImageStream(Action<Stream, string, string> action)
		{
			_imageManager.UseImageStream(_currentIndex, action);
		}

		public double GetCacheProgress()
		{
			return 0.0;
		}

		public void Dispose()
		{
			// Intentionally blank
		}
	}
}
