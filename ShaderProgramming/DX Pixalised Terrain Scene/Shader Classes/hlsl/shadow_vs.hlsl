Texture2D mtexture : register(t0);
SamplerState msampler : register(s0);

cbuffer MatrixBuffer : register(b0)
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
    matrix lightViewMatrix;
    matrix lightProjectionMatrix;
    
    float id;
    float3 padding;
};

struct InputType
{
    float4 position : POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
};

struct OutputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float4 lightViewPos : TEXCOORD1;
};

float GetHeightDisplacement(float2 uv)
{
    float offset = mtexture.SampleLevel(msampler, uv, 0).r;
    return offset * 150.0f;
}

float3 CalculateNormal(float2 uv)
{
    float texWidth = 4096.0f;
    float value;
    mtexture.GetDimensions(0, texWidth, texWidth, value);
    float uvOffset = 1.0f / 100.f;
    float heightN = GetHeightDisplacement(float2(uv.x, uv.y + uvOffset));
    float heightS = GetHeightDisplacement(float2(uv.x, uv.y - uvOffset));
    float heightE = GetHeightDisplacement(float2(uv.x + uvOffset, uv.y));
    float heightW = GetHeightDisplacement(float2(uv.x - uvOffset, uv.y));

    float WorldStep = 100.f * uvOffset;
    float3 tan = normalize(float3(2.0f * WorldStep, heightE - heightW, 0));
    float3 bitan = normalize(float3(0, heightN - heightS, 2.0f * WorldStep));
    return cross(bitan, tan);

}
OutputType DoScenry(InputType input)
{
    OutputType output;
    
    //Scale and translate the input position
    float4x4 scaleMatrix = float4x4(2.5f, 0.0f, 0.0f, 0.0f,
                                    0.0f, 2.5f, 0.0f, 0.0f,
                                    0.0f, 0.0f, 2.5f, 0.0f,
                                    0.0f, 0.0f, 0.0f, 2.5f);
    float4x4 translationMatrix = float4x4(1.0f, 0.0f, 0.0f, 0.0f,
                                         0.0f, 1.0f, 0.0f, 0.0f,
                                         0.0f, 0.0f, 1.0f, 0.0f,
                                         -20.0f, -0.5f, 0.0f, 1.0f);
  
    float4x4 transformMatrix = mul(scaleMatrix, translationMatrix);
   
    input.position.w = 1.0f;
    float4 transformedPos = mul(input.position, transformMatrix);
    output.position = transformedPos;

    output.position.y += GetHeightDisplacement(input.tex);
    // Transform the position of the vertex using the view and projection matrices
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);
    
    // Leave the texture coordinates unchanged
    output.tex = input.tex;
    
    // Calculate the normal vector and transform it using the world matrix
    float3 crossProduct = CalculateNormal(input.tex);
    output.normal = mul(CalculateNormal(input.tex), (float3x3) worldMatrix);
    output.normal = normalize(output.normal);

    return output;
}
OutputType main(InputType input)
{
    OutputType output;

	//// Calculate the position of the vertex against the world, view, and projection matrices.
    

    //if(id == 1.0f)
    //{
        output.position = DoScenry(input).position;
        output.normal = DoScenry(input).normal;
        output.tex = input.tex;
        
    //}
    //else if(id == 2.0f)
    //{
    //    output.position = mul(input.position, worldMatrix);
    //    output.position = mul(output.position, viewMatrix);
    //    output.position = mul(output.position, projectionMatrix);
    //    output.normal = mul(input.normal, (float3x3) worldMatrix);
    //    output.normal = normalize(output.normal);   
    //}
    
    
    output.lightViewPos = mul(DoScenry(input).position, worldMatrix);
    output.lightViewPos = mul(output.lightViewPos, lightViewMatrix);
    output.lightViewPos = mul(output.lightViewPos, lightProjectionMatrix);

    return output;
}