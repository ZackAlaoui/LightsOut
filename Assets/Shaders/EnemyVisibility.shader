Shader "Custom/EnemyVisibility"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Threshold ("Light Threshold", Range(0, 1)) = 0.1
    }

     SubShader
     {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Visiblity"

            HLSLPROGRAM

            #pragma multi_compile _ _FORWARD_PLUS

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            float4 _BaseColor;
            float _Threshold;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 positionWS = IN.positionWS;

                InputData inputData = (InputData) 0;
                inputData.positionWS = positionWS;
                inputData.normalWS = normal;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(positionWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);

                float totalLight = 0;
                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, positionWS, half4(1,1,1,1));
                    float NdotL = dot(normal, normalize(light.direction));
                    totalLight += saturate(NdotL) * light.distanceAttenuation * light.shadowAttenuation;
                LIGHT_LOOP_END

                if (totalLight < _Threshold)
                {
                    discard;
                }

                return _BaseColor;
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipelin/Lit"
}
