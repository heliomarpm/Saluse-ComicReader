using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[Description("High B+W Contrast")]
	public class ComicContrastBW : CRShaderEffect
	{
		public ComicContrastBW() : base("ComicContrastBW.ps")
		{
		}
	}
}
