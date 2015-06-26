using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[Description("The '80s")]
	public class The80s : CRShaderEffect
	{
		public The80s() : base("The80s.ps")
		{
		}
	}
}
