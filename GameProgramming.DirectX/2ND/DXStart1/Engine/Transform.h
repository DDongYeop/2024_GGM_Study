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

