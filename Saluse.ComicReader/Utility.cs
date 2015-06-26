using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Saluse.ComicReader
{
	/// <summary>
	///		Provides common functionality
	/// </summary>
	static class Utility
	{
		#region Private Classes

		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
		private static extern int StrCmpLogicalW(string psz1, string psz2);

		/// <summary>
		///		Sorts strings by treating textual numbers as integer numbers
		/// </summary>
		internal class NaturalStringSort : IComparer<string>
		{

			public int Compare(string x, string y)
			{
				return StrCmpLogicalW(x, y);
			}
		}

		#endregion

		#region private variables

		private static NaturalStringSort _naturalStringComparer;

		#endregion

		#region Constructors
		
		static Utility()
		{
			_naturalStringComparer = new NaturalStringSort();
		}

		#endregion

		#region Public Methods

		public static string PrettifyName(string resource)
		{
			return resource.Replace('_', ' ').Replace('.', ' ');
		}
		
		/// <summary>
		///		Returns an instantiated Natural String Comparer (implements IComparer<string>)
		/// </summary>
		public static NaturalStringSort NaturalStringComparer
		{
			get
			{
				return _naturalStringComparer;
			}
		}

		#endregion
	}
}
