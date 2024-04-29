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

    /*vector<Vertex> vec(3);

    vec[0].pos = Vec3(0, 0.5f, 0.5f);
    vec[0].color = Vec4(1.0f, 0.f, 0.f, 1.0f);

    vec[1].pos = Vec3(0.5f, -0.5f, 0.5f);
    vec[1].color = Vec4(0, 1.f, 0.f, 1.f);
    
    vec[2].pos = Vec3(-.5f, -.5f, .5f);
    vec[2].color = Vec4(0, 0.f, 1.f, 1.0f);*/

    // VBV 버텍스 6개로 사각형 그리기 코드
    /*vector<Vertex> vec(6);
    vec[0].pos = Vec3(-0.5f, 0.5f, 0.5f);
    vec[0].color = Vec4(1.f, 0.f, 0.f, 1.f);
    vec[1].pos = Vec3(0.5f, 0.5f, 0.5f);
    vec[1].color = Vec4(0.f, 1.f, 0.f, 1.f);
    vec[2].pos = Vec3(0.5f, -0.5f, 0.5f);
    vec[2].color = Vec4(0.f, 0.f, 1.f, 1.f);

    vec[3].pos = Vec3(0.5f, -0.5f, 0.5f);
    vec[3].color = Vec4(0.f, 0.f, 1.f, 1.f);
    vec[4].pos = Vec3(-0.5f, -0.5f, 0.5f);
    vec[4].color = Vec4(0.f, 1.f, 0.f, 1.f);
    vec[5].pos = Vec3(-0.5f, 0.5f, 0.5f);
    vec[5].color = Vec4(1.f, 0.f, 0.f, 1.f);*/

    // VBV + IBV로 사각형 그려보기 코드
    vector<Vertex> vec(4);
    vec[0].pos = Vec3(-0.5f, 0.5f, 0.5f);
    vec[0].color = Vec4(1.f, 0.f, 0.f, 1.f);
    vec[1].pos = Vec3(0.5f, 0.5f, 0.5f);
    vec[1].color = Vec4(0.f, 1.f, 0.f, 1.f);
    vec[2].pos = Vec3(0.5f, -0.5f, 0.5f);
    vec[2].color = Vec4(0.f, 0.f, 1.f, 1.f);
    vec[3].pos = Vec3(-0.5f, -0.5f, 0.5f);
    vec[3].color = Vec4(0.f, 1.f, 0.f, 1.f);

    vector<uint32> indexVec;
    {
        indexVec.push_back(0);
        indexVec.push_back(1);
        indexVec.push_back(2);
    }
    {
        indexVec.push_back(0);
        indexVec.push_back(2);
        indexVec.push_back(3);
    }

    mesh->Init(vec, indexVec);
    shader->Init(L"..\\Resources\\Shader\\default.hlsli");
    
    GEngine->GetCmdQueue()->WaitSync();
}

void Game::Update()
{
    //GEngine->Render();

    GEngine->RenderBegin();

    // 메쉬와 쉐이더 렌더링 업데이트
    shader->Update();
    /*mesh->Render();*/

    //삼각형1
    //{
    //    Transform t;
    //    t.offset = Vec4(0.75f, 0.f, 0.f, 0.f);
    //    mesh->SetTransform(t);
    //    mesh->Render();
    //}
    ////삼각형2
    //{
    //    Transform t;
    //    t.offset = Vec4(0.f, 0.75f, 0.f, 0.f);
    //    mesh->SetTransform(t);
    //    mesh->Render();
    //}

    //VBV
    {
        Transform t;
        t.offset = Vec4(0.f, 0.f, 0.f, 0.f);
        mesh->SetTransform(t);
        mesh->Render();
    }

    GEngine->RenderEnd();
}