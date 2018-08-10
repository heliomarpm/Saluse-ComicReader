//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Sepia
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
	float contrast = 3.0;
	float vignetteIntensity = 1.6;
	float brightness = -0.1;
		
	float4 greyScaleColor = (color.r * 0.3) + (color.g * 0.59) + (color.b * 0.11);
	greyScaleColor.rgb = ((greyScaleColor.rgb - 0.5f) * max(contrast, 0)) + 0.5f;

	float4 sepiaColor = greyScaleColor * float4(0.9, 0.8, 0.6, 1.0);

	float2 distanceFromCentre = uv - 0.5f;
	sepiaColor = sepiaColor * (1 - dot(distanceFromCentre, distanceFromCentre) * vignetteIntensity);

	sepiaColor.rgb = sepiaColor.rgb + brightness;

	return sepiaColor;
}