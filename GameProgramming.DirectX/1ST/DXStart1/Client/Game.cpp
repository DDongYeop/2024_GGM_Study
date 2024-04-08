#include "pch.h"
#include "Game.h"
#include "Engine.h"

shared_ptr<Mesh> mesh = make_shared<Mesh>();
shared_ptr<Shader> shader = make_shared<Shader>();

void Game::Init(const WindowInfo& wInfo)
{
	GEngine->Init(wInfo);

	//삼각형 띄우기 테스트

	//정점 3개 만들기
	vector<Vertex> vec(3);
	vec[0].pos = Vec3(0, .5f, .5f);		vec[0].color = Vec4(1, 0, 0, 1);
	vec[1].pos = Vec3(.5f, -.5f, .5f);	vec[1].color = Vec4(0, 1, 0, 1);
	vec[2].pos = Vec3(-.5f, -.5f, .5f);	vec[2].color = Vec4(0, 0, 1, 1);
	mesh->Init(vec);

	//물리적인 파일 위치에서 쉐이더 읽기
	wstring str = L"..\\Resources\\Shader\\default.hlsli";
	shader->Init(str);

	GEngine->GetCmdQueue()->WaitSync();
}

void Game::Update()
{
	// GEngine->Render();

	GEngine->RenderBegin();

	//메쉬 쉐이더 렌더링 업데이트 
	shader->Update();
	mesh->Render();

	GEngine->RenderEnd();
}
