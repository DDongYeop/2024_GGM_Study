#pragma once
class Texture
{
	// CBV같은 애들과 다른점은 디스크립터힙을 만들고 내부에 여러개의 뷰를 만들었던 반면
	// 텍스처는 한번만 로딩을 하고 뷰를 하나만 만들고 사용해도 된다
	
public:
	void Init(const wstring& path);

	D3D12_CPU_DESCRIPTOR_HANDLE GetCpuHandle() { return _srvHandle; }

public:
	void CreateTexture(const wstring& path);
	void CreateView();

private:
	ScratchImage			 		_image;
	ComPtr<ID3D12Resource>			_tex2D;

	ComPtr<ID3D12DescriptorHeap>	_srvHeap;
	D3D12_CPU_DESCRIPTOR_HANDLE		_srvHandle;
};

