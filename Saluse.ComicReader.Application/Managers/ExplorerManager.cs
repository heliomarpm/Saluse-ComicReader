using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader.Application.Managers
{
	class ExplorerManager
	{
		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		#region Constants

		// This flag selects a new item in Explorer but keeps the original item with focus
		int EXPLORER_DWFLAG = 5;

		#endregion

		#region Private Variables

		private string _workingDirectory;
		private InternetExplorer _explorerWindow = null;
		private string _filenameToSet = string.Empty;

		#endregion

		public ExplorerManager(string startPath)
		{
            if (!string.IsNullOrEmpty(startPath))
            {
                _workingDirectory = Path.GetDirectoryName(startPath);
                var shellWindows = new ShellWindows();
                foreach (InternetExplorer window in shellWindows)
                {
                    string explorerLocationPath = new Uri(window.LocationURL).LocalPath;
                    if (_workingDirectory.Equals(explorerLocationPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _explorerWindow = window;
                        break;
                    }
                }
            }
		}

		void ExplorerWindow_DocumentComplete(object pDisp, ref object URL)
		{
			// User manual change of folder in Explore will also fire this event.
			_explorerWindow.DocumentComplete -= ExplorerWindow_DocumentComplete;

			// This check is required because if the user changes directory in Explorer, this event will still fire
			if (!(string.IsNullOrEmpty(_filenameToSet)))
			{

				SetExplorerSelection(_filenameToSet);
				_filenameToSet = string.Empty;
			}
		}

		private void SetExplorerSelection(string filename)
		{
			// Once Explorer navigates to a folder, the Document has to be recaptured (each folder view has its own handle)
			var explorerDocument = (IShellFolderViewDual2)_explorerWindow.Document;
			var folderItems = explorerDocument.Folder.Items();
			for (int index = 0; index < folderItems.Count; index++)
			{
				var fileItem = folderItems.Item(index);
				if (filename.Equals(fileItem.Name, StringComparison.InvariantCultureIgnoreCase))
				{
					explorerDocument.SelectItem(fileItem, EXPLORER_DWFLAG);
					break;
				}
			}
		}

		public void SyncExplorer(string fullPath)
		{
			if ((_explorerWindow != null)  && (string.IsNullOrEmpty(_filenameToSet)))
			{
				var filenameOnly = Path.GetFileName(fullPath);
				var requestedFolderPath = Path.GetDirectoryName(fullPath);
				var currentExplorerLocation = new Uri(_explorerWindow.LocationURL).LocalPath;

				// Check if Explorer is already in the correct folder
				if (requestedFolderPath.Equals(currentExplorerLocation, StringComparison.InvariantCultureIgnoreCase))
				{
					SetExplorerSelection(filenameOnly);
				}
				else
				{
					// Have to wait for the folder (document) to be loaded in Explorer before accessing any of its functions
					_filenameToSet = filenameOnly;
					_explorerWindow.DocumentComplete += ExplorerWindow_DocumentComplete;
					_explorerWindow.Navigate2(requestedFolderPath);
					// task is completed in ExplorerWindow_DocumentComplete
				}
			}
		}

	}
}
