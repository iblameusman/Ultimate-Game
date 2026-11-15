Shader "DodgeGame/FireTileGlow"
{
    Properties
    {
        _BaseColor ("Charred Rock Color", Color) = (0.12, 0.06, 0.04, 0.92)
        _EmberColor ("Ember Glow Color", Color) = (1.0, 0.42, 0.05, 1.0)
        _EmissionColor ("Emission Color", Color) = (1.0, 0.35, 0.02, 1.0)
        _EmissionIntensity ("Emission Intensity", Float) = 2.5
        _ShimmerSpeed ("Heat Shimmer Speed", Float) = 4.0
        _FadeAlpha ("Fade Alpha", Float) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent" 
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EmberColor;
                float4 _EmissionColor;
                float _EmissionIntensity;
                float _ShimmerSpeed;
                float _FadeAlpha;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInputs.positionCS;
                output.positionWS = vertexInputs.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 uv = input.uv;

                // Animated heat shimmer distortion wave
                float shimmerTime = _Time.y * _ShimmerSpeed;
                float2 distortion = float2(
                    sin(shimmerTime + uv.y * 12.0) * 0.025,
                    cos(shimmerTime * 1.2 + uv.x * 12.0) * 0.025
                );
                float2 distortedUV = uv + distortion;

                // Procedural glowing embers / cracked hot floor pattern
                float2 grid = frac(distortedUV * 6.0) - 0.5;
                float distToCenter = length(grid);
                float crackPattern = sin(distortedUV.x * 18.0 + sin(distortedUV.y * 15.0)) * 
                                     cos(distortedUV.y * 18.0 + cos(distortedUV.x * 15.0));
                float emberGlow = saturate(crackPattern * 0.8 + (1.0 - smoothstep(0.15, 0.45, distToCenter)));

                // Organic pulsing heat fluctuation
                float heatPulse = sin(_Time.y * 5.0 + uv.x * 3.0) * 0.2 + 0.8;
                half3 surfaceColor = lerp(_BaseColor.rgb, _EmberColor.rgb, emberGlow);
                half3 emission = _EmissionColor.rgb * (_EmissionIntensity * emberGlow * heatPulse);

                half3 finalRGB = surfaceColor + emission;
                half finalAlpha = _BaseColor.a * _FadeAlpha * saturate(1.2 - length(uv - 0.5) * 1.8);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
