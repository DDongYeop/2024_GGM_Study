#include "pch.h"
#include "Mesh.h"
#include "Engine.h"

// 벡터에 버텍스 3개를 받아서 전달 (= 정점목록을 받는다)

void Mesh::Init(vector<Vertex>& vec)
{
    _vertexCount = static_cast<uint32>(vec.size());
    uint32 bufferSize = _vertexCount * sizeof(Vertex);

    D3D12_HEAP_PROPERTIES heapProperty = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD);
    D3D12_RESOURCE_DESC desc = CD3DX12_RESOURCE_DESC::Buffer(bufferSize);

    //_vertextBuffersms GPU의 공간을 가리키고 있다
    //GPU의 메모리에 정점 데이터를 복사해주는 과정

    //GPU쪽으로 밀어넣기 위한 리소스
    DEVICE->CreateCommittedResource(
        &heapProperty,
        D3D12_HEAP_FLAG_NONE,
        &desc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&_vertexBuffer));



    // Copy the triangle data to the vertex buffer.
    void* vertexDataBuffer = nullptr;
    CD3DX12_RANGE readRange(0, 0); // We do not intend to read from this resource on the CPU.
    _vertexBuffer->Map(0, &readRange, &vertexDataBuffer);
    ::memcpy(vertexDataBuffer, &vec[0], bufferSize);
    _vertexBuffer->Unmap(0, nullptr);

    // Initialize the vertex buffer view.
    _vertexBufferView.BufferLocation = _vertexBuffer->GetGPUVirtualAddress();
    _vertexBufferView.StrideInBytes = sizeof(Vertex);   //정점 1개의 크기
    _vertexBufferView.SizeInBytes = bufferSize;         //버퍼의 크기
}

void Mesh::Render()
{
    CMD_LIST->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    CMD_LIST->IASetVertexBuffers(0, 1, &_vertexBufferView); // Slot: (0~15)

    //루트 커스터 마이징 서명 

    CMD_LIST->DrawInstanced(_vertexCount, 1, 0, 0);
}
