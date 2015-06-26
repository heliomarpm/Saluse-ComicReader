using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Saluse.ComicReader.Effects
{
	/// <summary>
	///		Based on http://stackoverflow.com/questions/20813312/anglegradient-in-wpf
	///		by Johan Larsson
	/// </summary>
	[Description("Angle Gradient")]
	[ExcludeEffect]
	public class AngleGradient : ShaderEffect
	{
		#region Constructors

		static AngleGradient()
		{
			_pixelShader.UriSource = Global.MakePackUri("AngleGradient.ps");
		}

		public AngleGradient()
		{
			this.PixelShader = _pixelShader;

			// Update each DependencyProperty that's registered with a shader register.  This
			// is needed to ensure the shader gets sent the proper default value.
			UpdateShaderValue(InputProperty);
			UpdateShaderValue(PrimaryColorProperty);
			UpdateShaderValue(SecondaryColorProperty);
		}

		#endregion

		#region Dependency Properties

		public Brush Input
		{
			get {
				Brush brush = (Brush)GetValue(InputProperty);
				return brush;
			}
			set {
				Brush brush = value;
				SetValue(InputProperty, brush);
			}
		}

		/// <summary>The primary color of the gradient. </summary>
		public Color PrimaryColor
		{
			get
			{
				return ((Color)(this.GetValue(PrimaryColorProperty)));
			}
			set
			{
				this.SetValue(PrimaryColorProperty, value);
			}
		}
		/// <summary>The secondary color of the gradient. </summary>
		public Color SecondaryColor
		{
			get
			{
				return ((Color)(this.GetValue(SecondaryColorProperty)));
			}
			set
			{
				this.SetValue(SecondaryColorProperty, value);
			}
		}

		// Brush-valued properties turn into sampler-property in the shader.
		// This helper sets "ImplicitInput" as the default, meaning the default
		// sampler is whatever the rendering of the element it's being applied to is.
		public static readonly DependencyProperty InputProperty =
				ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(AngleGradient), 0);

		public static readonly DependencyProperty PrimaryColorProperty = DependencyProperty.Register(
						"PrimaryColor",
						typeof(Color),
						typeof(AngleGradient),
						new UIPropertyMetadata(Color.FromArgb(255, 0, 0, 255), PixelShaderConstantCallback(0)));

		public static readonly DependencyProperty SecondaryColorProperty = DependencyProperty.Register(
				"SecondaryColor",
				typeof(Color),
				typeof(AngleGradient),
				new UIPropertyMetadata(Color.FromArgb(255, 255, 0, 0), PixelShaderConstantCallback(1)));

		#endregion

		#region Member Data

		private static PixelShader _pixelShader = new PixelShader();

		#endregion
	}
}
