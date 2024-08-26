#include "pch.h"
#include "Transform.h"
#include "Engine.h"

Transform::Transform() : Component(COMPONENT_TYPE::TRANSFORM)
{

}

Transform::~Transform()
{

}

void Transform::FinalUpdate()
{
    // SRT�� ��� ���ش�. 
    // ȸ�� �� ���� ���� ���� ���ʹϾ� ����ϴµ�
    // �ϴ� ���Ϸ� ���� ���
    Matrix matScale = Matrix::CreateScale(_localScale);
    Matrix matRotation = Matrix::CreateRotationX(_localRotation.x);
    matRotation *= Matrix::CreateRotationY(_localRotation.y);
    matRotation *= Matrix::CreateRotationZ(_localRotation.z);
    Matrix matTranslation = Matrix::CreateTranslation(_localPosition);

    // ���� ����� ���
    // �θ� ���� ���¶�� �� ��ü�� ��������� ��.
    // _matLocal�� �ʿ��� ���� �����Ƿ� �ϴ� ���� �뵵�̴�.
    _matLocal = matScale * matRotation * matTranslation;
    _matWorld = _matLocal;

    shared_ptr<Transform> parent = GetParent().lock();
    if (parent != nullptr)
    {
        _matWorld *= parent->GetLocalToWorldMatrix();
    }
}

void Transform::PushData()
{
    // W[VP]
    // TODO :

    CONST_BUFFER(CONSTANT_BUFFER_TYPE::TRANSFORM)->PushData(&_matWorld, sizeof(_matWorld));
}

// CONST_BUFFER(CONSTANT_BUFFER_TYPE::TRANSFORM)->PushData(&_transform, sizeof(_transform));