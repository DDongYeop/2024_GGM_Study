#pragma once

#include "Component.h"

class Transform : public Component
{
public:
	Transform();
	virtual ~Transform();

    virtual void FinalUpdate() override;
    void PushData();

public:
    // Parent 기준
    const Vec3& GetLocalPosition() { return _localPosition; }
    const Vec3& GetLocalRotation() { return _localRotation; }
    const Vec3& GetLocalScale() { return _localScale; }

    const Matrix& GetLocalToWorldMatrix() { return _matWorld; }
    const Vec3& GetWorldPosition() { return _matWorld.Translation(); }

    Vec3 GetRight() { return _matWorld.Right(); }
    Vec3 GetUp() { return _matWorld.Up(); }
    Vec3 GetLook() { return _matWorld.Backward(); }

    void SetLocalPosition(const Vec3& position) { _localPosition = position; }
    void SetLocalRotation(const Vec3& rotation) { _localRotation = rotation; }
    void SetLocalScale(const Vec3& scale) { _localScale = scale; }

public:
    void SetParent(shared_ptr<Transform> parent) { _parent = parent; }
    weak_ptr<Transform> GetParent() { return _parent; }


private:
    // Parent 기준
    Vec3 _localPosition = {};
    Vec3 _localRotation = {};
    Vec3 _localScale = { 1.f, 1.f, 1.f };

    Matrix _matLocal = {};
    Matrix _matWorld = {};

    weak_ptr<Transform> _parent;

};


/*
* 
* 행렬(Matrix) (row x column) (2차원 배열로 표현 가능)
* 
*  3D 공간상의 특정 물체는 벡터로 위치를 표현할 수 있다
* 그 벡터를 (변환) 시킬 때 "행렬"을 사용하여 유용하게 활용가능
* 
* 행렬은 2차원 배열인데 이 안에는 스칼라값이 들어가 있는 형태이다
* 
* 벡터에게 곱연산을 적용시켜서(행렬을 곱해서) 새로운 결과물을 얻기 위함이다
* * SCALE, ROTATE, TRANSLATION(SRT)
* 
* - 게임에서의 행렬은 "곱셈"이 중요하다(V x M)
* - 벡터(v)는 오브젝트이며 행렬(m)은 포탈개념이다

* 결합법칙 o, 교환법칙 x  VM =/ MV
* 
* 단위행렬(Identify) :  대각선의 값이 모두1이고 나머지는 0
* : 단위행렬과 곱하면 자기자신이 나온다
* 
* 대각행렬(대각선이 0인 행렬)
* 전치행렬(Transpose)(대각선을 기준으로 뒤집은 행렬)
* 직교행렬(벡터끼리 내적을 했을 때 0이 나오는 경우)
* 
* 게임에서의 행렬은 주로(4x4)행렬을 사용한다
* 벡터를 행렬로 표시한다면(1x4)로 표시 할 수 있다
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 행렬의 변환
* 
* 
* - 동차좌표계(Homogeneous coodinates)(points vs vectors)
* 
*  순서 중요 : S->R->T
* 
* 
* 
* 
* 좌표계 변환(WVP)
* 
* Local(3D 모델링된 물체) -> World -> View(카메라) 
-> Projection(원근투영p or 직교투영o) -> Clip Space(정해진 사이즈의 박스)
-> Screen(뷰포트)
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 
* 

*/