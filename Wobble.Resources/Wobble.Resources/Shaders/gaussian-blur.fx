Texture2D<float4> SpriteTexture : register(t0);
SamplerState SpriteTextureSampler : register(s0);

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float3 p_blurValues;


float4 MainPS(VertexShaderOutput input) : SV_Target0
{
	const int QUALITY = 8;
	const int DIRECTION = 16;
	const float TAU = 6.28318530716f;

	float2 radius = p_blurValues.z / p_blurValues.xy;

	float4 colour = SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates);

	for(float d = 0.0; d <  TAU; d += TAU / (float)DIRECTION)
    {
		float2 dir = float2(cos(d), sin(d)) * radius;

		for(float i = 1.0 / (float)QUALITY; i <= 1.0; i += 1.0 / (float)QUALITY)
		{			
			colour += SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates + dir * i);
        }
    }

	colour /= (float)QUALITY*(float)DIRECTION+1.0;

	return colour * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_6_0 MainPS();
	}
};
