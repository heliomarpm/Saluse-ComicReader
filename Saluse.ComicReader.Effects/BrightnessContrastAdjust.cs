using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[ExcludeEffect]
	public class BrightnessContrastAdjust : ShaderEffect
	{
		#region Constructors

		static BrightnessContrastAdjust()
		{
			_pixelShader.UriSource = Global.MakePackUri("BrightnessContrastAdjust.ps");
		}

		public BrightnessContrastAdjust()
		{
			this.PixelShader = _pixelShader;

			// Update each DependencyProperty that's registered with a shader register.  This
			// is needed to ensure the shader gets sent the proper default value.
			UpdateShaderValue(InputProperty);
			UpdateShaderValue(BrightnessProperty);
			UpdateShaderValue(ContrastProperty);

			this.Contrast = 1.0;
			this.Brightness = 0.1;
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
				ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BrightnessContrastAdjust), 0);

		/// <summary>
		/// Gets or sets the Brightness variable within the shader.
		/// </summary>
		public double Brightness
		{
			get { return (double)GetValue(BrightnessProperty); }
			set { SetValue(BrightnessProperty, value); }
		}

		public static readonly DependencyProperty BrightnessProperty =
				DependencyProperty.Register("Brightness", typeof(double), typeof(BrightnessContrastAdjust),
								new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));


		/// <summary>
		/// Gets or sets the Contrast variable within the shader.
		/// </summary>
		public double Contrast
		{
			get { return (double)GetValue(ContrastProperty); }
			set { SetValue(ContrastProperty, value); }
		}

		public static readonly DependencyProperty ContrastProperty =
		DependencyProperty.Register("Contrast", typeof(double), typeof(BrightnessContrastAdjust),
						new UIPropertyMetadata(0.0, PixelShaderConstantCallback(1)));

		#endregion



		#region Member Data

		private static PixelShader _pixelShader = new PixelShader();

		#endregion

	}
}
