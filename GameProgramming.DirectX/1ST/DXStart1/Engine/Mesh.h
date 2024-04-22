#pragma once
class Mesh
{
public:
    void Init(vector<Vertex>& vec);
    void Render();

private:
    ComPtr<ID3D12Resource>        _vertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW    _vertexBufferView = {};
    uint32 _vertexCount = 0;
};



/*
* 1.Vertex란?
* 하나의 점이다.
* 정점이라고 한다.
* 3D의 가장 기본 단위이다.
* 2D에만 있는 포인트
* 정점은 위치, 색상, 법선 등
* 
* 2.Polygon이란?
* 최소의 면 단위
* 선과 선들이 모여서 면이 된다 -> 삼각형
* 버텍스 3개가 모이면 하나의 면 단위를 이루는 폴리곤
* 
* 3.Mesh란?
* 
* 폴리곤들이 삼차원상의 공간에 모여서 나온 물체덩어리(오브젝트)
* 
*/