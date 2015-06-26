using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace Saluse.ComicReader.Effects
{
	/// <summary>
	/// <remarks>From http://dotwaywpf.codeplex.com</remarks>
	/// </summary>
	[Description("B+W Print (half-tone)")]
	[ExcludeEffect]
	public class BWHalfTone : ShaderEffect
	{
		#region Constructors

		static BWHalfTone()
		{
			_pixelShader.UriSource = Global.MakePackUri("BWHalfTone.ps");
		}

		public BWHalfTone()
		{
			this.PixelShader = _pixelShader;

			// Update each DependencyProperty that's registered with a shader register.  This
			// is needed to ensure the shader gets sent the proper default value.
			UpdateShaderValue(InputProperty);
			UpdateShaderValue(AmplitudeMapProperty);

			this.AmplitudeMap = this.CreateAmplitudeMap128();
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
				ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BWHalfTone), 0);


		public static readonly DependencyProperty AmplitudeMapProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("AmplitudeMap", typeof(BWHalfTone), 1);
		public Brush AmplitudeMap
		{
			get { return (Brush)GetValue(AmplitudeMapProperty); }
			set { SetValue(AmplitudeMapProperty, value); }
		}

		#endregion

		#region Member Data

		private static PixelShader _pixelShader = new PixelShader();

		#endregion

		private Brush CreateAmplitudeMap32()
		{
			var AMMatrixSize = 8.0;
			PixelFormat pixelFormat = PixelFormats.Gray8;
			int rawStride = (int)(AMMatrixSize * pixelFormat.BitsPerPixel) / 8;
			byte[] rawImage = new byte[] { 14, 5, 6, 9, 19, 28, 27, 24, 
									 12, 4, 1, 7, 21, 29, 32, 26, 
									 13, 3, 2, 8, 20, 30, 31, 25, 
									 16, 10, 11, 15, 17, 23, 22, 18, 
									 19, 28, 27, 24, 14, 5, 6, 9, 
									 21, 29, 32, 26, 12, 4, 1, 7, 
									 20, 30, 31, 25, 13, 3, 2, 8, 
									 17, 23, 22, 18, 16, 10, 11, 15};

			BitmapSource bitmap = BitmapSource.Create((int)AMMatrixSize, (int)AMMatrixSize, 96, 96, pixelFormat, null, rawImage, rawStride);

			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = bitmap;
			return imageBrush;
		}

		private Brush CreateAmplitudeMap128()
		{
			var AMMatrixSize = 16;
			PixelFormat pixelFormat = PixelFormats.Gray8;
			//int rawStride = (int)(AMMatrixSize * pixelFormat.BitsPerPixel) / 8;
			int rawStride = AMMatrixSize;
			byte[] rawImage = new byte[]{ 64, 58, 50, 40, 39, 49, 57, 63, 65, 71, 79, 89, 90, 80, 72, 66, 
									      59, 34, 27, 18, 17, 26, 33, 56, 70, 95, 102, 111, 112, 103, 96, 73, 
									      51, 28, 14, 5, 6, 9, 25, 48, 78, 101, 115, 124, 123, 120, 104, 81, 
									      41, 19, 12, 4, 1, 7, 24, 38, 88, 110, 117, 125, 128, 122, 105, 91, 
									      42, 20, 13, 3, 2, 8, 23, 37, 87, 109, 116, 126, 127, 121, 106, 92, 
									      52, 29, 16, 10, 11, 15, 32, 47, 77, 100, 113, 119, 118, 114, 97, 82, 
									      60, 35, 30, 21, 22, 31, 36, 55, 69, 94, 99, 108, 107, 98, 93, 74, 
									      61, 53, 45, 43, 44, 46, 54, 62, 68, 76, 84, 86, 85, 83, 75, 67, 
									      65, 71, 79, 89, 90, 80, 72, 66, 64, 58, 50, 40, 39, 49, 57, 63, 
									      70, 95, 102, 111, 112, 103, 96, 73, 59, 34, 27, 18, 17, 26, 33, 56, 
									      78, 101, 115, 124, 123, 120, 104, 81, 51, 28, 14, 5, 6, 9, 25, 48, 
									      88, 110, 117, 125, 128, 122, 105, 91, 41, 19, 12, 4, 1, 7, 24, 38, 
									      87, 109, 116, 126, 127, 121, 106, 92, 42, 20, 13, 3, 2, 8, 23, 37, 
									      77, 100, 113, 119, 118, 114, 97, 82, 52, 29, 16, 10, 11, 15, 32, 47, 
									      69, 94, 99, 108, 107, 98, 93, 74, 60, 35, 30, 21, 22, 31, 36, 55, 
									      68, 76, 84, 86, 85, 83, 75, 67, 61, 53, 45, 43, 44, 46, 54, 62};

			BitmapSource bitmap = BitmapSource.Create(AMMatrixSize, AMMatrixSize, 96, 96, pixelFormat, null, rawImage, rawStride);

			ImageBrush imageBrush = new ImageBrush();
			imageBrush.ImageSource = bitmap;
			return imageBrush;
		}

	}
}
