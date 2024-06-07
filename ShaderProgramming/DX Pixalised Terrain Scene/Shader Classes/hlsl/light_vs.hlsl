// Light vertex shader
// Standard issue vertex shader, apply matrices, pass info to pixel shader
//cbuffer MatrixBuffer : register(b0)
//{
//    matrix worldMatrix;
//    matrix viewMatrix;
//    matrix projectionMatrix;
//};
//cbuffer CameraBuffer : register(b1)
//{
//    float3 cameraPosition;
//    float padding;

//};
//cbuffer LightInfo : register(b2)
//{
//    float3 lightType;
//    float id;
//}
//struct InputType
//{
//    float4 position : POSITION;
//    float2 tex : TEXCOORD0;
//    float3 normal : NORMAL;
//    float3 tangent : TANGENT;
//    float3 binormal : BINORMAL;
//};

//struct OutputType
//{
//    float4 position : SV_POSITION;
//    float2 tex : TEXCOORD0;
//    float3 normal : NORMAL;
//    float3 viewVector : TEXCOORD1;
//    float3 tangent : TANGENT;
//    float3 binormal : BINORMAL;
//    float4 positionWorld : POSITIONWORLD;
//};

//OutputType main(InputType input)
//{
//    OutputType output;
    
//    //point
//    if (lightType.x == 1.0f)
//    {
//        // Calculate the position of the vertex against the world, view, and projection matrices.
//        output.position = mul(input.position, worldMatrix);
//        output.position = mul(output.position, viewMatrix);
//        output.position = mul(output.position, projectionMatrix);
//        // Store the texture coordinates for the pixel shader.
//        output.tex = input.tex;
//        // Calculate the normal vector against the world matrix only and normalise.
//        output.normal = mul(input.normal, (float3x3) worldMatrix);
//        output.normal = normalize(output.normal);
        
//        float worldPosition = mul(input.position, worldMatrix).xyz;
        
//        return output;
//    }
//    //directional.
//    if (lightType.y == 1.0f)
//    {
//        // Calculate the position of the vertex against the world, view, and projection matrices.
//        output.position = mul(input.position, worldMatrix);
//        output.position = mul(output.position, viewMatrix);
//        output.position = mul(output.position, projectionMatrix);
	    
//        // Calculate the position of the vertex in the world.
//        float4 worldPosition = mul(input.position, worldMatrix);
//        output.viewVector = cameraPosition.xyz - worldPosition.xyz;
//        output.viewVector = normalize(output.viewVector);
        
//        // Store the texture coordinates for the pixel shader.
//        output.tex = input.tex;

//	    // Calculate the normal vector against the world matrix only and normalise.
//        output.normal = mul(input.normal, (float3x3) worldMatrix);
//        output.normal = normalize(output.normal);

//        return output;
//    }
//    //spot.
//    else
//    {

//    // Transform the vertex position into projection space
//        output.position = mul(input.position, worldMatrix);
//        output.position = mul(output.position, viewMatrix);
//        output.position = mul(output.position, projectionMatrix);

//    // Pass through the texture coordinates
//        output.tex = input.tex;

//    // Transform the normal, tangent, and binormal into world space
//        output.normal = mul(input.normal, (float3x3) worldMatrix);
//        output.normal = normalize(output.normal);
//        output.tangent = mul(input.tangent, (float3x3) worldMatrix);
//        output.binormal = mul(input.binormal, (float3x3) worldMatrix);
//    // Store the position in world space for use in the pixel shader
//        output.positionWorld = mul(input.position, worldMatrix);

//        return output;
//    }


//    return output;
//}
// Standard issue vertex shader, apply matrices, pass info to pixel shader
cbuffer MatrixBuffer : register(b0)
{
    matrix worldMatrix;
    matrix viewMatrix;
    matrix projectionMatrix;
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


OutputType main(InputType input)
{
    OutputType output;
// Calculate the position of the vertex against the world, view, and projection matrices.
    output.position = mul(input.position, worldMatrix);
    output.position = mul(output.position, viewMatrix);
    output.position = mul(output.position, projectionMatrix);
// Store the texture coordinates for the pixel shader.
    output.tex = input.tex;
// Calculate the normal vector against the world matrix only and normalise.
    output.normal = mul(input.normal, (float3x3) worldMatrix);
    output.normal = normalize(output.normal);
    output.worldPosition = mul(input.position, worldMatrix).xyz;
    return output;
}