cbuffer TRANSFORM_PARAMS : register(b0) // 행렬 관련 처리
{
    row_major matrix matWVP;
};

cbuffer MATERIAL_PARAMS : register(b1) // 머티리얼 관련 처리
{
    int int_0;
    int int_1;
    int int_2;
    int int_3;
    int int_4;
    float float_0;
    float float_1;
    float float_2;
    float float_3;
    float float_4;
};

Texture2D tex_0 : register(t0);
Texture2D tex_1 : register(t1);
Texture2D tex_2 : register(t2);
Texture2D tex_3 : register(t3);
Texture2D tex_4 : register(t4);

SamplerState sam_0 : register(s0);

struct VS_IN
{
    float3 pos : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
    
};

struct VS_OUT
{
    float4 pos : SV_Position;
    float4 color : COLOR;
    float2 uv : TEXCOORD;
};

VS_OUT VS_Main(VS_IN input)
{
    VS_OUT output = (VS_OUT)0;
    
    // 받은값에 WVP행렬을 곱해준다
    // float4의 마지막값이 1인 이유는 좌표이기 때문이다. 즉 점이기 때문
    // 방향성이 있는 벡터로 추출하고 싶다면 0으로 세팅하면 된다
    output.pos = mul(float4(input.pos, 1.f), matWVP);
    output.color = input.color;
    output.uv = input.uv;

    return output;
}

float4 PS_Main(VS_OUT input) : SV_Target
{
    //return input.color;
    float4 color = tex_0.Sample(sam_0, input.uv);
    return color;
}