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
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        } //반투명 렌더링의 투명큐 + 투명한 오브젝트 처리..?
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 부드러운 Alpha Blending
            ZWrite Off //깊이(depth) 업데이트 비활성화 < 겹쳐보이게?
            Cull Off //모든 면을 렌더링

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t //입력구조체
            {
                float4 vertex : POSITION; //3D 모델의 정점(position)
                float2 uv : TEXCOORD0; // UV 좌표
            };

            struct v2f //버텍스쉐이더에서 프래그먼트 쉐이더로 전달되는 구조체
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _GlowIntensity;
            float _BlurStrength;

            v2f vert(appdata_t v) //버텍스 쉐이더
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