using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Saluse.ComicReader.Application.Managers
{
	public static class UtilityManager
	{
		public static Action<string> DisplayMessage;

		public static Dispatcher Dispatcher;
	}
}
