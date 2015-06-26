//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Devibe (Less vibrance)
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);

static float saturate = 0.3;
static float3 luminaceCoeff = float3(0.3, 0.59, 0.11);

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 color = tex2D(implicitInputSampler, uv);

	float intensityf = dot(color, luminaceCoeff);
	float3 intensity = float3(intensityf, intensityf, intensityf);
	float3 saturationColor = lerp(intensity, color, saturate);
	float4 finalColor = float4(saturationColor.r, saturationColor.g, saturationColor.b, 1.0);

	// Add brightness relative to how much the colour has deviated towards grey
	// Looks similar to Photoshop's Vibrance adjustment
	float4 distanceVector = distance(saturationColor, color);
	finalColor *= 1.0 + (saturate * distanceVector);

	return finalColor;
}


