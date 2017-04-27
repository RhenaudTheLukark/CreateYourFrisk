// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Download latest version at http://www.console-dev.de
#ifndef GRABPASSDISTORTION_GCINC
#define GRABPASSDISTORTION_GCINC

// _MainTex is the distortion texture and should use "Bypass sRGB Sampling" import setting.
sampler2D _MainTex;

// _MainTex tiling and offset properties.
float4 _MainTex_ST;

// _GrabTexture contains the contents of the screen where the object is about to be drawn.
sampler2D _GrabTexture;
			
// x=horizontal intensity, y=vertical intensity
// z=horizontal scrolling speed, w=vertical scrolling speed
float4 _IntensityAndScrolling;

// x=near distance at which distortions have full intensity
// y=far distance at which distortions have zero intensity
half2 _DistanceFade;

struct appdata_t
{
	float4 vertex  : POSITION;
	half2 texcoord : TEXCOORD0;
	fixed4 color   : COLOR;
};

struct v2f
{
	float4 vertex  : SV_POSITION;
	fixed4 color   : COLOR;		// a=distortion intensity multiplier
	half4 texcoord : TEXCOORD0; // xy=distort uv, zw=mask uv
	half4 screenuv : TEXCOORD1; // xy=screenuv, z=distance dependend intensity, w=depth
};

// ----------------------------------------------------------------------------
// Helper Functions
// ----------------------------------------------------------------------------
inline float2 Repeat(float2 t, float2 length)
{
	return t - floor(t / length) * length;
}

inline float2 PingPong(float2 t, float2 length)
{
	t = Repeat(t, length * 2);
	return length - abs(t - length);
}

// ----------------------------------------------------------------------------
// Vertex Shader
// ----------------------------------------------------------------------------
v2f vert (appdata_t v)
{
	v2f o = (v2f)0;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.color  = v.color;

	// texcoord.xy stores the distortion texture coordinates.
	o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex); // Apply texture tiling and offset.
	o.texcoord.xy += _Time.gg * _IntensityAndScrolling.zw; // Apply texture scrolling.

	// texcoord.zw stores the distortion mask texture coordinates.
	// We don't want to scroll the mask, so we just use the original texture coords.
	o.texcoord.zw = v.texcoord;

	half4 screenpos = ComputeGrabScreenPos(o.vertex);
	o.screenuv.xy = screenpos.xy / screenpos.w;

	// Calculate distance dependend intensity.
	// Blend intensity linearily between near to far params.
	half depth = length(mul(UNITY_MATRIX_MV, v.vertex));
	o.screenuv.z = saturate((_DistanceFade.y - depth) / (_DistanceFade.y - _DistanceFade.x));
	o.screenuv.w = depth;

	return o;
}
		
// ----------------------------------------------------------------------------
// Pixel Shader
// ----------------------------------------------------------------------------
fixed4 frag (v2f i) : COLOR {
	half2 distort = tex2D(_MainTex, i.texcoord.xy).xy;
				
	// distort*2-1 transforms range from 0..1 to -1..1.
	// negative values move to the left, positive to the right.
	half2 offset = (distort.xy * 2 - 1) * _IntensityAndScrolling.xy * i.screenuv.z * i.color.a;
							
#if MASK
	// _MainTex stores in the blue channel the mask.
	// The mask intensity represents how strong the distortion should be for this pixel.
	// black=no distortion, white=full distortion
	half mask = tex2D(_MainTex, i.texcoord.zw).b;				
	offset *= mask;
#endif							
	
#if ENABLE_CLIP				
	// Clip pixel if offset is really small. This makes masked particle
	// distortions blend together slightly better.
	clip(dot(offset,1) - 0.0001);					
#endif					

	// get screen space position of current pixel
	half2 uv = i.screenuv.xy + offset;

#if MIRROR_EDGE
	// Mirror uv's when it goes out of the screen.
	// This avoids streched seams at screen borders by introducing
	// these kind of mirroring artifacts. It looks less disturbing than the border seams though.
	uv = PingPong(uv, 1);
#endif

	half4 color = tex2D(_GrabTexture, uv);
	UNITY_OPAQUE_ALPHA(color.a);
				
#if DEBUGUV
	color.rg = uv;
	color.b = 0;
#endif

#if DEBUGDISTANCEFADE
	color.rgb = lerp(half3(1,0,0), half3(0,1,0), i.screenuv.z);
#endif

	//color.rgb = float3(fade,fade,fade)*15;
	return color;
}

#endif // GRABPASSDISTORTION_GCINC
