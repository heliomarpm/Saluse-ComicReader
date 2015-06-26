using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[Description("Contrast")]
	[ExcludeEffect]
	public class ComicContrast : ShaderEffect
	{
		#region Constructors

		static ComicContrast()
		{
			_pixelShader.UriSource = Global.MakePackUri("ComicContrast.ps");
		}

		public ComicContrast()
		{
			this.PixelShader = _pixelShader;

			// Update each DependencyProperty that's registered with a shader register.  This
			// is needed to ensure the shader gets sent the proper default value.
			UpdateShaderValue(InputProperty);
		}

		#endregion

		#region Dependency Properties

		public Brush Input
		{
			get { return (Brush)GetValue(InputProperty); }
			set { SetValue(InputProperty, value); }
		}

		// Brush-valued properties turn into sampler-property in the shader.
		// This helper sets "ImplicitInput" as the default, meaning the default
		// sampler is whatever the rendering of the element it's being applied to is.
		public static readonly DependencyProperty InputProperty =
				ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(ComicContrast), 0);

		#endregion

		#region Member Data

		private static PixelShader _pixelShader = new PixelShader();

		#endregion

	}
}
