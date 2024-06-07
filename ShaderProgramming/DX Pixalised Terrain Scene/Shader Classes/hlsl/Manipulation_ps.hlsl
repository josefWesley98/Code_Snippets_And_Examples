
Texture2D texture0 : register(t0);
SamplerState sampler0 : register(s0);

// Declare the sampler for the depth buffer
Texture2D depthMapTexture : register(t1);
SamplerState diffuseSampler : register(s2);
SamplerState shadowSampler : register(s3);

// Declare the depth buffer texture
cbuffer TimeBuffer : register(b1)
{
    float time;
    float check;
    float2 padding2;
};

cbuffer LightBuffer : register(b0)
{
    float4 diffuseColour;
    float3 lightDirection;
    float usingShadowMap;
    float3 lightPosition;
    float specularPower;
    float4 ambience;
    float4 specularColour;
    float3 cameraPos;
    float outerSpotlightRadius;
    
    float innerSpotlightRadius;
    float spotlightRange;
    float constantAttenuation;
    float linearAttenuation;
    
    float quadraticAttenuation;
    float pointLightRadius;
    float usePointLight;
    float useDirectionalLight; 
};

struct InputType
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 worldPosition : TEXCOORD1;
    //float4 lightViewPos : TEXCOORD2;
};

// attempt at creating a reflective shader.
float4 riverShader(InputType input, float4 textureColour)
{
    float3 scaleMultiplier = float3(4.0f, 2.5f, 2.0f);
    float3 scaledNormal = input.normal * scaleMultiplier;
    // Normalize the scaled normal vector
    scaledNormal = normalize(scaledNormal);
    // Calculate the reflection vector
    float3 reflectionVector = reflect(input.position.xyz, scaledNormal);
    
    // Sample the water normal map using the reflection vector as the texture coordinate
    scaledNormal = textureColour.rgb;
    
    // Calculate the specular lighting term using the reflection vector and the normal from the normal map
    float specular = pow(max(dot(normalize(input.position.xyz), scaledNormal), 0), 16);
    float4 waterColour = float4(0.0, 0.5, 0.9, 1.0);
    // Calculate the final color of the pixel by blending the water color with the reflected color
    float4 colour = lerp(waterColour, textureColour, 0.65f);
    
    // Add the specular lighting term to the final color
    colour.rgb += specular;
    
    return colour;
}

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

float4 calculateLighting(float3 lightDirection, float3 normal, float4 ldiffuse)
{
    float intensity = saturate(dot(normal, lightDirection));
    float4 colour = saturate(ldiffuse * intensity);
    return colour;
}


float4 ApplyDirectionalLight(InputType input,  float4 textureColour)
{
    float4 finalColour = float4(0.0f, 0.0f, 0.0f, 0.0f);

    float3 normal = input.normal * 2.5f;
    float4 diffuse = textureColour;
 
    float4 intensity = saturate(dot(normal , -lightDirection));
    float howMuchLight = dot(intensity, input.normal);
    
   
    finalColour += ambience+ howMuchLight * diffuse * (diffuseColour * intensity);
   
        
  
    float3 viewDirection = normalize(cameraPos - input.worldPosition);
    float3 reflectDirection = reflect(-intensity.xyz, normal);
    
    float specular = pow(max(dot(reflectDirection, input.normal), 0.0f), specularPower);


    float4 specularFinal = saturate(specularColour * specular);
    textureColour += specularFinal * 0.35f;

    return finalColour * textureColour;
}

float4 ApplyPointLight(InputType input, float4 textureColour)
{
    input.normal = normalize(input.normal);

    float4 diffuse = textureColour;

    float4 finalColour = float4(0.0f, 0.0f, 0.0f, 0.0f);
    
    //Create the vector between light position and pixels position
    float3 lightToPixelVec = lightPosition - input.worldPosition;
        
    //Find the distance between the light pos and pixel pos
    float d = length(lightToPixelVec);
    
    //Create the ambient light
    float4 finalAmbient = diffuse * ambience;

    //If pixel is too far, return pixel color with ambient light
    if (d > 100.0f)
    {
        return finalAmbient * 0.1f;
    }
       
    //Calculate how much light the pixel gets by the angle in which the light strikes the pixels surface
    float howMuchLight = dot(lightToPixelVec, input.normal);

    //If light is striking the front side of the pixel
    if (howMuchLight > 0.0f)
    {
        //Add light to the finalColor of the pixel
        finalColour += howMuchLight * diffuse * (diffuseColour * 1.5f);
        float attenuation = constantAttenuation + (linearAttenuation * d) + (quadraticAttenuation * (d * d));
        
        //Calculate Light's Falloff factor
        finalColour /= attenuation;
        float3 viewDirection = normalize(cameraPos - input.worldPosition);
        float3 reflectDirection = reflect(-lightToPixelVec, input.normal);
    
        float specular = pow(max(dot(reflectDirection, viewDirection), 0.0f), specularPower);
        float4 spec = saturate(specular * specularColour);
    
        textureColour += spec;
    }
    finalColour = saturate(finalColour + finalAmbient);
   
    //Return Final Color
    return finalColour * textureColour;
}

float4 main(InputType input) : SV_TARGET
{
    float4 textureColour = float4(0, 0, 0, 0);
    float shadowMapBias = 0.005f;
    float4 lightColour = float4(0, 0, 0, 0);
    textureColour = texture0.Sample(sampler0, input.tex);
    
    
    if(usingShadowMap == 1)
    {
        float newTex = texture0.Sample(diffuseSampler, input.tex);
        float4 newLightPos = dot(input.normal, -lightDirection);
        float2 pTexCoord = getProjectiveCoords(newLightPos);
        
        if (usePointLight == 1)
        {
            if (hasDepthData(pTexCoord))
            {
                if (!isInShadow(depthMapTexture, pTexCoord, newLightPos, shadowMapBias))
                {
                    lightColour += ApplyPointLight(input, textureColour);
                }
            }
        }
        if (useDirectionalLight == 1)
        {
            if (hasDepthData(pTexCoord))
            {
                if (!isInShadow(depthMapTexture, pTexCoord, newLightPos, shadowMapBias))
                {
                    // is NOT in shadow, therefore light
                    lightColour += ApplyDirectionalLight(input, textureColour);
                }
            }
       
        }
 
        return saturate(lightColour + ambience) * newTex;
    }
    
    if(usingShadowMap == 0)
    {
        if (usePointLight == 1)
        {
           lightColour += ApplyPointLight(input, textureColour);
        }
        if (useDirectionalLight == 1)
        {
            lightColour += ApplyDirectionalLight(input, textureColour);
        }
        return lightColour;

    }
    else
    {
        return lightColour;
        
    }
}


// attempt at a spotlight but it just kept going wrong.
//float4 Spotlight(InputType input, float4 textureColour, float4 lightColour)
//{
  
//    // Normalize the normal vector
//        float3 scaledNormal = input.normal * 2.5f;
//        scaledNormal = normalize(scaledNormal);

//    // Calculate the vector from the surface to the light
//        float3 lightVector = normalize(lightPosition - input.worldPosition.xyz);

//    // Calculate the intensity of the light on the surface
//        float intensity = saturate(dot(scaledNormal, lightVector));

//    // Calculate the distance from the light to the vertex
//        float distance = length(lightPosition - input.position.xyz);

//    // Calculate the diffuse lighting term
//        float4 diffuse = diffuseColour * CalculateDiffuseLighting(scaledNormal, lightVector, intensity) * 2;

//    // Calculate the ambient lighting term
//        float4 ambient = ambience * 0.45f;

//    // Calculate the specular lighting term
//        float3 reflectDirection = reflect(-lightDirection, scaledNormal);
//        float4 specular = CalculateSpecularLighting(scaledNormal, lightDirection, lightVector, intensity);
//       // specular = pow(max(dot(lightVector, reflectDirection), 0), specularPower);
//        specular = saturate(specular); // Clamp specular value between 0 and 1

//    // Calculate the spotlight attenuation factor
//        //float spotAttenuation = CalculateSpotlightAttenuation(input.position.xyz, lightPosition);

//    // Combine the diffuse, ambient, and specular lighting terms
//    lightColour += ambient + diffuse * intensity + (specular * specularPower); //* spotAttenuation;

//        return lightColour;
//}