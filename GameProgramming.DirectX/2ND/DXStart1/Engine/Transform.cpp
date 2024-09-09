#include "pch.h"
#include "Transform.h"
#include "Engine.h"
#include "Camera.h"


Transform::Transform() : Component(COMPONENT_TYPE::TRANSFORM)
{

}

Transform::~Transform()
{

}

void Transform::FinalUpdate()
{

    // SRT를 계산해준다
    // 회전값 같은 경우는 원래는 쿼터니언을 사용해서 해야하는데
    // 일단은 그냥 이렇게 오일러 공식을 적용해서 해준다
    Matrix matScale = Matrix::CreateScale(_localScale);
    Matrix matRotation = Matrix::CreateRotationX(_localRotation.x);
    matRotation *= Matrix::CreateRotationY(_localRotation.y);
    matRotation *= Matrix::CreateRotationZ(_localRotation.z);
    Matrix matTranslation = Matrix::CreateTranslation(_localPosition);

    // 월드행렬을 계산해준다
    // 부모가 없는 상태라면 이 자체가 월드행렬이 된다
    // _matLocal는 필요할 수도 있으므로 일단 저장 용도이다
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
    // WVP
    Matrix matWVP = _matWorld * Camera::S_MatView * Camera::S_MatProjection;
    CONST_BUFFER(CONSTANT_BUFFER_TYPE::TRANSFORM)->PushData(&matWVP, sizeof(matWVP));
}