// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

Shader "Tiled2Unity/Default (Instanced)"
{
    Properties
    {
        _MainTex ("Tiled Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"
            #include "Tiled2Unity.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
            };


            UNITY_INSTANCING_BUFFER_START(MyProperties)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
#define _Color_arr MyProperties
            UNITY_INSTANCING_BUFFER_END(MyProperties)

            v2f vert(appdata_t In)
            {
                UNITY_SETUP_INSTANCE_ID(In);

                v2f Out;
                Out.vertex = UnityObjectToClipPos(In.vertex);
                Out.texcoord = In.texcoord;
                Out.color = In.color * UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);

                #ifdef PIXELSNAP_ON
                Out.vertex = UnityPixelSnap (Out.vertex);
                #endif

                return Out;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f In) : COLOR
            {
                half4 texcol = tex2D(_MainTex, In.texcoord);
                texcol = texcol * In.color;
                return texcol;
            }
        ENDCG
        }
    }

    Fallback "Sprites/Default"
}