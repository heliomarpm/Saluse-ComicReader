//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- ComicContrastBW
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
		float4 color = tex2D(implicitInputSampler, uv);
		float contrast = 1.8;
		
		float4 greyScaleColor = (color.r * 0.3) + (color.g * 0.59) + (color.b * 0.11);
		greyScaleColor.rgb = ((greyScaleColor.rgb + 0.5f) * max(contrast, 0)) - 0.5f;
		
		if (greyScaleColor.r > 0.9)
		{
			greyScaleColor.rgb = 1.0;
		}
		else
		{
			greyScaleColor.rgb = 0.0;
		}

		// return final pixel color
		return greyScaleColor;
}


