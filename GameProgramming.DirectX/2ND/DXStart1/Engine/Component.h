#pragma once

enum class COMPONENT_TYPE : uint8
{
    TRANSFORM,
    MESH_RENDERER,
    // ...
    MONO_BEHAVIOUR,
    END,
};

enum
{
    FIXED_COMPONENT_COUNT = static_cast<uint8>(COMPONENT_TYPE::END) - 1
};

class GameObject;
class Transform;

class Component
{
public:
    Component(COMPONENT_TYPE type);
    virtual ~Component();   // virtual을 붙힘으로써 메모리 누수를 방지.

public:
    virtual void Awake() { }
    virtual void Start() { }
    virtual void Update() { }
    virtual void LateUpdate() { }

public:
    COMPONENT_TYPE GetType() { return _type; }
    bool IsValid() { return _gameObject.expired() == false; }

    shared_ptr<GameObject> GetGameObject();
    shared_ptr<Transform> GetTransform();

private:
    friend class GameObject;
    void SetGameObject(shared_ptr<GameObject> gameObject) { _gameObject = gameObject; }
    // SetGameObject 함수는 GameObject만 실행할 수 있도록 하기 위해서 사용함

protected:
    COMPONENT_TYPE _type;
    weak_ptr<GameObject> _gameObject;
    // weak_ptr로 만든 이유
    //  GameObject에서도 컴포넌트를 물고 있을 때 컴포넌트 입장에서도 누구에게 포함이 되는지
    //  서로 포인터 개념으로 가리키는 형태로 알고 있어야만 한다.
    // 
    //  양쪽을 shared_ptr로 만들게 되면 순환구조로 만들어지기 때문에 레퍼런스 카운트가 영구적으로 줄지 않는 문제가 생기기에 weak_ptr로 만들어야함.
}; 
