using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Saluse.ComicReader.Effects
{
	[ExcludeEffect]
	public class CRShaderEffect : ShaderEffect
	{
        public CRShaderEffect()
        {
            this.InitialisePixelShader(this.GetType().Name);
        }

        public CRShaderEffect(string pixelShaderSource)
		{
            this.InitialisePixelShader(pixelShaderSource);
		}

        private void InitialisePixelShader(string pixelShaderSource)
        {
            var fileExtension = Path.GetExtension(pixelShaderSource);
            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                pixelShaderSource += ".cso";
            }

            var pixelShader = new PixelShader();

            pixelShader.UriSource = Global.MakePackUri(pixelShaderSource);
            this.PixelShader = pixelShader;

            // Make available screen space deltas in the shader
            // Pixel Shader 2.0 has a maximum of 32 constant registers. This float4 value will be in register(c31)
            this.DdxUvDdyUvRegisterIndex = 31;

            // Update each DependencyProperty that's registered with a shader register.  This
            // is needed to ensure the shader gets sent the proper default value.
            UpdateShaderValue(InputProperty);
        }

		#region Dependency Properties

		public Brush Input
		{
			get
			{
				Brush brush = (Brush)GetValue(InputProperty);
				return brush;
			}
			set
			{
				Brush brush = value;
				SetValue(InputProperty, brush);
			}
		}

		// Brush-valued properties turn into sampler-property in the shader.
		// This helper sets "ImplicitInput" as the default, meaning the default
		// sampler is whatever the rendering of the element it's being applied to is.
		public static readonly DependencyProperty InputProperty =
				ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(CRShaderEffect), 0);

		#endregion
	}
}
