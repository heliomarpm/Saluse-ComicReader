//--------------------------------------------------------------------------------------
// 
// WPF ShaderEffect HLSL -- Halftone
// Based on: https://www.shadertoy.com/view/4t2yDt
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Sampler Inputs (Brushes, including ImplicitInput)
//--------------------------------------------------------------------------------------

#define PI 3.14159265358979323846
#define SECTIONS 228.0
#define RADIUS_START 10.5

sampler2D implicitInputSampler : register(S0);

// ddxUvDdyUv.x and ddxUvDdyUv.y contains the float of the next pixel (pixel width-height used for 0.0..1.0 pixel addressing)
float4 ddxUvDdyUv : register(c31);


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float2 rotate2D(float2 _uv, float _angle) {
	_uv -= 0.5;
	_uv = mul(float2x2(cos(_angle), -sin(_angle), sin(_angle), cos(_angle)),  _uv);

	_uv += 0.5;
	return _uv;
}

float2 tile(float2 _uv, float _zoom) {
	_uv *= _zoom;
	return frac(_uv);
}

float circle(in float2 _uv, in float _radius) {
	float2 dist = _uv - 0.5;
	return 1. - smoothstep(_radius - (_radius*0.1), _radius + (_radius*0.1), dot(dist, dist)*4.0);
}

float4 greyscale(float4 color) {
	return (color.r * 0.3) + (color.g * 0.59) + (color.b * 0.11);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
	//see: https://msdn.microsoft.com/en-us/library/system.windows.media.effects.shadereffect.ddxuvddyuvregisterindex(v=vs.110).aspx
	float2 uv_square = float2(uv.x, uv.y * (ddxUvDdyUv.x / ddxUvDdyUv.w));
	uv_square = rotate2D(uv_square, PI*.32);

	float2 uv_tiled = tile(uv_square, SECTIONS);

	float4 texColor = greyscale(tex2D(implicitInputSampler, uv));

	float c = circle(uv_tiled, RADIUS_START * pow(texColor.r, 4.5));
	
	return float4(float3(c, c, c), 1.0);
}
