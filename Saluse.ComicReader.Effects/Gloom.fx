//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Gloom
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);

// ddxUvDdyUv.x and ddxUvDdyUv.y contains the float of the next pixel (pixel width-height used for 0.0..1.0 pixel addressing)
float4 ddxUvDdyUv : register(c31);

float4 ToGreyscale(float4 color)
{
	return (color.r * 0.3) + (color.g * 0.59) + (color.b * 0.11);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 color = 0;
	float blurAmount = 2.0;
	float2 blurOffset = ddxUvDdyUv * blurAmount;

	color += tex2D(implicitInputSampler, uv);

	float2 adjacentUV1 = { uv.x - blurOffset.x, uv.y };
	color += tex2D(implicitInputSampler, adjacentUV1);

	float2 adjacentUV2 = { uv.x + blurOffset.x, uv.y };
	color += tex2D(implicitInputSampler, adjacentUV2);

	float2 adjacentUV3 = { uv.x, uv.y - blurOffset.y };
	color += tex2D(implicitInputSampler, adjacentUV3);

	float2 adjacentUV4 = { uv.x, uv.y + blurOffset.y };
	color += tex2D(implicitInputSampler, adjacentUV4);

	return (color / 5);
}


