// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MaskClipShader 1" 
{
  Properties 
  {
	_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
	_AlphaTex ("MaskTexture", 2D) = "white" {}
  }
 
  SubShader
  {
	Tags{
	  "Queue" = "Transparent"
	  "IgnoreProjector" = "True"
	  "RenderType" = "Transparent"
	}
	 Pass
	{
		Cull Off
		Lighting Off
		ZWrite Off
		Offset -1, -1
		Fog { Mode Off }
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc"
 
		sampler2D _MainTex;
		float4 _MainTex_ST;
		sampler2D _AlphaTex;
		float4 _ClipRange0 = float4(0.0, 0.0, 1.0, 1.0);
		float2 _ClipArgs0 = float2(1000.0, 1000.0);
 
		struct appdata_t
		{
			float4 vertex : SV_POSITION;
			half4 color : COLOR;
			float2 texcoord : TEXCOORD0;
		};
 
		struct v2f
		{
			float4 vertex : SV_POSITION;
			half4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float2 worldPos : TEXCOORD1;
		};
 
		v2f vert (appdata_t v)
		{
			v2f o = (v2f)0;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.color = v.color;
			o.texcoord = v.texcoord;
			o.worldPos = v.vertex.xy * _ClipRange0.zw + _ClipRange0.xy;
			return o;
		}
 
		half4 frag (v2f IN) : SV_Target
		{
			float2 factor = (float2(1.0, 1.0) - abs(IN.worldPos)) * _ClipArgs0;
			
			half4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
			half4 maskAlpha = tex2D(_AlphaTex, IN.texcoord);
			col.a *= clamp(min(factor.x, factor.y), 0.0, 1.0);
			col.a *= maskAlpha.a;
			return col;
		}
		ENDCG
	}
  }
}