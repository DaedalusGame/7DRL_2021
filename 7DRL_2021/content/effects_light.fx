#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_3
#endif

static const float PI = 3.14159265f;
static const float TAU = 6.28318530f;
static const float NOISE_TEXTURE_SIZE = 256.0;

matrix WorldViewProjection;

texture2D texture_main : register(t0);
SamplerState sampler_main;

float3 lightPosition;

struct VertexLightShaderInput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
};

struct VertexLightShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : TEXCOORD2;
	float2 TextureCoordinates : TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
};

VertexLightShaderOutput LightVS(in VertexLightShaderInput input)
{
	VertexLightShaderOutput output = (VertexLightShaderOutput) 0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.WorldPosition = mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(float4(input.Normal, 0.0), WorldViewProjection)).xyz;
	output.TextureCoordinates = input.TextureCoordinates;
	
	return output;
}

float4 LightPS(in VertexLightShaderOutput input) : COLOR
{
	float4 color = texture_main.Sample(sampler_main, input.TextureCoordinates);
	
	float quantizeMod = 3.0;
	float3 worldPosition = input.WorldPosition.xyz;
	float3 worldNormal = normalize(input.Normal);
	float3 lightVector = normalize(lightPosition - worldPosition);
	float brightness = ceil(quantizeMod * max(0, min(1, dot(worldNormal, lightVector) + 0.3))) / quantizeMod;

	return float4(brightness * color.rgb, color.a);
}

technique Light
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL LightVS();
		PixelShader = compile PS_SHADERMODEL LightPS();
	}
};