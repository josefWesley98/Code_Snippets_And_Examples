#pragma once

#include "DXF.h"

using namespace std;
using namespace DirectX;

class LightShader : public BaseShader
{
private:
	struct LightBufferType
	{
		XMFLOAT4 ambient;
		XMFLOAT4 diffuse;
		XMFLOAT3 lightPosition;
		float padding;
		XMFLOAT3 direction;
		float id;
		XMFLOAT4 specularColour;
		XMFLOAT3 cameraOption;
		float specularPower;

		float lightRange;
		float lightInnerCone;
		float lightOuterCone;
		float padding2;

	};

	struct CameraBufferType
	{
		XMFLOAT3 CameraPosition;
		float padding;
	};
	struct LightInfoType
	{
		XMFLOAT3 lightType;
		float id;
	};

public:
	LightShader(ID3D11Device* device, HWND hwnd);
	~LightShader();

	void setShaderParameters(ID3D11DeviceContext* deviceContext, const XMMATRIX& world, const XMMATRIX& view, const XMMATRIX& projection, ID3D11ShaderResourceView* texture, Light* light, FPCamera* cam, float _id, XMFLOAT3 camType, float _lightRange, float _lightInnerCone, float _lightOuterCone);

private:
	void initShader(const wchar_t* cs, const wchar_t* ps);

private:
	ID3D11Buffer* matrixBuffer;
	ID3D11SamplerState* sampleState;
	ID3D11Buffer* lightBuffer;
	ID3D11Buffer* cameraBuffer;
	ID3D11Buffer* lightInfoBuffer;
};


