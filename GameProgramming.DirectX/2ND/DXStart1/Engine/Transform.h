#pragma once

#include "Component.h"

struct TransformMatrix
{
	Vec4 offset;
};


class Transform : public Component
{
public:
	Transform();
	virtual ~Transform();

	// TODO : 부모와 자식 관계에 대한 변수들과 메서드

private:
	// TODO : World 위치 관련된 변수와 메서들

};

/*
 * 행렬(Matrix) (row X coloum) (2차원 배열로 표현 가능)
 * 
 * 3D 공간상의 특정 물체는 백터로 위치를 표현할 수 있다. 
 * 그 백터를 (변환) 시킬 때 "행렬"을 사용하여 유용하게 사용 가능
 * 
 * 행렬은 2차원 배열인데 이 안에는 스칼라 값이 들어가 있는 형태이다. 
 * 
 * 백터에게 곱연산을 적용시켜서 (행렬을 곱해서) 새로운 결과물을 얻기 위함이다. 
 * * Scale, Rotate, Translation(SRT)
 * 
 * - 게임에서의 행렬은 "곱셈"이 중요하다. (V x M)
 * - 벡터(v)는 오브젝트이며 행렬은(m)은 포탈 개념이다.
 * 
 * 결합법칙 O , 교환법칙 X   VM =/ MV
 * 
 * 단위행렬(Identify) : 대각선의 값이 모두 1이고 나머지는 0
 * : 단위행렬과 곱하면 자기자신이 나온다.
 * 
 * 직교행렬 (대각선이 0인 행렬)
 * 전치행렬(Transpose) (대각선을 기준으로 뒤집은 행렬)
 * 직교행렬(벡터끼리 내적을 했을 때 0이 나오는 경우)
 * 
 * 게임에서의 행렬은 주로(4X4)행렬을 사용한다. 
 * 벡터를 행렬로 표시한다면(1X4)로 표시 할 수 있다.
 * 
 * 
 * 
 * 
 * 
 * 행렬의 변환
 * 
 * - 동차 좌표계 (Homogeneous coodinates) (points vs vectors)
 *  순서 중요 : S -> R -> T
 * 
 * 
 * 
 * 좌표계 변환 (WVP)
 * 
 * Local (3D 모델링된 물체) -> World -> View(카메라)
 * -> Projection(원근투영p or 직교투영o) -> Clip Space(정해진 사이즈의 박스)
 * -> Screen(뷰포트)
 * 
 * 
 * 
 */