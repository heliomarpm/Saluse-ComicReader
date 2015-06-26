//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Sharpen
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);

// ddxUvDdyUv.x and ddxUvDdyUv.y contains the float of the next pixel (pixel width-height used for 0.0..1.0 pixel addressing)
float4 ddxUvDdyUv : register(c31);

float4 main(float2 uv : TEXCOORD) : COLOR
{
		//float amount = 1.0 * (1.0 - ddxUvDdyUv.x);
		float4 color = tex2D(implicitInputSampler, uv);

		float2 adjacentUV1 = {uv.x - ddxUvDdyUv.x, uv.y};
		color.rgb += tex2D(implicitInputSampler, adjacentUV1);// * amount;

		float2 adjacentUV2 = {uv.x + ddxUvDdyUv.x, uv.y};
		color.rgb -= tex2D(implicitInputSampler, adjacentUV2);// * amount;

		//amount = 1.0 * (1.0 - ddxUvDdyUv.y);
		float2 adjacentUV3 = {uv.x, uv.y - ddxUvDdyUv.y};
		color.rgb += tex2D(implicitInputSampler, adjacentUV3);// * amount;

		float2 adjacentUV4 = {uv.x, uv.y + ddxUvDdyUv.y};
		color.rgb -= tex2D(implicitInputSampler, adjacentUV4);// * amount;

		return color;
}


