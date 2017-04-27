// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

    Shader "Selfmade/FlagWave"
    {
     
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" { }
    }
     
    SubShader
    {
        Pass
        {
            CULL Off
           
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
           
            float4 _Color;
            sampler2D _MainTex;
           
            //float4 _Time;
           
            // vertex input: position, normal
            struct appdata {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
            };
           
            struct v2f {
                float4 pos : POSITION;
                float2 uv: TEXCOORD0;
            };
           
            v2f vert (appdata v) {
                v2f o;
               
                float angle= _Time * 50;
               
                v.vertex.x =  v.texcoord.x * sin(v.vertex.x + angle);
                v.vertex.x += sin(v.vertex.z / 2 + angle);
                v.vertex.x *= v.vertex.x * 0.1f;
               
                o.pos = UnityObjectToClipPos( v.vertex );
                o.uv = v.texcoord;
                return o;
            }
           
            float4 frag (v2f i) : COLOR
            {
                half4 color = tex2D(_MainTex, i.uv);
                return color;
            }
     
            ENDCG
           
     
            //SetTexture [_MainTex] {combine texture}
        }
    }
    Fallback "VertexLit"
    }
