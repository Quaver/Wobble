#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#elif SM6
	#define SV_POSITION SV_Position
	#define VS_SHADERMODEL vs_6_0
	#define PS_SHADERMODEL ps_6_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D<float4> SpriteTexture : register(t0);
sampler SpriteTextureSampler : register(s0);

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float2 rand(float2 p) { p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3))); return frac(sin(p)*43758.5453); }

float3 p_blurValues;

float4 MainPS(VertexShaderOutput input) : SV_Target0
{
	return SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates + (p_blurValues.z / p_blurValues.xy) * (rand(input.TextureCoordinates) - 0.5)) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
