Texture2D mtexture : register(t0);
SamplerState msampler : register(s0);

cbuffer MatrixBuffer : register(b0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
};

cbuffer TimeBuffer : register(b1)
{
	float time;
	float check;
    float2 padding2;
        
    matrix lightViewMatrix;
    matrix lightProjectionMatrix;
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
    float3 worldPosition : TEXCOORD1;
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

OutputType CreateWaveEffect(InputType input, float positionX, float positionY, float positionZ, float waveDirection)
{

    OutputType output;

    // Modify the y-coordinate of the position to create a continuously flowing river effect
    float x = input.position.x;
    float z = input.position.z;
    float flowSpeed = 0.5f; 
    float flowDirection = waveDirection; // 0.0f = right to left, 1.0f = left to right
    float flowOffset = (time * flowSpeed) + (x * (1.0f - flowDirection));
    float waveAmplitude = 0.15f; 
    float waveFrequency = 1.0f; 
    float y = input.position.y + sin((flowOffset * waveFrequency)) * waveAmplitude;

    // Transform the position to clip space
    output.position = mul(float4(x, y, z, 1.0f), worldMatrix);
    output.tex = input.tex;

    // Resize and reposition the river object using a transformation matrix
    float4x4 scaleMatrix = float4x4(4.0f, 0.0f, 0.0f, 0.0f,
                                    0.0f, 2.5f, 0.0f, 0.0f,
                                    0.0f, 0.0f, 2.5f, 0.0f,
                                    0.0f, 0.0f, 0.0f, 2.5f);
    
    float4x4 translationMatrix = float4x4(1.0f, 0.0f, 0.0f, 0.0f,
                                        0.0f, 1.0f, 0.0f, 0.0f,
                                        0.0f, 0.0f, 1.0f, 0.0f,
                                        positionX, positionY, positionZ, 1.0f);
    
    float4x4 transformMatrix = mul(scaleMatrix, translationMatrix);
    
    float4 transformedPosition = mul(float4(x, y, z, 1.0f), transformMatrix);

    // Transform the position to clip space
    float4 transformedPos = mul(float4(x, y, z, 1.0f), transformMatrix);
    output.position = mul(transformedPos, worldMatrix);
    output.tex = input.tex;
    
    return output;
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
                                         -20.0f, -2.5f, 0.0f, 1.0f);
  
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

    if (check == 1)
    {
        output = CreateWaveEffect(input, -30.0f, -1.35f, -48.5f, 0.0);
        output.normal = mul(input.normal, (float3x3) worldMatrix);
        output.normal = normalize(output.normal);
        output.position = mul(output.position, viewMatrix);
        output.position = mul(output.position, projectionMatrix);

    // set the texture coordinate
        output.tex = input.tex;
    }
    if (check == 2)
    {
        output = CreateWaveEffect(input, -30.0f, -1.75f, 48.5f, 0.0f);
        output.normal = mul(input.normal, (float3x3) worldMatrix);
        output.normal = normalize(output.normal);
        output.position = mul(output.position, viewMatrix);
        output.position = mul(output.position, projectionMatrix);

    // set the texture coordinate
        output.tex = input.tex;
    }
    if (check == 3)
    {
        output = DoScenry(input);
    }
    if(check == 4)
    {
        output.position = mul(input.position, worldMatrix);
        output.position = mul(output.position, viewMatrix);
        output.position = mul(output.position, projectionMatrix);
        output.normal = mul(input.normal, (float3x3) worldMatrix);
        output.normal = normalize(output.normal);   
        output.tex = input.tex;
    }
  
    return output;
}