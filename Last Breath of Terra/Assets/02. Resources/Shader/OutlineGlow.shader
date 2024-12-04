Shader "Custom/URPOutlineGlow"
{
    Properties
    {
        // 기본 Lit Shader 속성
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Glossiness("Smoothness", Range(0, 1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0

        // Outline 및 Glow 속성 추가
        [PerRendererData] _Outline("Outline", Float) = 0
        [PerRendererData] _OutlineColor("Outline Color", Color) = (1,1,1,1)
        [PerRendererData] _OutlineSize("Outline Size", float) = 1.0

        [PerRendererData] _Glow("Glow", Float) = 0
        [PerRendererData] _GlowColor("Glow Color", Color) = (0,1,0.8,1)
        [PerRendererData] _GlowIntensity("Glow Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseColor;

            float _Outline;
            float _OutlineSize;
            float4 _OutlineColor;

            float _Glow;
            float4 _GlowColor;
            float _GlowIntensity;

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                // 기본 색상 및 텍스처 샘플링
                float4 baseColor = _BaseColor * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                float3 normalWS = normalize(IN.normalWS);

                // 라이팅 계산
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionHCS.xyz);
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float3 lightColor = _MainLightColor.rgb;

                float NdotL = saturate(dot(normalWS, lightDir));
                float3 diffuse = baseColor.rgb * lightColor * NdotL;

                half4 litColor = half4(diffuse, baseColor.a);

                // Outline 및 Glow 적용
                half4 finalColor = litColor;

                if (_Outline > 0)
                {
                    // Outline 크기만큼 주변 픽셀 샘플링
                    float alphaThreshold = 0.01;
                    float4 outlineColor = _OutlineColor;

                    [unroll(4)]
                    for (int i = 1; i <= _OutlineSize; i++)
                    {
                        float2 offset = float2(i * 0.01, i * 0.01);
                        float4 sample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + offset);
                        if (sample.a < alphaThreshold)
                        {
                            finalColor.rgb = outlineColor.rgb;
                            break;
                        }
                    }
                }

                if (_Glow > 0)
                {
                    float glowFactor = 0.0;

                    // Glow 효과를 위해 주변 영역 샘플링
                    [unroll(8)]
                    for (int i = 1; i <= 8; i++)
                    {
                        float2 offset = float2(i * 0.005, i * 0.005);
                        glowFactor += SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv + offset).a;
                        glowFactor += SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv - offset).a;
                    }

                    glowFactor = saturate(glowFactor / 16.0) * _GlowIntensity;
                    finalColor.rgb += _GlowColor.rgb * glowFactor;
                }

                return finalColor;
            }
            ENDHLSL
        }
    }
}
