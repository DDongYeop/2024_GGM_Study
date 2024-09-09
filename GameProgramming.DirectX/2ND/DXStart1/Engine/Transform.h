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
    // Parent ����
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
    // Parent ����
    Vec3 _localPosition = {};
    Vec3 _localRotation = {};
    Vec3 _localScale = { 1.f, 1.f, 1.f };

    Matrix _matLocal = {};
    Matrix _matWorld = {};

    weak_ptr<Transform> _parent;

};


/*
* 
* ���(Matrix) (row x column) (2���� �迭�� ǥ�� ����)
* 
*  3D �������� Ư�� ��ü�� ���ͷ� ��ġ�� ǥ���� �� �ִ�
* �� ���͸� (��ȯ) ��ų �� "���"�� ����Ͽ� �����ϰ� Ȱ�밡��
* 
* ����� 2���� �迭�ε� �� �ȿ��� ��Į���� �� �ִ� �����̴�
* 
* ���Ϳ��� �������� ������Ѽ�(����� ���ؼ�) ���ο� ������� ��� �����̴�
* * SCALE, ROTATE, TRANSLATION(SRT)
* 
* - ���ӿ����� ����� "����"�� �߿��ϴ�(V x M)
* - ����(v)�� ������Ʈ�̸� ���(m)�� ��Ż�����̴�

* ���չ�Ģ o, ��ȯ��Ģ x  VM =/ MV
* 
* �������(Identify) :  �밢���� ���� ���1�̰� �������� 0
* : ������İ� ���ϸ� �ڱ��ڽ��� ���´�
* 
* �밢���(�밢���� 0�� ���)
* ��ġ���(Transpose)(�밢���� �������� ������ ���)
* �������(���ͳ��� ������ ���� �� 0�� ������ ���)
* 
* ���ӿ����� ����� �ַ�(4x4)����� ����Ѵ�
* ���͸� ��ķ� ǥ���Ѵٸ�(1x4)�� ǥ�� �� �� �ִ�
* 
* 
* 
* 
* 
* 
* 
* 
* 
* ����� ��ȯ
* 
* 
* - ������ǥ��(Homogeneous coodinates)(points vs vectors)
* 
*  ���� �߿� : S->R->T
* 
* 
* 
* 
* ��ǥ�� ��ȯ(WVP)
* 
* Local(3D �𵨸��� ��ü) -> World -> View(ī�޶�) 
-> Projection(��������p or ��������o) -> Clip Space(������ �������� �ڽ�)
-> Screen(����Ʈ)
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