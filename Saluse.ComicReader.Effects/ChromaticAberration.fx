//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- ChromaticAberration
// Code from: http://www.francois-tarlier.com/blog/cubic-lens-distortion-shader/
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
	// lens distortion coefficient
	float k = -0.05;
	
	// cubic distortion value
	float kcube = 0.5;
	
	float r2 = (uv.x - 0.5) * (uv.x - 0.5) + (uv.y - 0.5) * (uv.y - 0.5);	
	float f = 0;

	//only compute the cubic distortion if necessary 
	if(kcube == 0.0)
	{
		f = 1 + r2 * k;
	}
	else
	{
		f = 1 + r2 * (k + kcube * sqrt(r2));
	};
	
    // get the right pixel for the current position
	float x = f * (uv.x - 0.5) + 0.5;
	float y = f * (uv.y - 0.5) + 0.5;
	float3 inputDistord = tex2D(implicitInputSampler, float2(x,y));

	return float4(inputDistord.r, inputDistord.g, inputDistord.b, 1);
}


