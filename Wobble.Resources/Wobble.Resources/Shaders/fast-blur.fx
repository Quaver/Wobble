Texture2D<float4> SpriteTexture : register(t0);
SamplerState SpriteTextureSampler : register(s0);

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float2 rand(float2 p) { p = float2(dot(p, float2(127.1f, 311.7f)), dot(p, float2(269.5f, 183.3f))); return frac(sin(p) * 43758.5453f); }

float3 p_blurValues;

float4 MainPS(VertexShaderOutput input) : SV_Target0
{
	return SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates + (p_blurValues.z / p_blurValues.xy) * (rand(input.TextureCoordinates) - 0.5f)) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_6_0 MainPS();
	}
};
