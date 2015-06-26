using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Saluse.ComicReader.Effects
{
	[Description("Test Shader")]
	[ExcludeEffect]
	public class Test : CRShaderEffect
	{
		public Test() : base("Test.ps")
		{
		}
	}
}
