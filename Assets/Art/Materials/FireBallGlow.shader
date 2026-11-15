Shader "DodgeGame/FireBallGlow"
{
    Properties
    {
        _EmissionBase ("EmissionBase", Float) = 1.5
        _EmissionPulse ("EmissionPulse", Float) = 2.5
        _FlickerSpeed ("FlickerSpeed", Float) = 7.85398
        _RimColor ("RimColor", Color) = (1.0, 0.65, 0.1, 1.0)
        _RimPower ("RimPower", Float) = 3.0
        _BaseColor ("Base Color (Orange)", Color) = (1.0, 0.42, 0.05, 1.0)
        _BaseColorRed ("Base Color (Red)", Color) = (0.85, 0.08, 0.02, 1.0)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            CBUFFER_START(UnityPerMaterial)
                float _EmissionBase;
                float _EmissionPulse;
                float _FlickerSpeed;
                float4 _RimColor;
                float _RimPower;
                float4 _BaseColor;
                float4 _BaseColorRed;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInputs.positionCS;
                output.positionWS = vertexInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Orange to red vertical/radial surface gradient
                float gradientFactor = saturate(input.normalWS.y * 0.5 + 0.5);
                half3 surfaceColor = lerp(_BaseColorRed.rgb, _BaseColor.rgb, gradientFactor);

                // Animated intensity pulsing between EmissionBase (1.5) and EmissionBase + EmissionPulse (4.0) over ~0.8s
                float timeVal = _Time.y * _FlickerSpeed;
                float pulse = sin(timeVal) * 0.5 + 0.5;

                // Organic multi-harmonic noise offset so the flame flickers naturally
                float flickerNoise = (sin(timeVal * 3.73 + 1.25) * 0.5 + 0.5) * (cos(timeVal * 5.17) * 0.5 + 0.5);
                float combinedPulse = saturate(pulse * 0.85 + flickerNoise * 0.25);
                float emissionIntensity = _EmissionBase + (_EmissionPulse * combinedPulse);

                // Fresnel rim glow in yellow-orange for heat edge
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS);
                float3 normalWS = normalize(input.normalWS);
                float NdotV = saturate(dot(normalWS, viewDirWS));
                float fresnel = pow(1.0 - NdotV, _RimPower);
                half3 rimGlow = _RimColor.rgb * fresnel * (_EmissionBase + _EmissionPulse * 0.8);

                // Composite final lit emissive color
                half3 finalColor = (surfaceColor * emissionIntensity) + rimGlow;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs vertexInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInputs.positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
