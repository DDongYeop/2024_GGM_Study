#pragma once

// 장치(하드웨어)와 직접 소통하기 위한 함수들의 모음

class Device
{	
public:
	void Init();

	ComPtr<IDXGIFactory> GetDXGI() { return _dxgi; }
	ComPtr<ID3D12Device> GetDevice() { return _device; }

private:

	// COM이란 무엇인가? 컴포넌트 오브젝트 모델
	// DX 프로그래밍 언어의 독립성과 하위 호환성을 가능하게 하는 기술
	// COM객체(=COM인터페이스)를 사용하여 세부사항들을 우리에게 숨겨짐
	// ComPtr = 일종의 스마트 포인터로 접근하여 사용 가능하다

	ComPtr<ID3D12Debug>			_debugController;
	ComPtr<IDXGIFactory>		_dxgi; // 화면 관련 기능들
	ComPtr<ID3D12Device>		_device; // 각종 객체 생성
};

