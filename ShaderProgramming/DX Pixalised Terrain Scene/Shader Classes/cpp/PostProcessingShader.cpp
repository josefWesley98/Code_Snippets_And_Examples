#include "PostProcessingShader.h"
//#include <iostream>

PostProcessingShader::PostProcessingShader(ID3D11Device* device, HWND hwnd) : BaseShader(device, hwnd)
{
	initShader(L"postprocessing_vs.cso", L"postprocessing_ps.cso");
}

PostProcessingShader::~PostProcessingShader()
{
	// Release the sampler state.
	if (sampleState)
	{
		sampleState->Release();
		sampleState = 0;
	}

	if (sampleState1)
	{
		sampleState->Release();
		sampleState = 0;
	}
	// Release the pixel size constant buffer.
	if (pixelSizeBuffer)
	{
		pixelSizeBuffer->Release();
		pixelSizeBuffer = 0;
	}

	// Release the layout.
	if (layout)
	{
		layout->Release();
		layout = 0;
	}
	if (screenSizeBuffer)
	{
		screenSizeBuffer->Release();
		screenSizeBuffer = 0;
	}

	//Release base shader components
	BaseShader::~BaseShader();
}

void PostProcessingShader::initShader(const wchar_t* vsFilename, const wchar_t* psFilename)
{
	D3D11_BUFFER_DESC matrixBufferDesc;
	D3D11_BUFFER_DESC pixelSizeBufferDesc;
	D3D11_SAMPLER_DESC samplerDesc;
	D3D11_SAMPLER_DESC samplerDesc1;
	D3D11_BUFFER_DESC screenSizeBufferDesc;

	// Load (+ compile) shader files
	loadVertexShader(vsFilename);
	loadPixelShader(psFilename);


	// Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
	matrixBufferDesc.Usage = D3D11_USAGE_DYNAMIC;
	matrixBufferDesc.ByteWidth = sizeof(MatrixBufferType);
	matrixBufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	matrixBufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	matrixBufferDesc.MiscFlags = 0;
	matrixBufferDesc.StructureByteStride = 0;
	renderer->CreateBuffer(&matrixBufferDesc, NULL, &matrixBuffer);

	// Setup the description of the pixel size constant buffer that is in the pixel shader.
	pixelSizeBufferDesc.Usage = D3D11_USAGE_DYNAMIC;
	pixelSizeBufferDesc.ByteWidth = sizeof(PixelSizeBufferType);
	pixelSizeBufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	pixelSizeBufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	pixelSizeBufferDesc.MiscFlags = 0;
	pixelSizeBufferDesc.StructureByteStride = 0;
	renderer->CreateBuffer(&pixelSizeBufferDesc, NULL, &pixelSizeBuffer);

	// Create a texture sampler state description.
	samplerDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
	samplerDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
	samplerDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
	samplerDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
	samplerDesc.MipLODBias = 0.0f;
	samplerDesc.MaxAnisotropy = 1;
	samplerDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
	samplerDesc.BorderColor[0] = 0;
	samplerDesc.BorderColor[1] = 0;
	samplerDesc.BorderColor[2] = 0;
	samplerDesc.BorderColor[3] = 0;
	samplerDesc.MinLOD = 0;
	samplerDesc.MaxLOD = 0;
	renderer->CreateSamplerState(&samplerDesc, &sampleState);

	samplerDesc1.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
	samplerDesc1.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
	samplerDesc1.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
	samplerDesc1.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
	samplerDesc1.MipLODBias = 0.0f;
	samplerDesc1.MaxAnisotropy = 1;
	samplerDesc1.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
	samplerDesc1.BorderColor[0] = 0;
	samplerDesc1.BorderColor[1] = 0;
	samplerDesc1.BorderColor[2] = 0;
	samplerDesc1.BorderColor[3] = 0;
	samplerDesc1.MinLOD = 0;
	samplerDesc1.MaxLOD = 0;
	renderer->CreateSamplerState(&samplerDesc1, &sampleState1);

	screenSizeBufferDesc.Usage = D3D11_USAGE_DYNAMIC;
	screenSizeBufferDesc.ByteWidth = sizeof(ScreenSizeBufferType);
	screenSizeBufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	screenSizeBufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	screenSizeBufferDesc.MiscFlags = 0;
	screenSizeBufferDesc.StructureByteStride = 0;
	renderer->CreateBuffer(&screenSizeBufferDesc, NULL, &screenSizeBuffer);

}

void PostProcessingShader::setShaderParameters(ID3D11DeviceContext* deviceContext, const XMMATRIX& world, const XMMATRIX& view, const XMMATRIX& projection, ID3D11ShaderResourceView* texture, ID3D11ShaderResourceView* texture1, float pixelSize)
{

	D3D11_MAPPED_SUBRESOURCE mappedResource;
	MatrixBufferType* dataPtr;
	XMMATRIX tworld, tview, tproj;

	// Transpose the matrices to prepare them for the shader.
	tworld = XMMatrixTranspose(world);
	tview = XMMatrixTranspose(view);
	tproj = XMMatrixTranspose(projection);

	// Set the constant buffer for matrices in the vertex shader
	deviceContext->Map(matrixBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
	dataPtr = (MatrixBufferType*)mappedResource.pData;
	dataPtr->world = tworld;
	dataPtr->view = tview;
	dataPtr->projection = tproj;
	deviceContext->Unmap(matrixBuffer, 0);
	deviceContext->VSSetConstantBuffers(0, 1, &matrixBuffer);

	// Send screen size data to the vertex shader
	ScreenSizeBufferType* dataPtr1;
	deviceContext->Map(screenSizeBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
	dataPtr1 = (ScreenSizeBufferType*)mappedResource.pData;
	dataPtr1->screenWidth = 0;
	dataPtr1->screenHeight = 0;
	dataPtr1->padding = XMFLOAT2(0.0f, 0.0f);
	deviceContext->Unmap(screenSizeBuffer, 0);
	deviceContext->VSSetConstantBuffers(1, 1, &screenSizeBuffer);

	// Send pixel size data to the pixel shader
	PixelSizeBufferType* dataPtr2;
	deviceContext->Map(pixelSizeBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
	dataPtr2 = (PixelSizeBufferType*)mappedResource.pData;
	dataPtr2->pixelSize = pixelSize;
	dataPtr2->padding = XMFLOAT3(0, 0, 0);
	deviceContext->Unmap(pixelSizeBuffer, 0);
	deviceContext->PSSetConstantBuffers(0, 1, &pixelSizeBuffer);

	// Set the shader textures in the pixel shader
	deviceContext->PSSetShaderResources(0, 1, &texture);
	deviceContext->PSSetShaderResources(1, 1, &texture1);

	// Set the sampler states in the pixel shader
	deviceContext->PSSetSamplers(0, 1, &sampleState);
	deviceContext->PSSetSamplers(1, 1, &sampleState1);

}