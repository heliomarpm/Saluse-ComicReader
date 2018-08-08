using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Saluse.ComicReader.Effects
{
	[Description("Animated")]
	[ExcludeEffect]
	public class Animated : CRShaderEffect
    {
		#region Constructors

		//static Animated()
		//{
		//	_pixelShader.UriSource = Global.MakePackUri("Animated.ps");
		//}

		public Animated()
		{
			//this.PixelShader = _pixelShader;

			// Update each DependencyProperty that's registered with a shader register.  This
			// is needed to ensure the shader gets sent the proper default value.
			//UpdateShaderValue(InputProperty);
			UpdateShaderValue(LocationProperty);
			UpdateShaderValue(LineWidthProperty);
			UpdateShaderValue(StrengthProperty);

			Random random = new Random();
			Timer timer = new Timer(40);
			int maxRunLength = 10;
			int counter = random.Next(maxRunLength);
			double step = 0.003;
			timer.Elapsed += (a, b) =>
				{
					Dispatcher.Invoke(() =>
						{
							if (counter > 0)
							{
								this.Location += step;
								this.Strength += step;
								counter--;
							}
							else
							{
								counter = random.Next(maxRunLength);
								//step = random.NextDouble() / 700;

								if (counter > (maxRunLength >> 1))
								{
									step *= -1;
								}

								this.Location = random.NextDouble();
								this.Strength = random.NextDouble() + 0.5;
								this.LineWidth = (random.NextDouble() / 400);
							}
						});
				};
			
			timer.Start();
		}

		#endregion

		#region Dependency Properties

		//public Brush Input
		//{
		//	get {
		//		return (Brush)GetValue(InputProperty);;
		//	}
		//	set {
		//		SetValue(InputProperty, value);
		//	}
		//}

		public double Location
		{
			get
			{
				return (double)GetValue(LocationProperty);
			}
			set
			{
				SetValue(LocationProperty, value);
			}
		}

		public double LineWidth
		{
			get
			{
				return (double)GetValue(LineWidthProperty);
			}
			set
			{
				SetValue(LineWidthProperty, value);
			}
		}

		public double Strength
		{
			get
			{
				return (double)GetValue(StrengthProperty);
			}
			set
			{
				SetValue(StrengthProperty, value);
			}
		}

		// Brush-valued properties turn into sampler-property in the shader.
		// This helper sets "ImplicitInput" as the default, meaning the default
		// sampler is whatever the rendering of the element it's being applied to is.
		//public static readonly DependencyProperty InputProperty =
		//		ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(Animated), 0);

		public static readonly DependencyProperty LocationProperty = DependencyProperty.Register(
								"Location",
								typeof(double),
								typeof(Animated),
								new UIPropertyMetadata(0.5, PixelShaderConstantCallback(0)));

		public static readonly DependencyProperty LineWidthProperty = DependencyProperty.Register(
						"LineWidth",
						typeof(double),
						typeof(Animated),
						new UIPropertyMetadata(0.0025, PixelShaderConstantCallback(1)));

		public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(
				"Strength",
				typeof(double),
				typeof(Animated),
				new UIPropertyMetadata(0.8, PixelShaderConstantCallback(2)));

		#endregion

		//#region Member Data

		//private static PixelShader _pixelShader = new PixelShader();

		//#endregion
	}
}
