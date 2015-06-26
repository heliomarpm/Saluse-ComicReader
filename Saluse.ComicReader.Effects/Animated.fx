//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Animated
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);

float location : register(C0);
float lineWidth: register(C1);
float strength: register(C2);

/*
static int permutation[] = {
	151, 180 // TODO: requires random selection of 0..25
};

texture permTexture
<
	string texturetype = "2D";
	string format = "l8";
	string function = "GeneratePermTexture";
	int width = 256, height = 1;
>;

float4 GeneratePermTexture(float p : POSITION) : COLOR
{
	return permutation[p * 256] / 255.0;
}

sampler permSampler = sampler_state
{
	texture = <permTexture>;
	AddressU  = Wrap;
	AddressV  = Clamp;
	MAGFILTER = POINT;
	MINFILTER = POINT;
	MIPFILTER = NONE;
};
*/


float4 main(float2 uv : TEXCOORD) : COLOR
{
		float4 pixelColor = tex2D(implicitInputSampler, uv);
		float result = (location - uv.x);
		if (result < 0)
		{
			result *= -1;
		}

		if (result < lineWidth)
		{
			/*
			// Distort effect
			uv.x += lineWidth;
			pixelColor = tex2D(implicitInputSampler, uv);
			*/

			pixelColor *= strength;
		}

		// return final pixel color
		return pixelColor;
}


