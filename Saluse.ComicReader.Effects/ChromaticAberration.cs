using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[Description("Chromatic Aberration")]
	[ExcludeEffect]
	// Exlucded because it is just a Barrel distortion effect at the moment
	public class ChromaticAberration : CRShaderEffect
	{
		public ChromaticAberration() : base("ChromaticAberration.ps")
		{
		}
	}
}
