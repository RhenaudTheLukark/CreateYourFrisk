// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Transparent/Cutout/Diffuse Shake" {
 
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Detail("Detail", 2D) = "gray" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    _ShakeDisplacement ("Displacement", Range (0, 1.0)) = 1.0
    _ShakeTime ("Shake Time", Range (0, 1.0)) = 1.0
    _ShakeWindspeed ("Shake Windspeed", Range (0, 1.0)) = 1.0
    _ShakeBending ("Shake Bending", Range (0, 1.0)) = 1.0
}
 
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 200
   
CGPROGRAM
#pragma target 3.0
#pragma surface surf Lambert alphatest:_Cutoff vertex:vert addshadow
 
sampler2D _MainTex;
sampler2D _Detail;
fixed4 _Color;
float _ShakeDisplacement;
float _ShakeTime;
float _ShakeWindspeed;
float _ShakeBending;
 
struct Input {
	float2 uv_MainTex;
	float4 screenPos;
};
 
void FastSinCos (float4 val, out float4 s, out float4 c) {
    val = val * 6.408849 - 3.1415927;
    float4 r5 = val * val;
    float4 r6 = r5 * r5;
    float4 r7 = r6 * r5;
    float4 r8 = r6 * r5;
    float4 r1 = r5 * val;
    float4 r2 = r1 * r5;
    float4 r3 = r2 * r5;
    float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841} ;
    float4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587} ;
    s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;
    c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}
 
 
void vert (inout appdata_full v) {
   
    float factor = (1 - _ShakeDisplacement -  v.color.r) * 0.5;
       
    const float _WindSpeed  = (_ShakeWindspeed  +  v.color.g );    
    const float _WaveScale = _ShakeDisplacement;
   
    const float4 _waveXSize = float4(0.048, 0.06, 0.24, 0.096);
    const float4 waveSpeed = float4 (1.2, 2, 1.6, 4.8);
 
    float4 _waveXmove = float4(0.024, 0.04, -0.12, 0.096);
   
    float4 waves;
    waves = v.vertex.x * _waveXSize;
 
    waves += _Time.x * (1 - _ShakeTime * 2 - v.color.b ) * waveSpeed *_WindSpeed;
 
    float4 s, c;
    waves = frac (waves);
    FastSinCos (waves, s,c);
 
    float waveAmount = v.texcoord.y * (v.color.a + _ShakeBending);
    s *= waveAmount;
 
    s *= normalize (waveSpeed);
 
    s = s * s;
    float fade = dot (s, 1.3);
    s = s * s;
    float3 waveMove = float3 (0,0,0);
    waveMove.x = dot (s, _waveXmove);
    v.vertex.xz -= mul ((float3x3)unity_WorldToObject, waveMove).x;
   
}
 
void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
	float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
	screenUV *= float2(40, 40);
	o.Albedo *= tex2D(_Detail, screenUV).rgb * 2;
}
ENDCG
}
 
Fallback "Transparent/Cutout/VertexLit"
}