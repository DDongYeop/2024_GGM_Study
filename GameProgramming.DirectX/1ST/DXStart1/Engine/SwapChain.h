#pragma once

/*
* swap chain
* ��ȯ �罽
* [���� ����]
* - GPU�� ������ ����� �Ѵ�(�������� �ϰ� �ִ�)
* - � ������ �󼼹����� ���� � �������� ��� ������� �˷���
* - ������� �޾� ȭ�鿡 �׷���
* 
* [���� �����]�� ��� �޾� ������?
* - � ����(Buffer)�� �׷��� �ǳ��޶�� ��Ź�� �غ���
* - Ư���� ���̸� ���� -> �ǳ��ְ� -> ������� �ش� ���̿� �޴´� 
* - �������� �츮 ȭ�鿡 Ư���� ����(���� �����)�� ������ش� 
* 
* - �׷��� ȭ�鿡 ���� ������� ����ϴ� ���߿�, ���� ȭ�鵵 ���ָ� �ðܾ� �ϴ� ��Ȳ
* - ���� ȭ�� ������� �̹� ȭ�鿡 ����� �����...
* - Ư���� ���̴� 2���� ���� 0���� ���� ȭ���� �׷��ְ�, 1���� ���ָ� �ñ��, 
* ------------------> ���� ���۸�
* 
* 
* [0] [1] <-> GPU �۾��� 
*/

class SwapChain
{
public:
    void Init(const WindowInfo& info, ComPtr<IDXGIFactory> dxgi, ComPtr<ID3D12CommandQueue> cmdQueue);
    void Present();
    void SwapIndex();

private:
    ComPtr<IDXGISwapChain>    _swapChain;
    ComPtr<ID3D12Resource>    _renderTargets[SWAP_CHAIN_BUFFER_COUNT];
    uint32                    _backBufferIndex = 0;
};

