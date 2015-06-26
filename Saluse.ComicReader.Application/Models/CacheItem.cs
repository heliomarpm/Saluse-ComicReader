using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Saluse.ComicReader.Application.Models
{
	internal class CacheItem
	{
		public int Index {get;set;}
		public BitmapSource Image { get; set; }
		public string ImageName { get; set; }

		public CacheItem(int index, BitmapSource image, string imageName)
		{
			this.Index = index;
			this.Image = image;
			this.ImageName = imageName;
		}

		public CacheItem()
			: this(0, null, string.Empty)
		{
		}

		public override string ToString()
		{
			return this.Index.ToString();
		}
	}
}
