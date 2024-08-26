#include "pch.h"
#include "SceneManager.h"
#include "Scene.h"

#include "Engine.h"
#include "Material.h"
#include "GameObject.h"
#include "MeshRenderer.h"

void SceneManager::Update()
{
    if (_activeScene == nullptr)
        return;

    _activeScene->Update();
    _activeScene->LateUpdate();
	_activeScene->FinalUpdate();
}

void SceneManager::LoadScene(wstring sceneName)
{
    // TODO : 기존 Scene 정리
    // TODO : 파일에서 Scene 정보 로드

    _activeScene = LoadTestScene();

    _activeScene->Awake();
    _activeScene->Start();
}

shared_ptr<Scene> SceneManager::LoadTestScene()
{
    // scene 
    shared_ptr<Scene> scene = make_shared<Scene>();

    // TestObject
    shared_ptr<GameObject> gameObject = make_shared<GameObject>();

	// VBV + IBV로 사각형 그려보기 코드
	vector<Vertex> vec(4);
	vec[0].pos = Vec3(-0.5f, 0.5f, 0.5f);
	vec[0].color = Vec4(1.f, 0.f, 0.f, 1.f);
	vec[0].uv = Vec2(0.f, 0.f);

	vec[1].pos = Vec3(0.5f, 0.5f, 0.5f);
	vec[1].color = Vec4(0.f, 1.f, 0.f, 1.f);
	vec[1].uv = Vec2(1.f, 0.f);

	vec[2].pos = Vec3(0.5f, -0.5f, 0.5f);
	vec[2].color = Vec4(0.f, 0.f, 1.f, 1.f);
	vec[2].uv = Vec2(1.f, 1.f);

	vec[3].pos = Vec3(-0.5f, -0.5f, 0.5f);
	vec[3].color = Vec4(0.f, 1.f, 0.f, 1.f);
	vec[3].uv = Vec2(0.f, 1.f);

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

	// 게임 오브젝트 방식 테스트 코드 추가
	gameObject->Init();

	shared_ptr<MeshRenderer> meshRenderer = make_shared<MeshRenderer>();
	{
		shared_ptr<Mesh> mesh = make_shared<Mesh>();
		mesh->Init(vec, indexVec);
		meshRenderer->SetMesh(mesh);
	}

	//
	{
		shared_ptr<Shader> shader = make_shared<Shader>();
		shared_ptr<Texture> texture = make_shared<Texture>();
		shader->Init(L"..\\Resources\\Shader\\default.hlsli");
		texture->Init(L"..\\Resources\\Texture\\defaultTex.jpg");

		shared_ptr<Material> material = make_shared<Material>();
		material->SetShader(shader);
		material->SetFloat(0, 0.3f);
		material->SetFloat(1, 0.4f);
		material->SetFloat(2, 0.3f);
		material->SetTexture(0, texture);

		meshRenderer->SetMaterial(material);
		//mesh->SetMaterial(material);
	}

	// 게임오브젝트에 만들어준 컴포넌트를 추가해준다
	gameObject->AddComponent(meshRenderer);

	scene->AddGameObject(gameObject);

	return scene;
}
