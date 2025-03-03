Shader "Custom/GlowArcShader"
{
    Properties
    {
        _Color ("Glow Color", Color) = (0, 1, 1, 1) // Glow 색상
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 5.0 // Glow 강도
        _BlurStrength ("Blur Strength", Range(0, 2)) = 0.5 // Glow 퍼짐 정도
        _GlowWidth ("Glow Width", Range(0, 1)) = 0.3 // 🔥 Glow 두께 조절
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend One One // 🔥 Additive Blending → Glow가 강하게
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _GlowIntensity;
            float _BlurStrength;
            float _GlowWidth;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 🔥 UV 기준으로 선을 따라 Glow가 퍼지도록 조절
                float dist = abs(i.uv.y - 0.5); // Y축을 기준으로 거리 계산 (선 중심 기준)
                
                // Glow 강도를 선 중심에서 점진적으로 줄어들게
                float glow = exp(-pow(dist / _GlowWidth, 2) * _BlurStrength * 5.0) * _GlowIntensity;

                return float4(_Color.rgb * glow, glow);
            }
            ENDCG
        }
    }
}
