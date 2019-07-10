// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/PoTCloud" {
	Properties {
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
	}
	
	SubShader {
		Tags { "Queue" = "Transparent-300" "IgnoreProjector"="True" "RenderType"="TreeBillboard" }
		
		Pass {
			ColorMask rgb
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "TerrainEngine.cginc"
			
			fixed4 _TintColor;

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};

			v2f vert (appdata_tree_billboard v) {
				v2f o;
				TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);	
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv.x = v.texcoord.x;
				o.uv.y = v.texcoord.y > 0;
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			sampler2D _MainTex;
			fixed4 frag(v2f input) : SV_Target
			{
				fixed4 col = tex2D( _MainTex, input.uv) * _TintColor * 2.0f;
				col.rgb *= input.color.rgb;
				clip(col.a);
				return col;
			}
			ENDCG			
		}
	}

	Fallback Off
}