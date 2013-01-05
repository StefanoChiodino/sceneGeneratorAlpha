float4x4 World;
float4x4 View;
float4x4 Projection;

float4 WaveSpeeds;
float4 WaveHeights;
float4 WaveLengths;
float3 CameraPosition;
float2 WaveDir0;
float2 WaveDir1;
float2 WaveDir2;
float2 WaveDir3;

float TexStretch;
float BumpStrength;
float Time;
float FresnelTerm;
int SpecularPowerTerm;

Texture TextureCube;
samplerCUBE TextureCubeSampler = sampler_state
{
	texture = <TextureCube>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};
Texture BumpMap;
sampler BumpMapSampler = sampler_state
{
	texture = <BumpMap>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = mirror;
	AddressV = mirror;
};

struct App2VS
{
	float2 Position		: POSITION0;
	float2 TexCoord		: TEXCOORD0;	
};
struct VS2PS
{
	float4 Position			: POSITION;
	float2 TexCoord			: TEXCOORD0;
	float3 Position3D		: TEXCOORD1;
	float3 Normal			: TEXCOORD2;
};
struct PS2Screen
{
    float4 Color 	: COLOR0;
};
    
VS2PS VS(App2VS input)
{
	VS2PS output = (VS2PS)0;	
	
	// create waves perpendicular to the direction
	float4 dotProducts;
    dotProducts.x = dot(WaveDir0, input.Position);
    dotProducts.y = dot(WaveDir1, input.Position);
    dotProducts.z = dot(WaveDir2, input.Position);
    dotProducts.w = dot(WaveDir3, input.Position);        
    
    // adjust the lenght by the wave lengths and add the time variable
    // adjusted by the wave speeds
    float4 arguments = dotProducts / WaveLengths + Time * WaveSpeeds;
    float4 heights = WaveHeights * sin(arguments);
    
    // the final 3d position, xz from input
    // and y from heights
    float4 position3D = 1;
    position3D.xz = input.Position;
    position3D.y = heights.x;
    position3D.y += heights.y;
    position3D.y += heights.z;
    position3D.y += heights.w;
	
	float4x4 ViewProjection = mul(View, Projection);
	float4x4 WorldViewProjection = mul(World, ViewProjection);
	output.Position = mul(position3D, WorldViewProjection);	
	output.Position3D = mul(position3D, World);
	
	// computing normals, the derivative of the sine is the cosine
	//float4 derivatives = WaveHeights * cos(arguments);
	float4 derivatives = WaveHeights * cos(arguments) / WaveLengths;
	float2 deviations = derivatives.x * WaveDir0;
    deviations += derivatives.y * WaveDir1;
    deviations += derivatives.z * WaveDir2;
    deviations += derivatives.w * WaveDir3;	
	
	// computing 3 perpendicular direction
	float3 Normal = float3(-deviations.x, 1, -deviations.y);
	Normal = normalize(Normal);
	//float3 Binormal = float3(1, deviations.x, 0);
    //float3 Tangent = float3(0, deviations.y, 1);    
	
	//float3x3 tangent;
	//tangent[0] = normalize(Binormal);
	//tangent[1] = normalize(Tangent);
	//tangent[2] = normalize(Normal);		
	
	//float3x3 tangentWorld = mul(tangent, World);
	//output.TangentWorld = tangentWorld;
	
	output.Normal = Normal;
	
	// apply the bumpMap flowing on the direction of the first wave
	output.TexCoord = input.TexCoord + Time / 50.0f * WaveDir0;
	
	return output;
}

PS2Screen PS(VS2PS input) : COLOR0
{
	PS2Screen output = (PS2Screen)0;
	// sampling thrice, from color [0,1]*3 to deviation [-0.5, 0.5]
	float3 bump = tex2D(BumpMapSampler, TexStretch * input.TexCoord) +
		tex2D(BumpMapSampler, TexStretch * 1.3 * input.TexCoord) +
		tex2D(BumpMapSampler, TexStretch * 2.6 * input.TexCoord) - 1.5f;
	bump /= 3;

	float3 bumpedNormal = bump * BumpStrength + input.Normal;
		
	float3 cameraVector = normalize(input.Position3D - CameraPosition);
	float3 reflection = reflect(cameraVector, bumpedNormal);		
	float4 reflectiveColor = texCUBE(TextureCubeSampler, reflection);
	reflectiveColor = pow(reflectiveColor, SpecularPowerTerm);
	
	//float3 up = float3 (0, 1, 0);
	//FresnelTerm = FresnelTerm + dot(-cameraVector, input.Normal);
	//FresnelTerm = FresnelTerm / 3;
	
	float4 waterColor = float4 (0,0.15,0.4,1);	
	output.Color = waterColor * FresnelTerm + reflectiveColor * (1 - FresnelTerm);
	
	return output;
}

technique OceanGenerator
{
	pass Pass0
    {  
    	VertexShader = compile vs_2_0 VS();
        PixelShader  = compile ps_2_0 PS();
    }
}
