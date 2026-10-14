Shader "Custom/ColorRestoration"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _ColorRadius ("Color Radius", Float) = 5
        _EdgeWidth ("Edge Width", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ColorRadius;
                float _EdgeWidth;
            CBUFFER_END

            float4 _ColorSource;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs pos =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS =
                    TransformObjectToWorldNormal(input.normalOS);

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogCoord = ComputeFogFactor(pos.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv)
                    * _BaseColor;

                float3 normalWS = normalize(input.normalWS);

                // Basic main-light shading
                Light mainLight = GetMainLight();

                float NdotL = saturate(
                    dot(normalWS, mainLight.direction)
                );

                float3 lighting =
                    SampleSH(normalWS) +
                    mainLight.color * NdotL;

                float3 color = baseColor.rgb * lighting;

                // Grayscale conversion
                float gray = dot(
                    color,
                    float3(0.299, 0.587, 0.114)
                );

                float3 grayscale = gray.xxx;

                // Distance from the surface to the color source
                float distanceToSource = distance(
                    input.positionWS,
                    _ColorSource.xyz
                );

                // Restore color near the source
                float edgeWidth = max(_EdgeWidth, 0.001);

                float colorMask = 1.0 - smoothstep(
                    _ColorRadius - edgeWidth,
                    _ColorRadius + edgeWidth,
                    distanceToSource
                );

                float3 result = lerp(
                    grayscale,
                    color,
                    colorMask
                );

                result = MixFog(result, input.fogCoord);

                return half4(result, baseColor.a);
            }

            ENDHLSL
        }
    }
}