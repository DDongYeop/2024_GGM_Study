#pragma once

class SwapChain;
class DescriptorHeap;

class CommandQueue
{
public:
    ~CommandQueue();

    void Init(ComPtr<ID3D12Device> device, shared_ptr<SwapChain> swapChain, shared_ptr<DescriptorHeap> descHeap);
    void WaitSync();

private:
    // DX12 이전까진 없던 개념 (DX12에서 처음 등장!)
    // 외주를 요청할때 하나씩 요청하면 비효율적
    // "외주 목록"에 일감을 하나하나 기록했다가 한번에 요청한다

    ComPtr<ID3D12CommandQueue>          _cmdQueue;
    ComPtr<ID3D12CommandAllocator>      _cmdAlloc;
    ComPtr<ID3D12GraphicsCommandList>   _cmdList;
    
    // Fence : 울타리 
    // CPU / GPU 사이에 동기화를 위한 간단한 도구 
    ComPtr<ID3D12Fence> _fence;
    uint32              _fenceValue = 0;
    HANDLE              _fenceEvent = INVALID_HANDLE_VALUE;

    shared_ptr<SwapChain>       _swapChain;
    shared_ptr<DescriptorHeap>  _descHeap;
};

