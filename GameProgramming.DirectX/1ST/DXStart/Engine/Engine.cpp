#include "pch.h"
#include "Engine.h"

void Engine::Init(const WindowInfo& wInfo)
{
	_window = wInfo;
	ResizeWindow(wInfo.width, wInfo.height);

	// 그려질 화면 크기를 설정
	_viewport = { 0, 0, static_cast<FLOAT>(wInfo.width), static_cast<FLOAT>(wInfo.height), 0.0f, 1.0f };
	_scissorRect = CD3DX12_RECT(0, 0, wInfo.width, wInfo.height);

	// 각종 장치초기화 변수들의 메모리를 할당한다
	_device = make_shared<Device>();
	_cmdQueue = make_shared<CommandQueue>();
	_swapChain = make_shared<SwapChain>();
	_descHeap = make_shared<DescriptorHeap>();
	_rootSignature = make_shared<RootSignature>();
	_cb = make_shared<ConstantBuffer>();
	_tableDescHeap = make_shared<TableDescriptorHeap>();

	// 초기화 함수 호출
	_device->Init();
	_cmdQueue->Init(_device->GetDevice(), _swapChain, _descHeap);
	_swapChain->Init(wInfo, _device->GetDXGI(), _cmdQueue->GetCmdQueue());
	_descHeap->Init(_device->GetDevice(), _swapChain);
	_rootSignature->Init();
	_cb->Init(sizeof(Transform), 256);
	_tableDescHeap->Init(256);
}

void Engine::Render()
{
	RenderBegin();

	// 나머지 물체들을 그려준다

	RenderEnd();
}

void Engine::RenderBegin()
{
	_cmdQueue->RenderBegin(&_viewport, &_scissorRect);
}

void Engine::RenderEnd()
{
	_cmdQueue->RenderEnd();
}

void Engine::ResizeWindow(int32 width, int32 height)
{
	_window.width = width;
	_window.height = height;

	RECT rect = { 0, 0, width, height };
	::AdjustWindowRect(&rect, WS_OVERLAPPEDWINDOW, false);
	::SetWindowPos(_window.hwnd, 0, 100, 100, width, height, 0);
}
