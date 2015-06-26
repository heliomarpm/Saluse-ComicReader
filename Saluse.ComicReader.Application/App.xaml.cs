using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Saluse.ComicReader.Application
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : System.Windows.Application
	{
		private void Application_Startup(object sender, StartupEventArgs e)
		{

			string filePath = string.Empty;
			if (e.Args.Length == 1)
			{
				filePath = e.Args[0];
			}

			MainWindow mainWindow = new MainWindow();

			mainWindow.Initialise(filePath);
			mainWindow.Show();
		}
	}
}
