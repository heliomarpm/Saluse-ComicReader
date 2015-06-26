//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- ContrastAdjustEffect
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
		float4 pixelColor = tex2D(implicitInputSampler, uv);
		float contrast = 2.0;
		float brightness = 0.1;
		
		//contrast
		pixelColor.rgb = ((pixelColor.rgb - 0.5f) * max(contrast, 0)) + 0.5f;

		//brightness
		pixelColor.rgb = pixelColor.rgb + brightness;
		
		// return final pixel color
		return pixelColor;
}


