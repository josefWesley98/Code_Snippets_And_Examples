Texture2D shaderTexture : register(t0);
SamplerState SampleType : register(s0);

Texture2D shaderTexture1 : register(t1);
SamplerState SampleType1 : register(s1);
// The size of each pixel in texels

cbuffer PixelSizeBufferType : register(b0)
{
    float pixelSize;
    float3 padding;
};

struct InputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
};

cbuffer ScreenSizeBuffer : register(b1)
{
    float screenWidth;
    float screenHeight;
    float2 padding2;
};

float4 ditherMatrix[16] =
{
    float4(0, 8, 2, 10),
    float4(12, 4, 14, 6),
    float4(3, 11, 1, 9),
    float4(15, 7, 13, 5),
    float4(8, 0, 10, 2),
    float4(4, 12, 6, 14),
    float4(11, 3, 9, 1),
    float4(7, 15, 5, 13),
    float4(2, 10, 0, 8),
    float4(14, 6, 12, 4),
    float4(1, 9, 3, 11),
    float4(13, 5, 15, 7),
    float4(10, 2, 8, 0),
    float4(6, 14, 4, 12),
    float4(9, 1, 11, 3),
    float4(5, 13, 7, 15)
};

float4 PixelArtEffect(InputType input)
{
    float2 tex = input.tex;
    // Sample the texture
    float4 colour = shaderTexture.Sample(SampleType, tex);
    // Downsample the image by rounding down the texture coordinate to the nearest multiple of pixelSize
    tex = tex / pixelSize;
    tex = floor(tex) * pixelSize;
    // Upscale the image back to its original resolution by sampling at the rounded down coordinate
    colour = shaderTexture.Sample(SampleType, tex);
    return colour;
}

float4 main(InputType input) : SV_TARGET
{
    return PixelArtEffect(input);
}