Shader "BB/Video/ChromaKeyMagenta"
{
    Properties
    {
        _MainTex ("Video Texture", 2D) = "black" {}
        _KeyColor ("Key Color", Color) = (1, 0, 1, 1)
        _Tolerance ("Tolerance", Range(0.001, 0.5)) = 0.08
        _Feather ("Feather", Range(0, 0.5)) = 0.035
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _KeyColor;
            float _Tolerance;
            float _Feather;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float distanceFromKey = distance(col.rgb, _KeyColor.rgb);
                float alpha = smoothstep(_Tolerance, _Tolerance + _Feather, distanceFromKey);
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
