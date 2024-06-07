#pragma once
//#include "BaseShader.h"
#include "DXF.h"

using namespace std;
using namespace DirectX;

class PostProcessingShader : public BaseShader
{
	private:
	struct PixelSizeBufferType
	{
		float pixelSize;
		XMFLOAT3 padding;
	};

	struct ScreenSizeBufferType
	{
		float screenWidth;
		float screenHeight;

		XMFLOAT2 padding;
	};
public:
	PostProcessingShader(ID3D11Device* device, HWND hwnd);
	~PostProcessingShader();

	void setShaderParameters(ID3D11DeviceContext* deviceContext, const XMMATRIX& world, const XMMATRIX& view, const XMMATRIX& projection, ID3D11ShaderResourceView* texture, ID3D11ShaderResourceView* texture1,  float pixelSize);

private:
	void initShader(const wchar_t* vsFilename, const wchar_t* psFilename);

private:
	ID3D11Buffer* pixelSizeBuffer;
	
	ID3D11Buffer* matrixBuffer;
	ID3D11SamplerState* sampleState;
	ID3D11SamplerState* sampleState1;
	ID3D11Buffer* screenSizeBuffer;
};

