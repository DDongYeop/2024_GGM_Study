#pragma once

/*
* 교환 사슬
* [외주 과정]
* - GPU가 열심히 계산을 한다(외주일을 하고 있다)
* - 어떤 문서에 상세문서를 적고 어떤 공식으로 어떻게 계산할지 알려줌
* - 결과물을 받아서 화면에 그려준다
* 
* 
* [외주 결과물]을 어디에 받아 놓을까?
* - 어떤 종이(Buffer)에 그려서 건내달라고 부탁을 해보자
* - 특수한 종이를 만들어서 -> 건내주고 -> 결과물을 해당 종이에 받는다
* - 마지막에 우리 화면에 특수한 종이(외주 결과물) 출력해준다
* 
* 
* - 그런데 화면에 현재 결과물을 출력하는 도중에, 다음 화면도 외주를 맡겨야 하는 상황
* - 현재 화면 결과물은 이미 화면에 출력에 사용중...
* - 특수한 종이는 2장을 만들어서 0번은 현재화면을 그려주고 1번은 외주를 맡기고
* -----------> 더블 퍼버링
* 
* 
* [0]  [1] <-> GPU 작업중
*/



class SwapChain
{
public:
	void Init(const WindowInfo& info, ComPtr<IDXGIFactory> dxgi, ComPtr<ID3D12CommandQueue> cmdQueue);

	void Present();
	void SwapIndex();

	ComPtr<IDXGISwapChain> GetSwapChain() { return _swapChain; }
	ComPtr<ID3D12Resource> GetRenderTarget(int32 index) { return _renderTargets[index]; }

	uint32 GetCurrentBackBufferIndex() { return _backBufferIndex; }
	ComPtr<ID3D12Resource> GetCurrentBackBufferResource() { return _renderTargets[_backBufferIndex]; }


private:
	ComPtr<IDXGISwapChain>	_swapChain;
	ComPtr<ID3D12Resource>	_renderTargets[SWAP_CHAIN_BUFFER_COUNT];
	uint32					_backBufferIndex = 0;

};

