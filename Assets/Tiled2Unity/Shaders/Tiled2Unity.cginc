// Make up for auto-upgrade with shaders. Allows us to keep backwards compatability with older versions of Unity.
//UNITY_SHADER_NO_UPGRADE

#ifndef TILED_2_UNITY_INCLUDED
#define TILED_2_UNITY_INCLUDED

// This file is always included in all unity shaders.
#include "UnityShaderVariables.cginc"

// We need to define UnityObjectToClipPos for Unity versions before 5.4
#if UNITY_VERSION < 540

// Tranforms position from object to homogenous space
inline float4 UnityObjectToClipPos(in float3 pos)
{
    // More efficient than computing M*VP matrix product
    return mul(UNITY_MATRIX_VP, mul(_Object2World, float4(pos, 1.0)));
}
inline float4 UnityObjectToClipPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
    return UnityObjectToClipPos(pos.xyz);
}

#endif

#endif
