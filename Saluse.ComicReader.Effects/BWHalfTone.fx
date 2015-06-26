//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- ContrastAdjustEffect
//
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

sampler2D implicitInputSampler : register(S0);
sampler2D amplitudeMap : register(S1);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 main(float2 uv : TEXCOORD) : COLOR
{
		float blocksX = 6144.0;
		float blocksY = 6144.0;
		float levels = 129.0;
		float matrixSize = 16.0; // TODO: this is tied into amplitudeMap (creation in .cs). make using sin()
	
		float2 blocks = float2(blocksX, blocksY);
		float2 blockSize = 1.0 / blocks;
		float2 blockIndex = floor(uv / blockSize);

		float2 amplitudeUV = float2(blockIndex.x % matrixSize, blockIndex.y % matrixSize);
		amplitudeUV /= (matrixSize - 1.0);
		float4 amplitude = tex2D(amplitudeMap, amplitudeUV);    
		amplitude = amplitude * (255.0/levels);

	 float4 color = tex2D(implicitInputSampler, uv);
	 float gray = color.r * 0.3 + color.g * 0.59 + color.b *0.11;  
	 
	 if (gray <= amplitude.r)
	 {
			 color.r = 0.0;
			 color.g = 0.0;
			 color.b = 0.0;
	 }
	 else
	 {
			 color.r = 1.0;
			 color.g = 1.0;
			 color.b = 1.0;
	 }               
	 
	 return color;
}


