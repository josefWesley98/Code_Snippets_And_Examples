// Light pixel shader
// Calculate diffuse lighting for a single directional light (also texturing)

Texture2D texture0 : register(t0);
SamplerState sampler0 : register(s0);

cbuffer LightBuffer : register(b0)
{
    float4 lightColour;
    float4 ambientColour;
    float4 diffuseColour;
    float3 lightPosition;
    float padding;
    float3 lightDirection;
    float id;
    float4 specularColour;

    float3 cameraOption;
    float specularPower;
    
    float lightRange;
    float lightInnerCone;
    float lightOuterCone;
    float padding2;

};

struct InputType
{
    float4 position : SV_POSITION;
    float3 worldPosition : TEXCOORD1;
    float2 tex : TEXCOORD0;
    float3 normal : NORMAL;
    float3 viewVector : TEXCOORD1;

};

float4 calculateLighting(float3 lightDirection, float3 normal, float4 diffuse)
{
    float intensity = saturate(dot(normal, lightDirection));
    float4 colour = saturate(diffuse * intensity);
    return colour;
}

float4 calcSpecular(float3 lightDirection, float3 normal, float3 viewVector, float4
specularColour, float specularPower)
{
	// blinn-phong specular calculation
    float3 halfway = normalize(lightDirection + viewVector);
    float specularIntensity = pow(max(dot(normal, halfway), 0.0), specularPower);
    return saturate(specularColour * specularIntensity);
}

void calculateSpotLight(InputType input, float3 normal, float3 toEye)
{
   float4 ambient = float4(0, 0, 0, 0);
   float4 specular = float4(0, 0, 0, 0);
   float4 diffuse = float4(0, 0, 0, 0);

    float3 lightV = lightPosition - input.position;

    float distance = length(lightV);

    if (distance > lightRange)
    {
        return;
    }

    lightV /= distance;

    //ambient = mat.ambient * light.ambient;

    float diffuseFact = dot(lightV, normal);

    if (diffuseFact > 0.0f)
    {
        float3 vect = reflect(-lightV, normal);

        float specularFact = pow(max(dot(vect, toEye), 0.0f), 0);

        diffuse = diffuseFact * diffuseColour;
        specular = calcSpecular(lightDirection, input.normal, input.viewVector, specularColour, specularPower);
    }

    float spot = pow(max(dot(-lightV, float3(-lightDirection.x, -lightDirection.y, lightDirection.z)), 0.0f), 0);

    float attenuation = spot / dot(0, float3(1.0f, distance, distance * distance));

    ambient *= spot;
    diffuse *= attenuation;
    specular *= attenuation;
}

float4 PointLight(InputType input)
{
    float4 textureColour = float4(0, 0, 0, 0);
    float4 lightColour = float4(0, 0, 0, 0);
    float3 _lightPosition = float3(0, 0, 0);
    float3 _lightDirection = float3(0, 0, 0);
    float4 specular = float4(0, 0, 0, 0);
    float intensity = 0;
    
    textureColour = texture0.Sample(sampler0, input.tex);
    intensity = saturate(dot(input.normal.x, -lightDirection));
    
    // 1,0,0 = point light.
    //if (cameraOption.x == 1)
    //{
    float3 lightVector = normalize(_lightPosition - input.worldPosition);
    lightColour = calculateLighting(-lightDirection, input.normal, diffuseColour);
    lightColour = ambientColour + saturate(diffuseColour * intensity);
    specular = calcSpecular(lightDirection, input.normal, input.viewVector, specularColour, specularPower);
    lightColour += specular;
    return lightColour * textureColour;
        
}
float4 DirectionalLight(InputType input)
{
    float3 normal = input.normal;
    //normal = normalize(normal);

    // Calculate the diffuse lighting term.
    float diffuse = max(dot(normal, lightDirection), 0);

    // Calculate the ambient lighting term.
    float4 ambient = ambientColour;

    // Calculate the specular lighting term.
    float3 viewDirection = normalize(-input.position.xyz);
    float3 reflectDirection = reflect(-lightDirection, normal);
    float specular = pow(max(dot(viewDirection, reflectDirection), 0), specularPower);

    // Combine the diffuse, ambient, and specular lighting terms.
    float4 lighting = (ambient + diffuse * lightColour) + specular * specularColour;

    // Return the final color of the pixel.
    return lighting;
}
float4 main(InputType input) : SV_TARGET
{
   
        //return PointLight(input);
    
        return DirectionalLight(input);
        
    
}

