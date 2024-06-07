// tessellation shader.cpp
#include "TessellationShader.h"


TessellationShader::TessellationShader(ID3D11Device* device, HWND hwnd) : BaseShader(device, hwnd)
{
	initShader(L"tessellation_vs.cso", L"tessellation_hs.cso", L"tessellation_ds.cso", L"tessellation_ps.cso");
}

TessellationShader::~TessellationShader()
{
	if (sampleState)
	{
		sampleState->Release();
		sampleState = 0;
	}
	if (matrixBuffer)
	{
		matrixBuffer->Release();
		matrixBuffer = 0;
	}
	if (factorsBuffer)
	{
		factorsBuffer->Release();
		factorsBuffer = 0;
	}
	if (layout)
	{
		layout->Release();
		layout = 0;
	}

	//Release base shader components
	BaseShader::~BaseShader();
}

void TessellationShader::initShader(const wchar_t* vsFilename, const wchar_t* psFilename)
{


	// Load (+ compile) shader files
	loadVertexShader(vsFilename);
	loadPixelShader(psFilename);

	// Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
	D3D11_BUFFER_DESC matrixBufferDesc;
	matrixBufferDesc.Usage = D3D11_USAGE_DYNAMIC;
	matrixBufferDesc.ByteWidth = sizeof(MatrixBufferType);
	matrixBufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	matrixBufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	matrixBufferDesc.MiscFlags = 0;
	matrixBufferDesc.StructureByteStride = 0;

	renderer->CreateBuffer(&matrixBufferDesc, NULL, &matrixBuffer);

	D3D11_BUFFER_DESC factorBufferDesc;
	factorBufferDesc.Usage = D3D11_USAGE_DYNAMIC;
	factorBufferDesc.ByteWidth = sizeof(Factors);
	factorBufferDesc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	factorBufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	factorBufferDesc.MiscFlags = 0;
	factorBufferDesc.StructureByteStride = 0;

	renderer->CreateBuffer(&factorBufferDesc, NULL, &factorsBuffer);


}

void TessellationShader::initShader(const wchar_t* vsFilename, const wchar_t* hsFilename, const wchar_t* dsFilename, const wchar_t* psFilename)
{
	// InitShader must be overwritten and it will load both vertex and pixel shaders + setup buffers
	initShader(vsFilename, psFilename);

	// Load other required shaders.
	loadHullShader(hsFilename);
	loadDomainShader(dsFilename);
}


void TessellationShader::setShaderParameters(ID3D11DeviceContext* deviceContext, const XMMATRIX& worldMatrix, const XMMATRIX& viewMatrix, const XMMATRIX& projectionMatrix, float& newValue1, float& newValue2, float& newValue3, float& newValue4)
{
	HRESULT result;

	D3D11_MAPPED_SUBRESOURCE mappedResource;
	HRESULT result1;

	D3D11_MAPPED_SUBRESOURCE mappedResource1;

	// Transpose the matrices to prepare them for the shader.
	XMMATRIX tworld = XMMatrixTranspose(worldMatrix);
	XMMATRIX tview = XMMatrixTranspose(viewMatrix);
	XMMATRIX tproj = XMMatrixTranspose(projectionMatrix);

	// Lock the constant buffer so it can be written to.
	result = deviceContext->Map(matrixBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
	MatrixBufferType* dataPtr = (MatrixBufferType*)mappedResource.pData;
	dataPtr->world = tworld;// worldMatrix;
	dataPtr->view = tview;
	dataPtr->projection = tproj;
	deviceContext->Unmap(matrixBuffer, 0);
	deviceContext->DSSetConstantBuffers(0, 1, &matrixBuffer);

	result1 = deviceContext->Map(factorsBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource1);
	Factors* factorPtr = (Factors*)mappedResource1.pData;

	factorPtr->value1 = newValue1;
	factorPtr->value2 = newValue2;
	factorPtr->value3 = newValue3;
	factorPtr->value4 = newValue4;

	deviceContext->Unmap(factorsBuffer, 0);
	deviceContext->HSSetConstantBuffers(1, 1, &factorsBuffer);
}
