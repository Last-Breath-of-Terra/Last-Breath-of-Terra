Shader "Custom/GlowArcShader"
{
    Properties
    {
        _Color ("Glow Color", Color) = (0, 1, 1, 1) // 기본 파란색 Glow
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 3.0 // 강한 Glow
        _BlurStrength ("Blur Strength", Range(0, 1)) = 0.5 // Glow 퍼짐 정도
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 부드러운 Alpha Blending
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; // UV 좌표
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _GlowIntensity;
            float _BlurStrength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UV 중심으로 Radial Blur 적용
                float2 centeredUV = i.uv - 0.5;
                float dist = length(centeredUV);

                // Glow 강도를 거리에 따라 점점 약하게
                float glow = exp(-dist * _BlurStrength * 5.0) * _GlowIntensity;

                return float4(_Color.rgb * glow, glow);
            }
            ENDCG
        }
    }
}
