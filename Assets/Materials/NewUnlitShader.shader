// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/NewUnlitShader"
{
	Properties
	{
		// Color property for material inspector, default to white
		//_Color("Main Color", Color) = (1,1,1,1)
		_MainColor("Main Color", Color) = (1,0,1,1)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			fixed4 _MainColor;
			struct appdata
			{
				float4 vertex: POSITION;
			};
			struct v2f
			{
				float4 clipPos: SV_POSITION;
				float4 color: COLOR;
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.color = v.vertex+0.5;
				o.clipPos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			
			ENDCG
		}
	}
}
