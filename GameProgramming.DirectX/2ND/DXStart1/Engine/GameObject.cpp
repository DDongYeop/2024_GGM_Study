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
    // ���ӿ�����Ʈ ������ �⺻������ Transform�ϳ��� ������ �߰�
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
    // ������Ʈ�� ���� �θ���� ��(GameObject)�� ��� �Ѱ��ְ� �ʹ�
    // ex1) component->SetGameObject(this);
    // : ���⼭ this�� ����Ʈ �����Ͱ� �ƴ϶� GameObject�� ��������
    // ex2) component->SetGameObject(make_shared<GameObject>(this));
    // : ���ο� ����Ʈ �������� ���۷��� ī���Ͱ� ������(�޸� ������ ������)


    /*
    * enable_shared_from_this�� �ڱ��ڽ��� weke_ptr�� ������ �ִ� �༮�̴�
    */

    uint8 index = static_cast<uint8>(component->GetType());
    if (index < FIXED_COMPONENT_COUNT)
    {
        _components[index] = component;
    }
    else
    {
        _scripts.push_back(dynamic_pointer_cast<MonoBehaviour>(component));
        // dynamic_pointer_cast : ����Ʈ �����ͳ��� �۾��� �� �� ����ϴ� ���� �Լ��̴�
    }
}