#define RADIUS  7
#define KERNEL_SIZE (RADIUS * 2 + 1)

//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

float weights[KERNEL_SIZE];
float2 offsets[KERNEL_SIZE];

//-----------------------------------------------------------------------------
// Textures.
//-----------------------------------------------------------------------------

Texture2D<float4> colorMapTexture : register(t0);
sampler colorMap : register(s0);

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_GaussianBlur(float2 texCoord : TEXCOORD) : SV_Target0
{
    float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    for (int i = 0; i < KERNEL_SIZE; ++i)
        color += colorMapTexture.Sample(colorMap, texCoord + offsets[i]) * weights[i];

    return color;
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique GaussianBlur
{
    pass
    {
#if SM6
        PixelShader = compile ps_6_0 PS_GaussianBlur();
#else
        PixelShader = compile ps_2_0 PS_GaussianBlur();
#endif
    }
}
