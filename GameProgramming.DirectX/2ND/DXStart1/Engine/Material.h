#pragma once

class Shader;
class Texture;

enum
{
    MATERIAL_INT_COUNT = 5,
    MATERIAL_FLOAT_COUNT = 5,
    MATERIAL_TEXTURE_COUNT = 5,
};

struct MaterialParams
{
    void SetInt(uint8 index, int32 value) { intParams[index] = value; }
    void SetFloat(uint8 index, float value) { floatParams[index] = value; }

    // array는 고정된 배열형태이며 범위체크를 자동을 해줘서
    // 크래시가 날 가능성을 없애준다(디버그 모드 한정)
    array<int32, MATERIAL_INT_COUNT> intParams;
    array<float, MATERIAL_FLOAT_COUNT> floatParams;
};


class Material
{
public:
	shared_ptr<Shader> GetShader() { return _shader; }

    void SetShader(shared_ptr<Shader> shader) { _shader = shader; }
    void SetInt(uint8 index, int32 value) { _params.SetInt(index, value); }
    void SetFloat(uint8 index, float value) { _params.SetFloat(index, value); }
    void SetTexture(uint8 index, shared_ptr<Texture> texture) { _textures[index] = texture; }

    void PushData();

private:
	shared_ptr<Shader>    _shader;

    MaterialParams        _params;
    array<shared_ptr<Texture>, MATERIAL_TEXTURE_COUNT> _textures;
};

