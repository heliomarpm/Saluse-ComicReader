using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.Concurrent;
using DrawingImage = System.Drawing.Image;
using Saluse.ComicReader.Application.Models;

namespace Saluse.ComicReader.Application.Managers
{
	class FullCacheManager : ICacheManager
	{
		#region Private Variables

		private object _lock = new object();
		private Dictionary<int, CacheItem> _cache;
		private IImageManager _imageManager;
		private int _currentIndex;
		private CacheFactory.CacheProgressCallback _cacheCallback;

		// Stores the state of the look ahead cache fill array.
		private BlockingCollection<int> _fillAheadLookup;
		private CancellationTokenSource _fillAheadCancellationTokenSource;

		#endregion

		#region Constructors

		public FullCacheManager(IImageManager imageManager, int index, CacheFactory.CacheProgressCallback cacheCallback = null)
		{
			_imageManager = imageManager;
			_cache = new Dictionary<int, CacheItem>();
			_currentIndex = index;
			_cacheCallback = cacheCallback;

			if ((_imageManager != null) && (_imageManager.Count > 0))
			{
				_fillAheadLookup = new BlockingCollection<int>(_imageManager.Count);

				// Start filling the cache on a separate thread
				_fillAheadCancellationTokenSource = new CancellationTokenSource();
				Task.Run(() => FillAheadCache(), _fillAheadCancellationTokenSource.Token);
			}
		}

		#endregion

		#region Public Properties

		public int Count
		{
			get
			{
				return	_imageManager == null ? 0 :	_imageManager.Count;
			}
		}

		/// <summary>
		///		Returns the current page.
		///		<remarks>
		///			This value is 0-based.
		///		</remarks>
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return _currentIndex;
			}
		}

		public string Location
		{
			get
			{
				return _imageManager == null ? string.Empty : _imageManager.Location;
			}
		}

		#endregion

		#region Private Methods

		private void FillAheadCache()
		{
			var numbers = Enumerable.Range(0, _imageManager.Count);
			var offset = 1;

			while (_fillAheadLookup.Count < _imageManager.Count)
			{

				if (_fillAheadCancellationTokenSource.IsCancellationRequested)
				{
					break;
				}

				// Loads images alternating on either side of the current index.
				// TODO: optimise as the more images there are, the longer it takes to find the next (or previous) index
				var remainingIndices = numbers.Except(_fillAheadLookup);
				var maxIndex = remainingIndices.Max();
				var lookupIndex = _currentIndex + offset;
				while (true)
				{
					if (lookupIndex > maxIndex)
					{
						lookupIndex = remainingIndices.Last();
						offset = -1;
						break;
					}
					else if (lookupIndex < 0)
					{
						lookupIndex = remainingIndices.First();
						offset = 1;
						break;
					}
					else if (remainingIndices.Contains(lookupIndex))
					{
						// Alternate offset
						offset *= -1;
						break;
					}

					lookupIndex += offset;
				}

				this.InternalGetImage(lookupIndex);
				if (_cacheCallback != null)
				{
					_cacheCallback(lookupIndex, _fillAheadLookup.Count / (double)_imageManager.Count);
				}
			}
		}

		/// <summary>
		/// Adds the bitmap image at the specified index into the cache.
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="image">The bitmap source image</param>
		private CacheItem Add(int index, BitmapSource image, string imageName)
		{
			CacheItem cacheItem = null;
			if (_cache.ContainsKey(index))
			{
				cacheItem = _cache[index];
			}
			else
			{
				_fillAheadLookup.Add(index);
				cacheItem = new CacheItem(index, image, imageName);
				_cache.Add(index, cacheItem);
			}

			return cacheItem;
		}

		/// <summary>
		///		Gets the image by index.
		///		If not in the cache, then it is loaded via the Image Manager and then stored in the cache.
		/// </summary>
		/// <param name="index">This is a 0-based index</param>
		/// <returns></returns>
		private CacheItem InternalGetImage(int index)
		{
			CacheItem cacheItem = null;
			if (_imageManager != null)
			{
				if (_cache.ContainsKey(index))
				{
					cacheItem = _cache[index];
				}
				else
				{
					// Get Drawing.Image from the Image Manager, convert it to BitmapImage (WPF) and store in cache
					lock (_lock)
					{
						_imageManager.UseImage(
							index,
							(image, imageName) =>
							{
								var imageSource = CacheFactory.GetBitmapImage(image);
								cacheItem = this.Add(index, imageSource, imageName);
							});
					}
				}
			}

			return cacheItem;
		}

		#endregion

		#region Public Methods

		public CacheItem GetItem(int index)
		{
			_currentIndex = index;
			var cacheItem = InternalGetImage(index);
			if (cacheItem == null)
			{
				return null;
			}
			else
			{
				return cacheItem;
			}
		}

		public string GetItemName(int index)
		{
			return InternalGetImage(index).ImageName;
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
			return _cache.Count / (double)_imageManager.Count;
		}

		public void Dispose()
		{
			if (_fillAheadCancellationTokenSource != null)
			{
				_fillAheadCancellationTokenSource.Cancel();
			}
		}

		#endregion
	}
}
