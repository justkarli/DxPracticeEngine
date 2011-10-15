//--------------------------------------------------------------------------------------
// File: pass through shader.fx
//
// 
//--------------------------------------------------------------------------------------
Texture2D g_MeshTexture;        // Color texture for mesh

float  g_fTime ;                // App's time in seconds
matrix g_mWorldViewProjection ; // World * View * Projection matrix

//--------------------------------------------------------------------------------------
// States States States
//--------------------------------------------------------------------------------------
BlendState SrcAlphaBlendingAdd
{
    BlendEnable[0] = TRUE;
    SrcBlend = SRC_ALPHA;
    DestBlend = INV_SRC_ALPHA;
    BlendOp = ADD;
    SrcBlendAlpha = ZERO;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
	ALPHATOCOVERAGEENABLE = true;
};

//--------------------------------------------------------------------------------------
// Texture sampler
//--------------------------------------------------------------------------------------
SamplerState samLinear
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

//--------------------------------------------------------------------------------------
// Vertex shader input structure
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 vPosObject   : POSITION;
    float2 vTexUV       : TEXCOORD0;
};

struct VS_COLOR_INPUT
{
	float3 vPos : POSITION;
	float4 vColor : COLOR;
};
//--------------------------------------------------------------------------------------
// Pixel shader input structure
//--------------------------------------------------------------------------------------
struct PS_INPUT
{
    float4 vPosProj : SV_POSITION;
    float2 vTexUV   : TEXCOORD0;
};

struct PS_COLOR_INPUT 
{
	float4 vPos : SV_POSITION;
	float4 vColor : COLOR;
};


PS_INPUT VS( VS_INPUT input)
{
    PS_INPUT output;
    
    // Transform the position into  projected space for display
    output.vPosProj = mul( float4(input.vPosObject,1), g_mWorldViewProjection );
    // Pass the texture coordinate
    output.vTexUV = input.vTexUV;

    return output;
}

PS_COLOR_INPUT VS_COLOR(VS_COLOR_INPUT input) 
{
	PS_COLOR_INPUT output;
	output.vPos = mul( float4(input.vPos,1), g_mWorldViewProjection );
	output.vColor = input.vColor;

	return output;
}

float4 PS_COLOR(PS_COLOR_INPUT input) : SV_Target 
{
	return input.vColor;
}

float4 PS( PS_INPUT input) : SV_Target
{
    float4 output = g_MeshTexture.Sample( samLinear, input.vTexUV );
	//output[0] = output[3];
    //output[1] = output[2] = 0;
    //output[3] = 1;
    return output;
}



//--------------------------------------------------------------------------------------
// Techniques
//--------------------------------------------------------------------------------------
technique10 Default
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );

		//SetBlendState( SrcAlphaBlendingAdd, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
    }
}

technique10 Color 
{
	pass P0 
	{
		SetVertexShader( CompileShader( vs_4_0, VS_COLOR() ) );
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PS_COLOR() ) );
	}
}
