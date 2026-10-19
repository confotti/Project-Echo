Shader "Custom/SharpEdgeOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _NormalThreshold ("Normal Threshold", Range(0.01, 1)) = 0.25
        _DepthThreshold ("Depth Threshold", Range(0.001, 1)) = 0.1
        _OutlineThickness ("Outline Thickness", Range(0.5, 4)) = 0.1
        _EdgeStrength ("Edge Strength", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "SharpEdgeOutline"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _NormalThreshold;
                float _DepthThreshold;
                float _OutlineThickness;
                float _EdgeStrength;
            CBUFFER_END

            float GetRawDepth(float2 uv)
            {
                return SampleSceneDepth(saturate(uv));
            }

            float GetLinearDepth(float rawDepth)
            {
                return LinearEyeDepth(rawDepth, _ZBufferParams);
            }

            float3 GetNormal(float2 uv)
            {
                return normalize(SampleSceneNormals(saturate(uv)));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;

                half4 sceneColor = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    uv
                );

                float2 texel = _BlitTexture_TexelSize.xy * _OutlineThickness;

                float rawCenter = GetRawDepth(uv);

                // Detect background/sky pixels.
                #if UNITY_REVERSED_Z
                    bool centerIsSky = rawCenter <= 0.00001;
                #else
                    bool centerIsSky = rawCenter >= 0.99999;
                #endif

                if (centerIsSky)
                    return sceneColor;

                float depthCenter = GetLinearDepth(rawCenter);
                float3 normalCenter = GetNormal(uv);

                float normalEdge = 0.0;
                float depthEdge = 0.0;

                float2 offsets[4] =
                {
                    float2(texel.x, 0),
                    float2(-texel.x, 0),
                    float2(0, texel.y),
                    float2(0, -texel.y)
                };

                for (int i = 0; i < 4; i++)
                {
                    float2 sampleUV = saturate(uv + offsets[i]);
                    float rawSample = GetRawDepth(sampleUV);

                    #if UNITY_REVERSED_Z
                        bool sampleIsSky = rawSample <= 0.00001;
                    #else
                        bool sampleIsSky = rawSample >= 0.99999;
                    #endif

                    // Silhouette edges: geometry next to the sky.
                    if (sampleIsSky)
                    {
                        depthEdge = max(depthEdge, 1.0);
                        continue;
                    }

                    float sampleDepth = GetLinearDepth(rawSample);
                    float3 sampleNormal = GetNormal(sampleUV);

                    float normalDifference =
                        1.0 - saturate(dot(normalCenter, sampleNormal));

                    // Relative depth difference is less sensitive to distance.
                    float depthDifference =
                        abs(depthCenter - sampleDepth) /
                        max(min(depthCenter, sampleDepth), 0.01);

                    normalEdge = max(normalEdge, normalDifference);
                    depthEdge = max(depthEdge, depthDifference);
                }

                float normalOutline = smoothstep(
                    _NormalThreshold,
                    _NormalThreshold + 0.1,
                    normalEdge
                );

                float depthOutline = smoothstep(
                    _DepthThreshold,
                    _DepthThreshold * 2.0,
                    depthEdge
                );

                float edge = max(normalOutline, depthOutline) * _EdgeStrength;

                return lerp(sceneColor, _OutlineColor, saturate(edge));
            }
            ENDHLSL
        }
    }
}