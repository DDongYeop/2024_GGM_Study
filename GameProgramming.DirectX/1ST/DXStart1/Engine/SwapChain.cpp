#include "pch.h"
#include "SwapChain.h"

void SwapChain::Init(const WindowInfo& info, ComPtr<IDXGIFactory> dxgi, ComPtr<ID3D12CommandQueue> cmdQueue)
{
    // 이전에 만든 정보는 날린다 
    _swapChain.Reset();

    DXGI_SWAP_CHAIN_DESC sd;
    sd.BufferDesc.Width = static_cast<uint32>(info.width);      //버퍼 해상도 너비
    sd.BufferDesc.Height = static_cast<uint32>(info.height);    //버퍼 해상도 높이
    sd.BufferDesc.RefreshRate.Numerator = 60;   //화면 갱신 비율
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;  //버퍼의 디스플레이 방식 
    sd.BufferDesc.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
    sd.BufferDesc.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;
    sd.SampleDesc.Count = 1;    //멀티샘플링 
    sd.SampleDesc.Quality = 0;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;   //후면 버퍼에 랜더링 하겠다 
    sd.BufferCount = SWAP_CHAIN_BUFFER_COUNT;   //전면 + 후면 버퍼
    sd.OutputWindow = info.hwnd;
    sd.Windowed = info.windowed;
    sd.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    sd.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;

    dxgi->CreateSwapChain(cmdQueue.Get(), &sd, &_swapChain);

    for (int32 i = 0; i < SWAP_CHAIN_BUFFER_COUNT; i++)
        _swapChain->GetBuffer(i, IID_PPV_ARGS(&_renderTargets[i]));
}

void SwapChain::Present()
{
    //현재 플레임
    //현재 화면을 그려줘
    //렌더링된 이미지를 사용자에게 표시합니다
    _swapChain->Present(0, 0);
}

void SwapChain::SwapIndex()
{
    _backBufferIndex = (_backBufferIndex + 1) % SWAP_CHAIN_BUFFER_COUNT;
}