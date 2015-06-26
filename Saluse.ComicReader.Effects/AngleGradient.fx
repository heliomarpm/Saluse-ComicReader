// no input texture, the output is completely generated in code
sampler2D  inputSampler : register(S0);

/// <summary>The primary color of the gradient. </summary>
/// <defaultValue>Blue</defaultValue>
float4 primaryColor : register(C0);

/// <summary>The secondary color of the gradient. </summary>
/// <defaultValue>Red</defaultValue>
float4 secondaryColor : register(C1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
		float2 centerPoint = float2(0.5, 0.5);

		float4 src= tex2D(inputSampler, uv);
		float2 p = float2(centerPoint)-uv;
		float angle = (atan2(p.x, p.y) + 3.141596) / (2 * 3.141596);
		float3 f = lerp(primaryColor.rgb, secondaryColor.rgb, angle);
		float4 color = float4(src.a < 0.01 
																? float3(0, 0, 0) // WPF uses pre-multiplied alpha everywhere internally for a number of performance reasons.
																: f, src.a < 0.01 ? 0 : 1);
		return color;
}