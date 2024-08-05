#pragma once

#include "Device.h"
#include "CommandQueue.h"
#include "SwapChain.h"
#include "DescriptorHeap.h"
#include "RootSignature.h"
#include "Mesh.h"
#include "Shader.h"
#include "ConstantBuffer.h"
#include "TableDescriptorHeap.h"
#include "Texture.h"
#include "DepthStencilBuffer.h"


#include "Input.h"
#include "Timer.h"

class Engine
{
public:
	void Init(const WindowInfo& wInfo);
	void Render();

	void RenderBegin();
	void RenderEnd();

	void ResizeWindow(int32 width, int32 height);

public:
	void Update();


private:
	void ShowFPS();
	void CreateConstantBuffer(CBV_REGISTER reg, uint32 bufferSize, uint32 count);

public:
	shared_ptr<Device> GetDevice() { return _device; }
	shared_ptr<CommandQueue> GetCmdQueue() { return _cmdQueue; }
	shared_ptr<SwapChain> GetSwapChain() { return _swapChain; }
	shared_ptr<DescriptorHeap> GetDescHeap() { return _descHeap; }
	shared_ptr<RootSignature> GetRootSignature() { return _rootSignature; }
	//shared_ptr<ConstantBuffer> GetCB() { return _cb; }
	shared_ptr<TableDescriptorHeap> GetTableDescHeap() { return _tableDescHeap; }
	shared_ptr<DepthStencilBuffer> GetDepthStencilBuffer() { return _depthStencilBuffer; }


	shared_ptr<Input> GetInput() { return _input; }
	shared_ptr<Timer> GetTimer() { return _timer; }

	shared_ptr<ConstantBuffer> GetConstantBuffer(CONSTANT_BUFFER_TYPE type) { return _constantBuffers[static_cast<uint8>(type)]; }

private:
	// 그려질 화면 크기 관련
	WindowInfo		_window;
	D3D12_VIEWPORT	_viewport = {};
	D3D12_RECT		_scissorRect = {};

	shared_ptr<Device> _device = make_shared<Device>();
	shared_ptr<CommandQueue> _cmdQueue = make_shared<CommandQueue>();
	shared_ptr<SwapChain> _swapChain = make_shared<SwapChain>();
	shared_ptr<DescriptorHeap> _descHeap = make_shared<DescriptorHeap>();
	shared_ptr<RootSignature> _rootSignature = make_shared<RootSignature>();
	//shared_ptr<ConstantBuffer> _cb = make_shared<ConstantBuffer>();
	shared_ptr<TableDescriptorHeap> _tableDescHeap = make_shared<TableDescriptorHeap>();
	shared_ptr<DepthStencilBuffer> _depthStencilBuffer = make_shared<DepthStencilBuffer>();

	shared_ptr<Input> _input = make_shared<Input>();
	shared_ptr<Timer> _timer = make_shared<Timer>();

	vector<shared_ptr<ConstantBuffer>> _constantBuffers;

};

