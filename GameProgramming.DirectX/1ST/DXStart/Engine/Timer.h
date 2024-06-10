#pragma once
class Timer
{
	/*
	* 기능 명세서
	* 
	* 1. 초기화
	* 2. 업데이트
	* 3. GET 델타타임
	* 4. GET 초당 프레임
	*/

public:
    void Init();
    void Update();

    uint32 GetFps() { return _fps; }
    float GetDeltaTime() { return _deltaTime; }

private:
    uint64    _frequency = 0;
    uint64    _prevCount = 0;
    float    _deltaTime = 0.f;

    // 초당프레임 체크 관련
private:
    uint32    _frameCount = 0;
    float    _frameTime = 0.f;
    uint32    _fps = 0;

};

