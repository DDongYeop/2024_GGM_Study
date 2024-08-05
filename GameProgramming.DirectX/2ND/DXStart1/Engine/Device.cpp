#include "pch.h"
#include "Device.h"

void Device::Init()
{
	// DXGI란?
	// DXGI(DirectX Graphics Infrastructure)
	// 전체화면 모드 전환
	// 지원되는 디스플레이 모드 열거 등
	::CreateDXGIFactory(IID_PPV_ARGS(&_dxgi));


	// CreateDevice
	// 디스플레이 어댑터(그래픽 카드)를 나타내는 객체
	// padapter : null로 지정하면 시스템 기본 디스플레이 어댑터
	// 미니멈레벨 : 응용프로그램이 요구하는 최소 기능 수준(오래된 그래픽 카드 걸러낼 수 있다)
	::D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_11_0, IID_PPV_ARGS(&_device));
}
