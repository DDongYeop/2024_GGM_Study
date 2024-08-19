#include "pch.h"
#include "GameObject.h"

#include "Transform.h"
#include "MeshRenderer.h"
#include "MonoBehaviour.h"

GameObject::GameObject()
{

}

GameObject::~GameObject()
{

}

void GameObject::Init()
{
    AddComponent(make_shared<Transform>());
    // 게임오브젝트 생성시 기본적으로 Transform하나는 무조건 추가
}


void GameObject::Awake()
{
    for (shared_ptr<Component>& component : _components)
    {
        if (component)
            component->Awake();
    }

    for (shared_ptr<MonoBehaviour>& script : _scripts)
    {
        script->Awake();
    }
}

void GameObject::Start()
{
    for (shared_ptr<Component>& component : _components)
    {
        if (component)
            component->Start();
    }

    for (shared_ptr<MonoBehaviour>& script : _scripts)
    {
        script->Start();
    }
}

void GameObject::Update()
{
    for (shared_ptr<Component>& component : _components)
    {
        if (component)
            component->Update();
    }

    for (shared_ptr<MonoBehaviour>& script : _scripts)
    {
        script->Update();
    }
}

void GameObject::LateUpdate()
{
    for (shared_ptr<Component>& component : _components)
    {
        if (component)
            component->LateUpdate();
    }

    for (shared_ptr<MonoBehaviour>& script : _scripts)
    {
        script->LateUpdate();
    }
}

shared_ptr<Transform> GameObject::GetTransform()
{
    uint8 index = static_cast<uint8>(COMPONENT_TYPE::TRANSFORM);
    return static_pointer_cast<Transform>(_components[index]);
}

void GameObject::AddComponent(shared_ptr<Component> component)
{
    component->SetGameObject(shared_from_this());
    // 컴포넌트야 너의 부모님은 나(GameObject)야 라고 넘겨주고 싶다
    // ex1) component->SetGameObject(this);
    // : 여기서 this는 스마트 포인터가 아니라 GameObject의 포인터임
    // ex2) component->SetGameObject(make_shared<GameObject>(this));
    // : 새로운 스마트 포인터의 레퍼런스 카운터가 생성됨(메모리 누수의 문제점)


    /*
    * enable_shared_from_this는 자기자신을 weke_ptr로 가지고 있는 녀석이다
    */

    uint8 index = static_cast<uint8>(component->GetType());
    if (index < FIXED_COMPONENT_COUNT)
    {
        _components[index] = component;
    }
    else
    {
        _scripts.push_back(dynamic_pointer_cast<MonoBehaviour>(component));
        // dynamic_pointer_cast : 스마트 포인터끼리 작업을 할 때 사용하는 랩핑 함수이다
    }
}