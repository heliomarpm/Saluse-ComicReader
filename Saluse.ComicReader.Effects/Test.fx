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

		
		// Get the pixel to the right and bottom of the current pixel coordinate
		float4 pixelColor = tex2D(implicitInputSampler, uv + ddxUvDdyUv);

		// return final pixel color
		return pixelColor;
}


