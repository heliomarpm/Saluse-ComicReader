//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Test
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);

static float3 greyRatio = float3(0.3, 0.59, 0.11);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 AdjustSaturation(float4 color, float saturation)
{
		float grey = dot(color, greyRatio);
		return lerp(grey, color, saturation);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
		/*
		// Bi-directional "old school" blue
		float offset = 0.001;
		float4 pixelColor = tex2D(implicitInputSampler, uv);
		
		float2 newUV = uv;
		newUV.x += offset;
		pixelColor = (pixelColor + tex2D(implicitInputSampler, newUV)) / 2;

		newUV.x -= offset * 2;
		pixelColor = (pixelColor + tex2D(implicitInputSampler, newUV)) / 2;

		newUV.x = uv.x;
		newUV.y += offset;
		pixelColor = (pixelColor + tex2D(implicitInputSampler, newUV)) / 2;

		newUV.y -= offset * 2;
		pixelColor = (pixelColor + tex2D(implicitInputSampler, newUV)) / 2;
		*/

	/*
		// Weird color shift effect
		float offset = 0.005;
		float2 newUV = uv;

		// Set Red
		newUV.x += offset;
		newUV.y += offset;
		float4 pixelColor = tex2D(implicitInputSampler, newUV);

		// Set Green
		newUV.x = uv.x;
		newUV.y += offset;
		pixelColor.g = tex2D(implicitInputSampler, newUV).g;

		newUV.x -= offset;
		pixelColor.b = tex2D(implicitInputSampler, newUV).b;

		//pixelColor = (pixelColor + tex2D(implicitInputSampler, uv)) / 2;

		// return final pixel color
		return pixelColor;
*/

		float BloomIntensity = 1.5;
		float BaseIntensity = 1.0;
		float BloomSaturation = 1.0;
		float BaseSaturation = 1.0;

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


