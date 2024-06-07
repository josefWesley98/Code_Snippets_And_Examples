#pragma once

#include "DXF.h"
#include <cmath>

using namespace std;
using namespace DirectX;

class ManipulationShader : public BaseShader
{
private:
	struct LightBufferType
	{
		XMFLOAT4 diffuseColour;
		XMFLOAT3 lightDirection;
		float usingShadowMap;
		XMFLOAT3 lightPosition;
		float specularPower;
		XMFLOAT4 ambience;
		XMFLOAT4 specularColour;
		XMFLOAT3 cameraPos;
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
	struct DeviceBuffer
	{
		
	};
	struct TimeBufferType
	{
		float time;
		float check;
		XMFLOAT2 padding2;	
		XMMATRIX lightViewMatrix;
		XMMATRIX lightProjectionMatrix;
	};

public:
	ManipulationShader(ID3D11Device* device, HWND hwnd);
	~ManipulationShader();

	void setShaderParameters(ID3D11DeviceContext* deviceContext, const XMMATRIX& world, const XMMATRIX& view, const XMMATRIX& projection, ID3D11ShaderResourceView* texture, ID3D11ShaderResourceView* texture1, ID3D11ShaderResourceView* depthMap, Light* light, float& _usePointLight, float& _useDirectionalLight, float& _pointLightRadius, float& _check, float& time, Camera* cam, float& _constantAttenuation, float& _linearAttenuation, float& quadraticAttenuation, float& _outerSpotlightRadius, float& _innerSpotlightRadius, float& _spotlightRange, float& _usingShadowMap);

private:
	void initShader(const wchar_t* cs, const wchar_t* ps);

private:

	Timer* theTimer;
	ID3D11Buffer* matrixBuffer;
	ID3D11SamplerState* sampleState;

	ID3D11SamplerState* diffuseSampler;
	ID3D11SamplerState* sampleStateShadow;

	ID3D11Buffer* lightBuffer;
	ID3D11Buffer* timeBuffer;
};

