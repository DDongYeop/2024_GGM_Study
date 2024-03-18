#include "pch.h"
#include "CommandQueue.h"

CommandQueue::~CommandQueue()
{
    ::CloseHandle(_fenceEvent);
}

void CommandQueue::Init(ComPtr<ID3D12Device> device, shared_ptr<SwapChain> swapChain, shared_ptr<DescriptorHeap> descHeap)
{
    _swapChain = swapChain;
    _descHeap = descHeap;

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;

    // 커맨드큐를 함수를 통해 생성한다
    device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&_cmdQueue));
    // GPU가 직접 실행하는 명령 목록
    device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&_cmdAlloc));

    // 커맨드 리스트는 상태값이 CLOSE / OPEN (열었다가 닫혔다가 반혹)
    // OPEN상태에서 COMAND를 넣었다가 Close한 다음에 제출한다는 개념 
    device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, _cmdAlloc.Get(), nullptr, IID_PPV_ARGS(&_cmdList));

    _cmdList->Close();

    // CPU와 GPU의 동기화 수단으로 쓰임 
    device->CreateFence(0, D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&_fence));

    // 멀티스레드에서 동기화 할 때 주로 사용하는 방법
    // 이벤트는 신호등과 같은 존재
    // 빨간불일땐 멈춰 있다가 파란불일때 켜질때까지 대기 -> 동기화 시키는 용도 
    _fenceEvent = ::CreateEvent(nullptr, FALSE, FALSE, nullptr);
}


void CommandQueue::WaitSync()
{
    // GPU에게 외주를 줄 때마다 인덱스 증가
    _fenceValue++;

    // 명령 대기열에 새로운 펜스포인트를 설정하는 명령을 추가
    // GPU 타임라인에 있으므로 CPU가 하는 일이 완료될때까지 새 울타리 지점이 설정되지 않는다 
    _cmdQueue->Signal(_fence.Get(), _fenceValue);

    // GPU가 이 펜스지점까지 명령을 완료할 때까지 기다린다
    if (_fence->GetCompletedValue() < _fenceValue)
    {
        // 현재 펜스에 도달하면 이벤트를 발생시킴
        // _fenceValue만큼의 일이 다 끝냈다면 이벤트를 발생 (파란불)
        _fence->SetEventOnCompletion(_fenceValue, _fenceEvent);

        // GPU가 현재 팬스에 도달할때까지 기다리세요. 곧 이벤트가 시작됨
        // 즉 CPU가 기다리고 있는중 
        ::WaitForSingleObject(_fenceEvent, INFINITE);
    }
}

