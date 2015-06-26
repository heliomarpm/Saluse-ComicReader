using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[Description("Devibe")]
	public class Devibe : CRShaderEffect
	{
		public Devibe() : base("Devibe.ps")
		{
		}
	}
}
