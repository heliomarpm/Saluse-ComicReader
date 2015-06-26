//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Test
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);
// ddxUvDdyUv.x and ddxUvDdyUv.y contains the float of the next pixel (pixel width-height used for 0.0..1.0 pixel addressing)
float4 ddxUvDdyUv : register(c31);

/*static float2 Delta = { 1.0, 1.0 };
static int Adjustment = 0;
static float Sigma = 80.0;
*/
//TODO: Cannot increase size due to limitations for ps_2. tryc compiling using fxc.exe directly
static int Size = 4;

/*
float Function2D(float x, float y, float sigma)
{
	float PI = 3.14159265f;
	float sqrSigma = sigma * sigma; //pow(sigma, 2);

	float tmp = (x * x + y * y) / (-2 * sqrSigma);
	float z = exp(tmp);
	float n = 2 * PI * sqrSigma;
	float v = z / n;
	//exp( ( x * x + y * y ) / ( -2 * sqrSigma ) ) / ( 2 * PI * sqrSigma );
	return v;
}
*/

/*
float4 Get(float2 texcoord, int x, int y)
{
		// float2 coord = float2(texcoord.x + x / 512.0f, texcoord.y + y / 512.0f );
		float2 coord = texcoord + float2(x, y) * Delta;

		return tex2D(implicitInputSampler, coord);
}
*/

float4 main(float2 uv : TEXCOORD) : COLOR
{
	//int divisor = 0;
	//radius
	int r = Size / 2;
	//float minimum = Function2D(-r, -r, Sigma);
	float4 value = float4(0, 0, 0, 1);

		[unroll(Size)]for (int y = -r; y < r; y++)
		{
		[unroll(Size)]for (int x = -r; x < r; x++)
			{
				/*float tmp = Function2D(x, y, Sigma);
				tmp = tmp / minimum;
				int inttmp = (int)tmp;*/
				//value += Get(uv, x, y) * inttmp;

				//float2 coord = uv + float2(x * ddxUvDdyUv.x, y * ddxUvDdyUv.y) * Delta;
				float2 coord = uv + float2(x * ddxUvDdyUv.x, y * ddxUvDdyUv.y);
				value += tex2D(implicitInputSampler, coord);
				
				//divisor += inttmp;
				//divisor++;
			}
		}

	value /= (Size * Size);

	return value;
}