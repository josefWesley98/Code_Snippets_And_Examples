// Application.h
#ifndef _APP1_H
#define _APP1_H

// Includes
#include "DXF.h"
#include "ManipulationShader.h"
#include "TessellationShader.h"
#include "LightShader.h"
#include "ShadowShader.h"
#include "DepthShader.h"
#include "TextureShader.h"
#include "PostProcessingShader.h"

class App1 : public BaseApplication
{
public:

	App1();
	~App1();
	void init(HINSTANCE hinstance, HWND hwnd, int screenWidth, int screenHeight, Input* in, bool VSYNC, bool FULL_SCREEN);
	bool frame();

protected:
	bool render();
	void FirstPass();
	void Pixelate();
	void DepthPass();
	void FinalPass();
	void DoRotations();
	void gui();
	void RenderToTexture();
	void SceneSetup();
private:

	XMMATRIX CM;
	XMMATRIX CMHolder;
	XMMATRIX CMRotation;

	XMMATRIX SM;
	XMMATRIX SMHolder;
	XMMATRIX SMRotation;

	CubeMesh* cubeMesh;

	ManipulationShader* manipulationShader;
	TessellationShader* tesselationShader;
	ShadowShader* shadowShader;
	DepthShader* depthShader;
	LightShader* lightShader;

	TextureShader* textureShader;
	RenderTexture* renderTexture;
	RenderTexture* renderPassTexture;
	//RenderTexture* horizontalBlurTexture;
	//RenderTexture* verticalBlurTexture;
	RenderTexture* boxBlurTexture;


	PostProcessingShader* postProcessingShader;
	ShadowMap* shadowMap;
	PlaneMesh* waterMesh;
	PlaneMesh* waterMesh1;
	PlaneMesh* mesh;
	PlaneMesh* mesh2;
	OrthoMesh* orthoMesh;
	//TessellationMesh* waterMesh;
	
	//TessellationMesh* mesh;
	Light* light[2];
	Timer* theTimer;
	float check;
	float values[4];
	float time;
	float change;
	float rateOfChange;
	float rotationChange;
	float rateOfRotationChange;
	float pointLightRadius;
	bool changeBool;
	int activeLight;
	float doDirectionalLight, doPointLight;
	float outerSpotlightRadius, innerSpotlightRadius, spotlightRange;
	float constantAttenuation, linearAttenuation, quadraticAttenuation;

	bool doPixel;
	float pixelLevel;
	float directionX, directionY, directionZ;
	float positionX, positionY, positionZ;
	float camId;

	float isUsingShadowMap;
	bool doShadowMap;
	float lightPosX, lightPosY, lightPosZ;
	float lightDirX, lightDirY, lightDirZ;

	float moveX, moveY, moveZ;
};

#endif