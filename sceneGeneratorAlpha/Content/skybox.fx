float4x4 World;
float4x4 View;
float4x4 Projection;

Texture TextureCube;
sampler TextureCubeSampler = sampler_state 
{
	texture = <TextureCube>; 
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter = LINEAR; 
	AddressU = mirror; 
	AddressV = mirror;
};

struct VS2PS
{
    float4 Position   		: POSITION;    
    float3 Position3D		: TEXCOORD0;
};

struct PS2Screen
{
    float4 Color : COLOR0;
};


VS2PS VS (float4 Position : POSITION)
{	
	VS2PS output = (VS2PS)0;
	float4x4 preViewProjection = mul (View, Projection);
	float4x4 preWorldViewProjection = mul (World, preViewProjection);
	
	output.Position = mul(Position, preWorldViewProjection);
	output.Position3D = Position;
    
	return output;    
}

PS2Screen PS(VS2PS PSIn) 
{
	PS2Screen output = (PS2Screen)0;		
	
	output.Color = texCUBE(TextureCubeSampler, PSIn.Position3D);

	return output;
}

technique Skybox
{
	pass Pass0
    {   
    	VertexShader = compile vs_1_1 VS();
        PixelShader  = compile ps_1_1 PS();
    }
}

