#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#elif SM6
	#define SV_POSITION SV_Position
	#define VS_SHADERMODEL vs_6_0
	#define PS_SHADERMODEL ps_6_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D<float4> SpriteTexture : register(t0);
sampler SpriteTextureSampler : register(s0);

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float2 rand(float2  p) { p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3))); return frac(sin(p)*43758.5453); }

float3 p_blurValues;

float4 MainPS(VertexShaderOutput input) : SV_Target0
{
	const int QUALITY = 8;
	const int DIRECTION = 16;
	const float TAU = 6.28318530716;

	float2 radius = p_blurValues.z / p_blurValues.xy;

	float4 colour = SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates);

	for (float d = 0.0; d < TAU; d += TAU / (float)DIRECTION)
	{
		float2 dir = float2(cos(d), sin(d)) * radius;

		for (float i = 1.0 / (float)QUALITY; i <= 1.0; i += 1.0 / (float)QUALITY)
		{
			colour += SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates + dir * i);
		}
	}

	for (int i = 0; i < 32; ++i)
	{
		colour += SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates + radius*(rand(input.TextureCoordinates) - 0.5));
	}


	colour /= (float)QUALITY*(float)DIRECTION + 33.0;

	return colour * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
