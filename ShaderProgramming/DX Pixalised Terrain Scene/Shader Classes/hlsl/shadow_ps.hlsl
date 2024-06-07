
//Texture2D shaderTexture : register(t0);
//Texture2D depthMapTexture : register(t1);

//SamplerState diffuseSampler : register(s0);
//SamplerState shadowSampler : register(s1);

//cbuffer LightBuffer : register(b0)
//{
  
//    float4 diffuseColour;
//    float3 lightDirection;
//    float padding;
//    float3 lightPosition;
//    float specularPower;
//    float4 ambience;
//    float4 specularColour;
//    float3 cameraPos;
//    float outerSpotlightRadius;
//};

//struct InputType
//{
//    float4 position : SV_POSITION;
//    float2 tex : TEXCOORD0;
//    float3 normal : NORMAL;
//    float3 worldPosition : TEXCOORD1;
//    float4 lightViewPos : TEXCOORD2;
//};
//struct OutputType
//{
//    float4 color : SV_TARGET0;
//};
//// Calculate lighting intensity based on direction and normal. Combine with light colour.
//float4 calculateLighting(float3 lightDirection, float3 normal, float4 diffuse)
//{
//    float intensity = saturate(dot(normal, lightDirection));
//    float4 colour = saturate(diffuse * intensity);
//    return colour;
//}

//// Is the gemoetry in our shadow map
//bool hasDepthData(float2 uv)
//{
//    if (uv.x < 0.f || uv.x > 1.f || uv.y < 0.f || uv.y > 1.f)
//    {
//        return false;
//    }
//    return true;
//}

//bool isInShadow(Texture2D sMap, float2 uv, float4 lightViewPosition, float bias)
//{
//    // Sample the shadow map (get depth of geometry)
//    float depthValue = sMap.Sample(shadowSampler, uv).r;
//	// Calculate the depth from the light.
//    float lightDepthValue = lightViewPosition.z / lightViewPosition.w;
//    lightDepthValue -= bias;

//	// Compare the depth of the shadow map value and the depth of the light to determine whether to shadow or to light this pixel.
//    if (lightDepthValue < depthValue)
//    {
//        return false;
//    }
//    return true;
//}

//float2 getProjectiveCoords(float4 lightViewPosition)
//{
//    // Calculate the projected texture coordinates.
//    float2 projTex = lightViewPosition.xy / lightViewPosition.w;
//    projTex *= float2(0.5, -0.5);
//    projTex += float2(0.5f, 0.5f);
//    return projTex;
//}

////float4 main(InputType input) : SV_TARGET
////{
//// //   float shadowMapBias = 0.005f;
//// //   float4 colour = float4(0.f, 0.f, 0.f, 1.f);
//// //   float4 textureColour = shaderTexture.Sample(diffuseSampler, input.tex);

////	//// Calculate the projected texture coordinates.
//// //   float2 pTexCoord = getProjectiveCoords(input.lightViewPos);
	
//// //   // Shadow test. Is or isn't in shadow
//// //   if (hasDepthData(pTexCoord))
//// //   {
//// //       // Has depth map data
//// //       if (!isInShadow(depthMapTexture, pTexCoord, input.lightViewPos, shadowMapBias))
//// //       {
//// //           // is NOT in shadow, therefore light
//// //           colour = calculateLighting(-direction, input.normal, diffuse);
//// //       }
//// //   }
    
//// //   colour = saturate(colour + ambient);
//// //   return saturate(colour) * textureColour;
    
////}
//OutputType main(InputType input)
//{
//    OutputType output;

//    // Calculate the diffuse light
//    float3 light = normalize(-lightDirection);
//    float diffuseIntensity = clamp(dot(input.normal, light), 0.0f, 1.0f);
//    float4 diffuseLight = diffuseColour * diffuseIntensity;

//    // Calculate the specular light
//    float3 viewDirection = normalize(cameraPos - input.worldPosition);
//    float3 reflectDirection = reflect(-lightDirection, input.normal);
//    float specularIntensity = clamp(pow(max(dot(viewDirection, reflectDirection), 0.0f), specularPower), 0.0f, 1.0f);
//    float4 specularLight = specularColour * specularIntensity;

//    // Calculate the shadow factor
//    float shadowDepth = depthMapTexture.Sample(shadowSampler, input.lightViewPos.xy).r;
//    float shadowFactor = 1.0f;
//    if (shadowDepth < input.lightViewPos.z)
//    {
//        shadowFactor = 0.5f;
//    }

//    // Apply the shadow factor to the diffuse and specular light
//    diffuseLight *= shadowFactor;
//    specularLight *= shadowFactor;
//    // Combine the diffuse, specular, and ambient light
//    output.color = diffuseLight + specularLight + ambience;

//    return output;

//}

Texture2D shaderTexture : register(t0);
Texture2D depthMapTexture : register(t1);

SamplerState diffuseSampler : register(s0);
SamplerState shadowSampler : register(s1);

cbuffer LightBuffer : register(b0)
{
    float4 ambient;
    float4 diffuse;
    float3 direction;
};

struct InputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float4 lightViewPos : TEXCOORD1;
};

// Calculate lighting intensity based on direction and normal. Combine with light colour.
float4 calculateLighting(float3 lightDirection, float3 normal, float4 diffuse)
{
    float intensity = saturate(dot(normal, lightDirection));
    float4 colour = saturate(diffuse * intensity);
    return colour;
}

// Is the gemoetry in our shadow map
bool hasDepthData(float2 uv)
{
    if (uv.x < 0.f || uv.x > 1.f || uv.y < 0.f || uv.y > 1.f)
    {
        return false;
    }
    return true;
}

bool isInShadow(Texture2D sMap, float2 uv, float4 lightViewPosition, float bias)
{
    // Sample the shadow map (get depth of geometry)
    float depthValue = sMap.Sample(shadowSampler, uv).r;
	// Calculate the depth from the light.
    float lightDepthValue = lightViewPosition.z / lightViewPosition.w;
    lightDepthValue -= bias;

	// Compare the depth of the shadow map value and the depth of the light to determine whether to shadow or to light this pixel.
    if (lightDepthValue < depthValue)
    {
        return false;
    }
    return true;
}

float2 getProjectiveCoords(float4 lightViewPosition)
{
    // Calculate the projected texture coordinates.
    float2 projTex = lightViewPosition.xy / lightViewPosition.w;
    projTex *= float2(0.5, -0.5);
    projTex += float2(0.5f, 0.5f);
    return projTex;
}

float4 main(InputType input) : SV_TARGET
{
    float shadowMapBias = 0.005f;
    float4 colour = float4(0.f, 0.f, 0.f, 1.f);
    float4 textureColour = shaderTexture.Sample(diffuseSampler, input.tex);

	// Calculate the projected texture coordinates.
    float2 pTexCoord = getProjectiveCoords(input.lightViewPos);
	
    // Shadow test. Is or isn't in shadow
    if (hasDepthData(pTexCoord))
    {
        // Has depth map data
        if (!isInShadow(depthMapTexture, pTexCoord, input.lightViewPos, shadowMapBias))
        {
            // is NOT in shadow, therefore light
            colour = calculateLighting(-direction, input.normal, diffuse);
        }
    }
    
    colour = saturate(colour + ambient);
    return saturate(colour) * textureColour;
}