using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader
{
	public interface IImageManager : IDisposable
	{
		void UseImage(int index, Action<Image, string> action);
		void UseImageStream(int index, Action<Stream, string, string> action);

		string Location { get; }
		string DisplayName { get; }
		int Count { get; }

		/// <summary>
		///		Returns the initial index for this Image manager
		///		<remarks>
		///		-1 = No initial index
		///		</remarks>
		/// </summary>
		int InitialIndex { get; }
	}
}
