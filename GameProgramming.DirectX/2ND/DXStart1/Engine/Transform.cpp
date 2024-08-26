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
    // SRT를 계산 해준다. 
    // 회전 값 같은 경우는 원래 쿼터니언 써야하는데
    // 일단 오일러 공식 사용
    Matrix matScale = Matrix::CreateScale(_localScale);
    Matrix matRotation = Matrix::CreateRotationX(_localRotation.x);
    matRotation *= Matrix::CreateRotationY(_localRotation.y);
    matRotation *= Matrix::CreateRotationZ(_localRotation.z);
    Matrix matTranslation = Matrix::CreateTranslation(_localPosition);

    // 월드 행렬을 계산
    // 부모가 없는 상태라면 이 자체가 월드행렬이 됨.
    // _matLocal는 필요할 수도 있으므로 일단 저장 용도이다.
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