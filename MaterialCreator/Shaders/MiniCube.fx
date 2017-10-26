struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	float2 tex : TEXCORD;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float2 tex : TEXCORD;
};

Texture2D picture : register(t0);
SamplerState pictureSampler : register(s0);

PS_IN VS(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = input.pos;
	output.tex = input.tex;
	output.col = input.col;

	return output;
}

float4 PS(PS_IN input) : SV_Target
{
	return picture.Sample(pictureSampler, input.tex);
}