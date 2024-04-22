#include "pch.h"
#include "Game.h"
#include "Engine.h"
    

shared_ptr<Shader> shader = make_shared<Shader>();
shared_ptr<Mesh> mesh = make_shared<Mesh>();

void Game::Init(const WindowInfo& wInfo)
{
    GEngine->Init(wInfo);

    //삼각형 띄우기 테스트 코드

    // 힌트 정점 3개를 만드시오
    Vertex triangle[] = {
        { {0.0f,0.3f,0.0f},{1.f,0.f,0.f,1.f}},
        { {0.3f,-0.3f,0.0f},{1.f,1.f,0.f,1.f}},
        { {-0.3f,-0.3f,0.0f},{1.f,0.f,1.f,1.f}},
    };

    vector<Vertex> v;
    for (int i = 0; i < 3; i++)
        v.push_back(triangle[i]);

    // 물리적인 파일의 위치에서 쉐이더 파일을 읽어오시오
    wstring path = L"..\\Resources\\Shader\\default.hlsli";
    shader->Init(path);
    mesh->Init(v);
    
    GEngine->GetCmdQueue()->WaitSync();
}

void Game::Update()
{
    //GEngine->Render();

    GEngine->RenderBegin();

    // 메쉬와 쉐이더 렌더링 업데이트
    shader->Update();
    mesh->Render();

    GEngine->RenderEnd();
}