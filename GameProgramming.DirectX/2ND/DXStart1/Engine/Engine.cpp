#include "pch.h"
#include "Engine.h"
#include "Material.h"
#include "Transform.h"

#include "SceneManager.h"

void Engine::Init(const WindowInfo& wInfo)
{
	_window = wInfo;

	// 그려질 화면 크기를 설정
	_viewport = { 0, 0, static_cast<FLOAT>(wInfo.width), static_cast<FLOAT>(wInfo.height), 0.0f, 1.0f };
	_scissorRect = CD3DX12_RECT(0, 0, wInfo.width, wInfo.height);

	// 초기화 함수 호출
	_device->Init();
	_cmdQueue->Init(_device->GetDevice(), _swapChain, _descHeap);
	_swapChain->Init(wInfo, _device->GetDXGI(), _cmdQueue->GetCmdQueue());
	_descHeap->Init(_device->GetDevice(), _swapChain);
	_rootSignature->Init();
	_tableDescHeap->Init(256);

	// 인풋 & 타이머
	_input->Init(wInfo.hwnd);
	_timer->Init();

	CreateConstantBuffer(CBV_REGISTER::b0, sizeof(TransformParams), 256);
	CreateConstantBuffer(CBV_REGISTER::b1, sizeof(MaterialParams), 256);

	ResizeWindow(wInfo.width, wInfo.height);
}

void Engine::Render()
{
	RenderBegin();

	// TODO : 나머지 물체들을 그려준다
	GET_SINGLE(SceneManager)->Update();

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

	_depthStencilBuffer->Init(_window);
}

void Engine::Update()
{
	_input->Update();
	_timer->Update();

	Render();

	ShowFPS();
}

void Engine::LateUpdate()
{

}

void Engine::ShowFPS()
{
	uint32 fps = _timer->GetFps();

	WCHAR text[100] = L"";	
	::wsprintf(text, L"FPS : %d", fps);

	::SetWindowText(_window.hwnd,  text);
}

void Engine::CreateConstantBuffer(CBV_REGISTER reg, uint32 bufferSize, uint32 count)
{
	uint8 typeInt = static_cast<uint8>(reg);
	assert(_constantBuffers.size() == typeInt);

	shared_ptr<ConstantBuffer> buffer = make_shared<ConstantBuffer>();
	buffer->Init(reg, bufferSize, count);
	_constantBuffers.push_back(buffer);
}
