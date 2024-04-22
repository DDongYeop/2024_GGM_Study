#include "pch.h"
#include "ConstantBuffer.h"
#include "Engine.h"

ConstantBuffer::ConstantBuffer()
{
}

ConstantBuffer::~ConstantBuffer()
{
    if (_cbvBuffer)
    {
        if (_cbvBuffer != nullptr)
            _cbvBuffer->Unmap(0, nullptr);

        _cbvBuffer = nullptr;
    }
}

void ConstantBuffer::Init(uint32 size, uint32 count)
{
    // 상수 버퍼는 256 바이트 배수로 만들어야 한다
    // 0 256 512 768
    _elementSize = (size + 255) & ~255;
    _elementCount = count;

    CreateBuffer();
}

void ConstantBuffer::CreateBuffer()
{
    uint32 bufferSize = _elementSize * _elementCount;
    D3D12_HEAP_PROPERTIES heapProperty = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD);
    D3D12_RESOURCE_DESC desc = CD3DX12_RESOURCE_DESC::Buffer(bufferSize);

    DEVICE->CreateCommittedResource(
        &heapProperty,
        D3D12_HEAP_FLAG_NONE,
        &desc,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nullptr,
        IID_PPV_ARGS(&_cbvBuffer));

    _cbvBuffer->Map(0, nullptr, reinterpret_cast<void**>(&_mappedBuffer));
    //리소스 작업을 완료할 때까지 매핑을 해제할 필요가 없다
    //하지만 GPU가 리소스를 사용하는 동안에는 리소스를 사용해서는 안된다(따라서 동기화가 필요하다)
}

void ConstantBuffer::Clear()
{
    _currentIndex = 0;
}

void ConstantBuffer::PushData(int32 rootParamIndex, void* buffer, uint32 size)
{
    // 조건이 만족하지 않으면 크래시를 내는 디버깅 코드
    assert(_currentIndex < _elementSize);


    ::memcpy(&_mappedBuffer[_currentIndex * _elementSize], buffer, size);

    D3D12_GPU_VIRTUAL_ADDRESS address = GetGpuVirtualAddress(_currentIndex);
    CMD_LIST->SetGraphicsRootConstantBufferView(rootParamIndex, address);
    _currentIndex++;
}

D3D12_GPU_VIRTUAL_ADDRESS ConstantBuffer::GetGpuVirtualAddress(uint32 index)
{
    D3D12_GPU_VIRTUAL_ADDRESS objCBAddress = _cbvBuffer->GetGPUVirtualAddress();
    objCBAddress += index * _elementSize;
    return objCBAddress;
}