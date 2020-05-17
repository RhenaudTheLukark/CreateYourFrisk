Shader "Custom/Code_Test"
{
	Properties
	{
		_Texture ("Texture", 2D) = "white" {}
		_Limit ("Float", float) = 30

	}
	SubShader
	{
		//Tags { "RenderType"="Transparent" }
		Tags { "QUEUE"="Transparent" "RenderType"="Transparent" "RenderPipeline"="HDRenderPipeline" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _Texture;
			float _Limit;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;//TRANSFORM_TEX(v.uv, _Texture);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 final_col;
				if (i.vertex.y >= _Limit) {
					fixed4 col = tex2D(_Texture, (i.uv + float2( sin(_Time[1] * 3 + i.vertex.y/50) / 30 , 0 ) ) );
					fixed4 col2 = tex2D(_Texture, (i.uv + float2( -1 * sin(_Time[1] * 3 + i.vertex.y/50) / 30 , 0 ) ) );
					fixed4 col3 = tex2D(_Texture, (i.uv + float2( sin(_Time[1] * 3 + i.vertex.y/50 + 60) / 30 , 0 ) ) );

					col.a = col.a/3;
					col2.a = col2.a/3;
					col3.a = col3.a/3;

					final_col = col + col2 + col3;

					// apply fog
					//UNITY_APPLY_FOG(i.fogCoord, col);
					//col = 1-col;
				} else {
					final_col = tex2D(_Texture, i.uv);
				}
				//final_col = 1 - final_col;
				return final_col;
			}
			ENDCG
		}
	}
}
