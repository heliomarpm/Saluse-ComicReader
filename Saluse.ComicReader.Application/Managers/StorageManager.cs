using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Saluse.ComicReader.Application.Models;

namespace Saluse.ComicReader.Application.Managers
{
	class StorageManager
	{
		private Storage _storage;
		const string _storageFilePath = "readlist.txt";
		readonly string _fullStorageFilePath;

		public StorageManager()
		{
			// Ensure that the Comic Reader app data folder exists and if not, create it.
			string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string storageFolder = Path.Combine(appDataFolder, "Comic Reader");
			if (!(Directory.Exists(storageFolder)))
			{
				Directory.CreateDirectory(storageFolder);
			}

			_fullStorageFilePath = Path.Combine(storageFolder, _storageFilePath);

			_storage = this.Load();
		}

		private Storage Load()
		{
			Storage storage;
			if (File.Exists(_fullStorageFilePath))
			{
				var serializer = new XmlSerializer(typeof(Storage));
				using (var stream = File.Open(_fullStorageFilePath, FileMode.Open))
				{
					try
					{
						storage = (Storage)serializer.Deserialize(stream);
					}
					catch (InvalidOperationException)
					{
						// Sometimes the file may be corrupt or edited manually by user
						storage = new Storage();
					}
				}
			}
			else
			{
				storage = new Storage();
			}

			return storage;
		}

		public void Add(string location, int index)
		{
			var locationLowerCase = location.ToLower();
			var storageLocation = _storage.StorageLocations.Where(x => x.Location.ToLower() == locationLowerCase).FirstOrDefault();
			if (storageLocation != null)
			{
				storageLocation.Index = index;
				storageLocation.LastReadTicks = DateTime.Now.Ticks;
			}
			else
			{
				_storage.StorageLocations.Add(new StorageLocation(location, index));
			}
		}

		public StorageLocation GetStorageLocation(string location)
		{
			var locationLowerCase = location.ToLower();
			var storageLocation = _storage.StorageLocations.Where(x => x.Location.ToLower() == locationLowerCase).FirstOrDefault();
			if (storageLocation == null)
			{
				storageLocation = new StorageLocation(location, 0);
				_storage.StorageLocations.Add(storageLocation);
			}
			else
			{
				storageLocation.LastReadTicks = DateTime.Now.Ticks;
			}

			return storageLocation;
		}

		public void Save()
		{
			var serializer = new XmlSerializer(typeof(Storage));
			using (var stream = File.Open(_fullStorageFilePath, FileMode.Create))
			{
				serializer.Serialize(stream, _storage);
				stream.Close();
			}
		}

	}
}
