//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- EmbossedEffect
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);


float4 AdjustSaturation(float4 color, float saturation)
{
		float grey = dot(color, float3(0.3, 0.59, 0.11));
		return lerp(grey, color, saturation);
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
		// Bloom

		const static float BloomIntensity = 5.0;
		const static float BaseIntensity = 1.0;
		const static float BloomSaturation = 2.0;
		const static float BaseSaturation = 5.0;
		
		float BloomThreshold = 0.25f;

		float4 base = tex2D(implicitInputSampler, uv);
		float4 bloom = saturate((base - BloomThreshold) / (1 - BloomThreshold));
		
		// Adjust color saturation and intensity.
		bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
		base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
		
		// Darken down the base image in areas where there is a lot of bloom,
		// to prevent things looking excessively burned-out.
		base *= (1 - saturate(bloom));
		
		// Combine the two images.
		return base + bloom;
}


