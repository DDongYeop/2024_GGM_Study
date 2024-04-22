#pragma once

/// <RootSignature>
/// 서명을 하는놈. GPU의 가장 메모리에 어떤 버퍼를 사용하겠다를 명시하는 기능을 담당
/// </RootSignature>
class RootSignature
{
public:
	void Init(ComPtr<ID3D12Device> device);

	ComPtr<ID3D12RootSignature>    GetSignature() { return _signature; }

private:
	ComPtr<ID3D12RootSignature>    _signature;

};

