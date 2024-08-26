#pragma once

class GameObject;

class Scene
{
public:
    void Awake();
    void Start();
    void Update();
    void LateUpdate();
    void FinalUpdate();

    void AddGameObject(shared_ptr<GameObject> gameObject);
    void RemoveGameObject(shared_ptr<GameObject> gameObject);

private:
    vector<shared_ptr<GameObject>> _gameObjects;
    // 추가적으로는 유니티에서 존재하는 레이어 개념이 존재함(32)
    // 레이어에 따라서 구분되어진 게임오브젝트들의 리스트들을 따로 각각 관리해야만 한다

};

