
#include "App1.h"

App1::App1()
{
	mesh = nullptr;
	waterMesh = nullptr;
	manipulationShader = nullptr;
	tesselationShader = nullptr;
	textureShader = nullptr;
	lightShader = nullptr;
}

void App1::init(HINSTANCE hinstance, HWND hwnd, int screenWidth, int screenHeight, Input *in, bool VSYNC, bool FULL_SCREEN)
{
	// Call super/parent init function (required!)
	BaseApplication::init(hinstance, hwnd, screenWidth, screenHeight, in, VSYNC, FULL_SCREEN);
	
	cubeMesh = new CubeMesh(renderer->getDevice(), renderer->getDeviceContext());

	textureMgr->loadTexture(L"ground", L"res/MountainRange.png");
	textureMgr->loadTexture(L"heightmap", L"res/MountainRangeHeightMap.png");
	textureMgr->loadTexture(L"water1", L"res/water1.jpg");
	textureMgr->loadTexture(L"water2", L"res/water2.png");
	textureMgr->loadTexture(L"dithering", L"res/checkerboard.png");
	
	// Initalise scene variables.
	mesh = new PlaneMesh(renderer->getDevice(), renderer->getDeviceContext());
	mesh2 = new PlaneMesh(renderer->getDevice(), renderer->getDeviceContext());
	waterMesh = new PlaneMesh(renderer->getDevice(), renderer->getDeviceContext());
	waterMesh1 = new PlaneMesh(renderer->getDevice(), renderer->getDeviceContext());
	//waterMesh = new TessellationMesh(renderer->getDevice(), renderer->getDeviceContext());
	moveX, moveY, moveZ = 0;

	orthoMesh = new OrthoMesh(renderer->getDevice(), renderer->getDeviceContext(), screenWidth, screenHeight);
	

	manipulationShader = new ManipulationShader(renderer->getDevice(), hwnd);
	tesselationShader = new TessellationShader(renderer->getDevice(), hwnd);
	textureShader = new TextureShader(renderer->getDevice(), hwnd);
	lightShader = new LightShader(renderer->getDevice(), hwnd);
	depthShader = new DepthShader(renderer->getDevice(), hwnd);
	shadowShader = new ShadowShader(renderer->getDevice(), hwnd);
	
	
	postProcessingShader = new PostProcessingShader(renderer->getDevice(), hwnd);

	renderTexture = new RenderTexture(renderer->getDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);
	renderPassTexture = new RenderTexture(renderer->getDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);

	// Confirgure directional light
	//renderTexture = new RenderTexture(renderer->getDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);
	//horizontalBlurTexture = new RenderTexture(renderer->getDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);
	//verticalBlurTexture = new RenderTexture(renderer->getDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);
	
	isUsingShadowMap = 0;

	int shadowmapWidth = 1024 * 4;
	int shadowmapHeight = 1024 * 4;
	int sceneWidth = 100;
	int sceneHeight = 100;

	outerSpotlightRadius = 10.0f;
	innerSpotlightRadius = 20.0f;
	spotlightRange = 100.0f;

	constantAttenuation = 0.5f;
	linearAttenuation = 0.125f;
	quadraticAttenuation = 0.15f;


	doPixel = false;
	pixelLevel = 0.005f;

	shadowMap = new ShadowMap(renderer->getDevice(), shadowmapWidth, shadowmapHeight);
	pointLightRadius = 100.0f;
	for (int i = 0; i < 2; i++)
	{
		light[i] = new Light();
	}
	doShadowMap = false;
	activeLight = 1;
	theTimer = new Timer();
	check = 2;
	time = 0;
	camId = 0;
	change = 10;
	rotationChange = 0;
	CM = XMMatrixTranslation(-change, 2.5, -15);
	SM = XMMatrixTranslation(change, 2.5, -5);
	directionX = 0.0f;
	directionY = -0.7f;
	directionZ = 0.7f;
	positionX = 0;
	positionY = 0;
	positionZ = -10.0f;
	rateOfRotationChange = 0.05f;
	rateOfChange = 0.1f;
	check = 1;

	lightPosX = 0.0f;
	lightPosY = 10.0f;
	lightPosZ = 0.0f;
	
	lightDirX = 0.0f;
	lightDirY = -1.0f;
	lightDirZ = -1.0f;
	for (int i = 0; i < 2; i++)
	{
		light[i]->setDirection(lightDirX, lightDirY, lightDirZ);
		light[i]->setPosition(lightPosX, lightPosY, lightPosZ);
		light[i]->setDiffuseColour(0.5f, 0.5f, 1.0f, 1.0f);
		light[i]->setAmbientColour(0.2f, 0.2f, 0.5f, 1.0f);
		light[i]->setSpecularColour(1.0f, 1.0f, 1.0f, 1.0f);
	/*	light[i]->setDiffuseColour(1.0f, 0.0f, 0.0f, 1.0f);
		light[i]->setAmbientColour(1.0f, 0.0f, 0.0f, 1.0f);
		light[i]->setSpecularColour(1.0f, 0.0f, 0.0f, 1.0f);*/
		light[i]->setSpecularPower(80.0f);
	}

	values[0] = 3;
	values[1] = 3;
	values[2] = 3;
	values[3] = 3;
}


App1::~App1()
{
	// Run base application deconstructor
	BaseApplication::~BaseApplication();

	 //Release the Direct3D object.
	if (mesh)
	{
		delete mesh;
		mesh = 0;
	}

	if (waterMesh)
	{
		delete waterMesh;
		waterMesh = 0;
	}

	if (manipulationShader)
	{
		delete manipulationShader;
		manipulationShader = 0;
	}
	if (tesselationShader)
	{
		delete tesselationShader;
		tesselationShader = 0;
	}
	if (textureShader)
	{
		delete textureShader;
		textureShader = 0;
	}
	if (lightShader)
	{
		delete lightShader;
		lightShader = 0;
	}
}


bool App1::frame()
{
	bool result;

	result = BaseApplication::frame();
	if (!result)
	{
		return false;
	}
	
	// Render the graphics.
	result = render();
	if (!result)
	{
		return false;
	}

	return true;
}

bool App1::render()
{
	if (doShadowMap)
	{
		isUsingShadowMap = 1;
	}
	if (!doShadowMap)
	{
		isUsingShadowMap = 0;
	}
	if (activeLight == 0)
	{
		doDirectionalLight = 1;
		doPointLight = 0;
	}
	if (activeLight == 1)
	{
		doDirectionalLight = 0;
		doPointLight = 1;
	}
	theTimer->frame();
	time += theTimer->getTime() * 2;
	//DoRotations();
	FirstPass();
	Pixelate();
	DepthPass();
	FinalPass();

	return true;
}
void App1::SceneSetup()
{
	light[activeLight]->generateViewMatrix();
	XMMATRIX worldMatrix = renderer->getWorldMatrix();
	XMMATRIX viewMatrix = camera->getViewMatrix();
	XMMATRIX projectionMatrix = renderer->getProjectionMatrix();

	light[activeLight]->setPosition(lightPosX, lightPosY, lightPosZ);
	light[activeLight]->setDirection(lightDirX, lightDirY, lightDirZ);

	waterMesh->sendData(renderer->getDeviceContext());
	manipulationShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, viewMatrix, projectionMatrix, textureMgr->getTexture(NULL), textureMgr->getTexture(L"water1"), shadowMap->getDepthMapSRV(), light[activeLight], doDirectionalLight, doPointLight, pointLightRadius, check, time, camera, constantAttenuation, linearAttenuation, quadraticAttenuation, outerSpotlightRadius, innerSpotlightRadius, spotlightRange, isUsingShadowMap);
	manipulationShader->render(renderer->getDeviceContext(), waterMesh->getIndexCount());
	check += 1;

	waterMesh1->sendData(renderer->getDeviceContext());
	manipulationShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, viewMatrix, projectionMatrix, textureMgr->getTexture(NULL), textureMgr->getTexture(L"water2"), shadowMap->getDepthMapSRV(), light[activeLight], doDirectionalLight, doPointLight, pointLightRadius, check, time, camera, constantAttenuation, linearAttenuation, quadraticAttenuation, outerSpotlightRadius, innerSpotlightRadius, spotlightRange, isUsingShadowMap);
	manipulationShader->render(renderer->getDeviceContext(), waterMesh1->getIndexCount());
	check += 1;

	mesh->sendData(renderer->getDeviceContext());
	//shadowShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, viewMatrix, projectionMatrix, textureMgr->getTexture(L"ground"), textureMgr->getTexture(L"heightmap"), shadowMap->getDepthMapSRV(), light[activeLight], a);
	manipulationShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, viewMatrix, projectionMatrix, textureMgr->getTexture(L"heightmap"), textureMgr->getTexture(L"ground"), shadowMap->getDepthMapSRV(), light[activeLight], doDirectionalLight, doPointLight, pointLightRadius, check, time, camera, constantAttenuation, linearAttenuation, quadraticAttenuation, outerSpotlightRadius, innerSpotlightRadius, spotlightRange, isUsingShadowMap);
	manipulationShader->render(renderer->getDeviceContext(), mesh->getIndexCount());

	check += 1;
	XMMATRIX pos = XMMatrixTranslation(moveX, moveY, moveZ);
	cubeMesh->sendData(renderer->getDeviceContext());
	manipulationShader->setShaderParameters(renderer->getDeviceContext(), pos, viewMatrix, projectionMatrix, textureMgr->getTexture(L"heightmap"), textureMgr->getTexture(L"ground"), shadowMap->getDepthMapSRV(), light[activeLight], doDirectionalLight, doPointLight, pointLightRadius, check, time, camera, constantAttenuation, linearAttenuation, quadraticAttenuation, outerSpotlightRadius, innerSpotlightRadius, spotlightRange, isUsingShadowMap);
	manipulationShader->render(renderer->getDeviceContext(), cubeMesh->getIndexCount());
	check = 1;
}
void App1::FirstPass()
{

	renderTexture->setRenderTarget(renderer->getDeviceContext());
	renderTexture->clearRenderTarget(renderer->getDeviceContext(), 0.39f, 0.58f, 0.92f, 1.0f);

	// Get matrices
	camera->update();
	XMMATRIX worldMatrix = renderer->getWorldMatrix();
	XMMATRIX viewMatrix = camera->getViewMatrix();
	XMMATRIX projectionMatrix = renderer->getProjectionMatrix();

	if (doPixel)
	{
		SceneSetup();
	}

	renderer->setBackBufferRenderTarget();

}
void App1::Pixelate()
{
	XMMATRIX worldMatrix, baseViewMatrix, orthoMatrix;


	renderPassTexture->setRenderTarget(renderer->getDeviceContext());
	renderPassTexture->clearRenderTarget(renderer->getDeviceContext(), 1.0f, 1.0f, 0.0f, 1.0f);

	worldMatrix = renderer->getWorldMatrix();
	baseViewMatrix = camera->getOrthoViewMatrix();
	orthoMatrix = renderPassTexture->getOrthoMatrix();

	// Render for Horizontal Blur
	renderer->setZBuffer(false);

	orthoMesh->sendData(renderer->getDeviceContext());
	postProcessingShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, baseViewMatrix, orthoMatrix, renderTexture->getShaderResourceView(), textureMgr->getTexture(L"dithering"), pixelLevel);
	postProcessingShader->render(renderer->getDeviceContext(), orthoMesh->getIndexCount());

	renderer->setZBuffer(true);

	// Reset the render target back to the original back buffer and not the render to texture anymore.
	renderer->setBackBufferRenderTarget();
}
void App1::FinalPass()
{
	// Clear the scene. (default blue colour)
	renderer->beginScene(0.39f, 0.58f, 0.92f, 1.0f);

	// RENDER THE RENDER TEXTURE SCENE
	// Requires 2D rendering and an ortho mesh.
	renderer->setZBuffer(false);
	
	XMMATRIX worldMatrix = renderer->getWorldMatrix();
	XMMATRIX orthoMatrix = renderer->getOrthoMatrix();  // ortho matrix for 2D rendering
	XMMATRIX orthoViewMatrix = camera->getOrthoViewMatrix();	// Default camera position for orthographic rendering
	
	orthoMesh->sendData(renderer->getDeviceContext());
	textureShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, orthoViewMatrix, orthoMatrix, renderPassTexture->getShaderResourceView());
	textureShader->render(renderer->getDeviceContext(), orthoMesh->getIndexCount());
	
	renderer->setZBuffer(true);

	if (!doPixel)
	{
		SceneSetup();
	}
	

	// Render GUI
	gui();

	// Present the rendered scene to the screen.
	renderer->endScene();
}
void App1::DepthPass()
{
	// Set the render target to be the render to texture.
	shadowMap->BindDsvAndSetNullRenderTarget(renderer->getDeviceContext());

	// get the world, view, and projection matrices from the camera and d3d objects.
	//light->generateViewMatrix();
	XMMATRIX lightViewMatrix = light[activeLight]->getViewMatrix();
	XMMATRIX lightProjectionMatrix = light[activeLight]->getOrthoMatrix();
	XMMATRIX worldMatrix = renderer->getWorldMatrix();
	worldMatrix = XMMatrixTranslation(-50.f, 0.f, -10.f);
	XMMATRIX pos = XMMatrixTranslation(0.0f, 10.0f, 0.0f);
	// Render floor

	
	mesh->sendData(renderer->getDeviceContext());
	depthShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, lightViewMatrix, lightProjectionMatrix);
	depthShader->render(renderer->getDeviceContext(), mesh->getIndexCount());

	cubeMesh->sendData(renderer->getDeviceContext());
	depthShader->setShaderParameters(renderer->getDeviceContext(), pos, lightViewMatrix, lightProjectionMatrix);
	depthShader->render(renderer->getDeviceContext(), cubeMesh->getIndexCount());


	waterMesh->sendData(renderer->getDeviceContext());
	depthShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, lightViewMatrix, lightProjectionMatrix);
	depthShader->render(renderer->getDeviceContext(), waterMesh->getIndexCount());
		
	waterMesh1->sendData(renderer->getDeviceContext());
	depthShader->setShaderParameters(renderer->getDeviceContext(), worldMatrix, lightViewMatrix, lightProjectionMatrix);
	depthShader->render(renderer->getDeviceContext(), waterMesh1->getIndexCount());

	// Set back buffer as render target and reset view port.
	renderer->setBackBufferRenderTarget();
	renderer->resetViewport();
}

void App1::DoRotations()
{

	if (changeBool)
	{
		change += rateOfChange;
	}
	else if (!changeBool)
	{
		change -= rateOfChange;
	}
	if (change <= 1)
	{
		changeBool = true;
	}
	else if (change >= 10)
	{
		changeBool = false;
	}

	rotationChange += 0.05f;

	if (rotationChange >= 360.0f)
	{
		rotationChange = rateOfRotationChange;
	}

	SMHolder = XMMatrixTranslation(-change, 2.5, -5);
	DirectX::GXMVECTOR changeRotationSM = DirectX::XMVectorSet(0, 0, rotationChange, 1);
	SMRotation = XMMatrixRotationAxis(changeRotationSM, rotationChange);
	SM = XMMatrixMultiply(SMRotation, SMHolder);

	CMHolder = XMMatrixTranslation(change, 2.5, -5);
	DirectX::GXMVECTOR changeRotationCM = DirectX::XMVectorSet(0, rotationChange, 0, 1);
	CMRotation = XMMatrixRotationAxis(changeRotationCM, rotationChange);
	CM = XMMatrixMultiply(CMRotation, CMHolder);
}

void App1::gui()
{
	// Force turn off unnecessary shader stages.
	renderer->getDeviceContext()->GSSetShader(NULL, NULL, 0);
	renderer->getDeviceContext()->HSSetShader(NULL, NULL, 0);
	renderer->getDeviceContext()->DSSetShader(NULL, NULL, 0);

	// Build UI
	ImGui::Text("FPS: %.2f", timer->getFPS());
	ImGui::Text("runtime: %.2f", time);

	ImGui::Checkbox("Wireframe mode", &wireframeToggle);
	ImGui::SliderInt("Active Light: ", &activeLight, 0, 1);

	ImGui::SliderFloat("Light Pos X: ", &lightPosX, -100, 250);
	ImGui::SliderFloat("Light Pos Y: ", &lightPosY, -100, 250);
	ImGui::SliderFloat("Light Pos Z: ", &lightPosZ, -100, 250);

	ImGui::SliderFloat("Light Dir X: ", &lightDirX, -1, 1);
	ImGui::SliderFloat("Light Dir Y: ", &lightDirY, -1, 1);
	ImGui::SliderFloat("Light Dir Z: ", &lightDirZ, -1, 1);

	ImGui::SliderFloat("Cube X: ", &moveX, -100, 100);
	ImGui::SliderFloat("Cube Y: ", &moveY, -100, 100);
	ImGui::SliderFloat("Cube Z: ", &moveZ, -100, 100);

	
	ImGui::Checkbox("Do Shadow Map? : ", &doShadowMap);
	ImGui::Checkbox("Pixelate? : ", &doPixel);
	ImGui::SliderFloat("Pixel Intensity :", &pixelLevel, 0.001, 0.1);

	ImGui::SliderFloat("constant Attenuation: ", &constantAttenuation, 0, 1);
	ImGui::SliderFloat("linear Attenuation: ", &linearAttenuation, 0, 1);
	ImGui::SliderFloat("quadratic Attenuation: ", &quadraticAttenuation, 0, 1);
	//ImGui::SliderFloat("Edge 1: ", &values[0], 1, 64);
	//ImGui::SliderFloat("Edge 2: ", &values[1], 1, 64);
	//ImGui::SliderFloat("Edge 3: ", &values[2], 1, 64);
	//ImGui::SliderFloat("Inside: ", &values[3], 1, 64);

	// Render UI
	ImGui::Render();
	ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
}

