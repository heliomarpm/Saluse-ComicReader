//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- GreyScale
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
	float4 greyScaleColor = (color.r * 0.3) + (color.g * 0.59) + (color.b * 0.11);
	return greyScaleColor;
}

