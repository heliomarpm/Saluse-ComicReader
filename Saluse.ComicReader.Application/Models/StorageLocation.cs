using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader.Application.Models
{
	[Serializable]
	public class StorageLocation
	{
		public string Location { get; set; }
		public int Index { get; set; }
		public long LastReadTicks { get; set; }
		public bool Rotated { get; set; }

		public StorageLocation()
		{
		}

		public StorageLocation(string location, int index)
		{
			this.Location = location;
			this.Index = index;
			this.LastReadTicks = DateTime.Now.Ticks;
			this.Rotated = false;
		}
	}
}
