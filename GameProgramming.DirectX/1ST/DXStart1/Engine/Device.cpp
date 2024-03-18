#include "pch.h"
#include "Device.h"

void Device::Init()
{
	// DXGI란?
	// DXGI (DirectX Graphics Infrastructure)
	// 전체 화면 모드 전환
	// 지원되는 디스플레이 모드 열거 등 
	::CreateDXGIFactory(IID_PPV_ARGS(&_dxgi));

	// CreateDevice
	// 디스플레이 어댑터 (그래픽 카드)를 나타내는 객체 
	// padpter : null로 저장하면 시스템 기본 디스플레이 어댑터
	// 미너멈레벨 : 응용 프로그램이 요구하는 최소 기능 수준 (오랜된 그래픽카드 걸러내기 가능)
	::D3D12CreateDevice(nullptr, D3D_FEATURE_LEVEL_11_0, IID_PPV_ARGS(&_device));
}
