using Saluse.ComicReader.Application.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Saluse.ComicReader.Application.Managers
{
	/// <summary>
	///		Stores a limited number of images in the cache, surrounding the current image index
	/// </summary>
	class SlidingCacheManager : ICacheManager
	{
		#region Private Variables/Constants

		// 0-indexed
		private const int CACHE_LIMIT = 10;

		private readonly int _cacheCount;

		private IImageManager _imageManager;
		private CacheFactory.CacheProgressCallback _cacheCallback;
		private int _currentIndex;
		private ConcurrentDictionary<int, CacheItem> _cache;
		private ConcurrentStack<int> _fillAheadStack;

		private Task _fillAheadTask = null;
		private Task _prepareFillAheadTask = null;

		#endregion
		
		public SlidingCacheManager(IImageManager imageManager, int index, CacheFactory.CacheProgressCallback cacheCallback = null)
		{
			_imageManager = imageManager;
			_cacheCallback = cacheCallback;
			_currentIndex = index;

			_cacheCount  = _imageManager.Count < CACHE_LIMIT ? _imageManager.Count : CACHE_LIMIT;
			_cache = new ConcurrentDictionary<int, CacheItem>();
			_fillAheadStack = new ConcurrentStack<int>();

			RunPrepareFillAheadPopulation();
		}

		#region Public Properties

		public int Count
		{
			get
			{
				return _imageManager == null ? 0 : _imageManager.Count;
			}
		}

		public int CurrentIndex
		{
			get
			{
				return _currentIndex;
			}
			
			private set
			{
				_currentIndex = value;
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

		private void DisplayMessage(string message)
		{
			UtilityManager.Dispatcher.InvokeAsync(() => UtilityManager.DisplayMessage(message));
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
				cacheItem = new CacheItem(index, image, imageName);
				_cache.TryAdd(index, cacheItem);
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
					// Push the current index to top of the stack for immediate loading
					RunFillAheadCache(index);
					while (!(_cache.ContainsKey(index)))
					{
					}
					cacheItem = _cache[index];
				}
			}

			return cacheItem;
		}

		/// <summary>
		///		Returns a list of indexers based on the CurrentIndex
		///		and CacheCount
		/// </summary>
		/// <returns></returns>
		private IEnumerable<int> GetSlidingRange()
		{
			IEnumerable<int> range;
			var halfCacheCount = (int)Math.Ceiling(_cacheCount / 2d);
			var rangeStart = Math.Max(0, _currentIndex - halfCacheCount);
			var rangeEnd = (rangeStart == 0) ? _cacheCount : _currentIndex + halfCacheCount;
			var count = _cacheCount;

			if (rangeEnd > this.Count)
			{
				rangeEnd = this.Count;
				rangeStart = Math.Max(0, this.Count - _cacheCount);
			}

			count = (rangeEnd - rangeStart);
			range = Enumerable.Range(rangeStart, count);
			if (_currentIndex == 0)
			{
				//TODO: this code is for used because the items here will be pushed onto a stack
				//TODO: remove knowledge of other collection types
				range = range.Reverse();
			}
			else if (_currentIndex > rangeStart)
			{
				//TODO: optimise
				var sourceArray = range.ToArray();
				var staggeredList = new List<int>();
				int index = 0;
				int offset = 1;
				staggeredList.Add(sourceArray[_currentIndex - rangeStart]);
				while(staggeredList.Count < count)
				{
					index = _currentIndex + offset;
					if (index < rangeEnd)
					{
						staggeredList.Add(sourceArray[index - rangeStart]);
					}

					index = _currentIndex - offset;
					if (index >= rangeStart)
					{
						staggeredList.Add(sourceArray[index - rangeStart]);
					}

					offset++;
				}

				//TODO: this code is for used because the items here will be pushed onto a stack
				//TODO: remove knowledge of other collection types
				staggeredList.Reverse();
				range = staggeredList;
			}

			return range;
		}

		private void FillAheadFromStack()
		{
			int index;
			while (!(_fillAheadStack.IsEmpty))
			{
				if (_fillAheadStack.TryPop(out index))
				{
					if (!(_cache.ContainsKey(index)))
					{
#if DEBUG
						DisplayMessage(string.Format("Stack Index: {0}", index));
#endif
						_imageManager.UseImage(
							index,
							(image, imageName) =>
							{
								var imageSource = CacheFactory.GetBitmapImage(image);
								this.Add(index, imageSource, imageName);
							});
					}
				}
			}
		}
		
		private void RunFillAheadCache(int? index = null)
		{
			if (index != null)
			{
				_fillAheadStack.Push(index.Value);
			}

			if ((_fillAheadTask == null) || (_fillAheadTask.Status == TaskStatus.RanToCompletion))
			{
				_fillAheadTask = Task.Run(() =>
					{
						FillAheadFromStack();
					});
			}
		}

		private void PrepareFillAheadPopulation()
		{
			var range = GetSlidingRange();
			var itemsToRemove = _cache.Select(x => x.Key).Except(range).ToList();
			var itemsToLoad = range.Except(_cache.Select(x => x.Key));

			itemsToRemove.ForEach((index) =>
			{
				CacheItem cacheItem;
				_cache.TryRemove(index, out cacheItem);
			});

			if (itemsToLoad.Any())
			{
				_fillAheadStack.PushRange(itemsToLoad.ToArray());
				RunFillAheadCache();
			}
		}

		private void RunPrepareFillAheadPopulation()
		{
			//PrepareFillAheadPopulation();
			if ((_prepareFillAheadTask == null) || (_prepareFillAheadTask.Status == TaskStatus.RanToCompletion))
			{
				_prepareFillAheadTask = Task.Run(() => PrepareFillAheadPopulation());
			}
		}
		
		#endregion

		#region Public Methods

		public CacheItem GetItem(int index)
		{
			this.CurrentIndex = index;
			var cacheItem = InternalGetImage(index);

			RunPrepareFillAheadPopulation();

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
			var nextIndex = this.CurrentIndex + 1;
			if (nextIndex >= _imageManager.Count)
			{
				nextIndex = _imageManager.Count - 1;
			}

			return this.GetItem(nextIndex);
		}

		public CacheItem GetPreviousItem()
		{
			var previousIndex = (this.CurrentIndex - 1);
			if (previousIndex < 0)
			{
				previousIndex = 0;
			}

			return this.GetItem(previousIndex);
		}

		public CacheItem GetFirstItem()
		{
			return this.GetItem(0);
		}

		public CacheItem GetLastItem()
		{
			return this.GetItem((this.Count - 1));
		}

		public void UseCurrentImageStream(Action<Stream, string, string> action)
		{
			_imageManager.UseImageStream(this.CurrentIndex, action);
		}

		public double GetCacheProgress()
		{
			return (_cacheCount - _fillAheadStack.Count) / (double)_cacheCount;
		}

		public void Dispose()
		{
			// Intentionally blank
		}

		#endregion
	}
}
