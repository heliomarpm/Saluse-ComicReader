using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Saluse.ComicReader.Application.Models
{
	[Serializable]
	public class Storage
	{
		public List<StorageLocation> StorageLocations { get; set; }

		public Storage()
		{
			this.StorageLocations = new List<StorageLocation>();
		}
	}
}
