Texture2D<float4> SpriteTexture : register(t0);
SamplerState SpriteTextureSampler : register(s0);

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


float2 p_dimensions;

float2 p_position;
float2 p_rectangle;

float  p_alpha;


float4 MainPS(VertexShaderOutput input) : SV_Target0
{

	float2 coord = input.TextureCoordinates * p_dimensions;

	if (coord.x <= p_position.x + p_rectangle.x && coord.x >= p_position.x &&
		coord.y <= p_position.y + p_rectangle.y && coord.y >= p_position.y)
	{
		input.Color.a = p_alpha;
	}

	return SpriteTexture.Sample(SpriteTextureSampler, input.TextureCoordinates) * input.Color;	
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile ps_6_0 MainPS();
	}
};
