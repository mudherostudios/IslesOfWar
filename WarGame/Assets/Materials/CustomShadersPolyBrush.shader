Shader "Unlit/CustomShadersPolyBrush"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Texture1("Texture", 2D) = "white" {}
		_Texture2("Texture", 2D) = "white" {}
		_Texture3("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			#define Z_TEXTURE_CHANNELS 4
			#define Z_MESH_ATTRIBUTES COLOR


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Texture1;
			sampler2D _Texture2;
			sampler2D _Texture3;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col1 = tex2D(_MainTex, i.uv)* i.color.r;
				fixed4 col2 = tex2D(_Texture1, i.uv)* i.color.B;
				fixed4 col3 = tex2D(_Texture2, i.uv)* i.color.b;
				fixed4 col4 = tex2D(_Texture3, i.uv)* i.color.a;

				fixed4 final = col1 + col2 + col3 + col4;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, final);
				return col;
			}
			ENDCG
		}
	}
}
