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
	///		Interface for a generic Cache manager
	/// </summary>
	internal interface ICacheManager : IDisposable
	{
		/// <summary>
		///		The total number of images in the ImageManager.
		///		<remarks>Does not reflect the number of items in the cache</remarks>
		/// </summary>
		int Count { get; }

		/// <summary>
		///		The current image that was last requested
		/// </summary>
		int CurrentIndex { get; }

		/// <summary>
		///		The path location of the image sources
		/// </summary>
		string Location { get; }

		/// <summary>
		///		Returns a bitmap image based on positional index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		CacheItem GetItem(int index);

		/// <summary>
		///		Returns the name of the Image, if available
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		string GetItemName(int index);

		/// <summary>
		///		Gets next item based on CurrentIndex
		/// </summary>
		/// <returns></returns>
		CacheItem GetNextItem();

		/// <summary>
		///		Gets previous item based on CurrentIndex
		/// </summary>
		/// <returns></returns>
		CacheItem GetPreviousItem();

		/// <summary>
		///		Gets first item from set
		/// </summary>
		/// <returns></returns>
		CacheItem GetFirstItem();

		/// <summary>
		///		Gets last item in set
		/// </summary>
		/// <returns></returns>
		CacheItem GetLastItem();

		/// <summary>
		///		Executes a custom action using the raw image stream
		/// </summary>
		/// <param name="action"></param>
		void UseCurrentImageStream(Action<Stream, string, string> action);

		/// <summary>
		///		Returns the current state of the cache load
		/// </summary>
		/// <returns></returns>
		double GetCacheProgress();
	}
}
