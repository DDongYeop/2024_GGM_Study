#pragma once

class Texture;

class Mesh
{
public:
	void Init(const vector<Vertex>& vertexBuffer, const vector<uint32>& indexBuffer);
	void Render();

	void SetTransform(const Transform& t) { _transform = t; }
	void SetTexture(shared_ptr<Texture> tex) { _tex = tex; }

private:
	void CreateVertexBuffer(const vector<Vertex>& buffer);
	void CreateIndexBuffer(const vector<uint32>& buffer);

private:
	// VBV 관련
	ComPtr<ID3D12Resource>		_vertexBuffer;
	D3D12_VERTEX_BUFFER_VIEW	_vertexBufferView = {};
	uint32 _vertexCount = 0;

	// IBV 관련
	ComPtr<ID3D12Resource>		_indexBuffer;
	D3D12_INDEX_BUFFER_VIEW		_indexBufferView;
	uint32 _indexCount = 0;

	Transform _transform = {};
	shared_ptr<Texture> _tex = {};
};



/*
* 1. 버텍스
* 하나의 점이다
* 정점이라고도 한다
* 3D의 가장 기본 단위이다
* 2D에만 있는 포인트
* 정점은 위치, 색상, 법선 등 다양한 정보를 담고 있다
* 
* 2. 폴리곤이란?
*  최소의 면 단위
* 선과선들이 모여서 면이된다 -> 삼각형
* 버텍스 3개가 모이면 하나의 면 단위를 이루는 폴리곤
* 
* 3. 메쉬란?
* 
* 폴리곤들이 삼차원상의 공간에 모여서 나온 물체덩어리(오브젝트)
* 
*/