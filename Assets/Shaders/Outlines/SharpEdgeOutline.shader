
Shader "Custom/SharpEdgeOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _NormalThreshold ("Normal Threshold", Range(0.01, 1)) = 0.25
        _DepthThreshold ("Depth Threshold", Range(0.001, 1)) = 0.1
        _OutlineThickness ("Outline Thickness", Range(0.5, 4)) = 1
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
            Blend Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            TEXTURE2D_X(_DefaultLayerMask);
            SAMPLER(sampler_DefaultLayerMask);

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

            float GetMask(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X(_DefaultLayerMask,sampler_DefaultLayerMask,saturate(uv)).r;
            }

            bool IsSky(float rawDepth)
            {
                #if UNITY_REVERSED_Z
                    return rawDepth <= 0.00001;
                #else
                    return rawDepth >= 0.99999;
                #endif
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);

                if (_EdgeStrength <= 0.0)
                    return sceneColor;

                // Only outline pixels belonging to the selected layer.
                float centerMask = GetMask(uv);

                if (centerMask < 0.5)
                    return sceneColor;

                float rawCenter = GetRawDepth(uv);

                if (IsSky(rawCenter))
                    return sceneColor;

                float centerDepth = GetLinearDepth(rawCenter);
                float3 centerNormal = GetNormal(uv);

                float2 texel = abs(_BlitTexture_TexelSize.xy)
                    * max(_OutlineThickness, 0.5);

                float normalEdge = 0.0;
                float depthEdge = 0.0;

                float2 offsets[4] =
                {
                    float2(texel.x, 0),
                    float2(-texel.x, 0),
                    float2(0, texel.y),
                    float2(0, -texel.y)};

                for (int i = 0; i < 4; i++)
                {
                    float2 sampleUV = saturate(uv + offsets[i]);

                    // Selected-layer boundary.
                    float sampleMask = GetMask(sampleUV);

                    if (sampleMask < 0.5)
                    {
                        depthEdge = 1.0;
                        continue;
                    }

                    float rawSample = GetRawDepth(sampleUV);

                    if (IsSky(rawSample))
                    {
                        depthEdge = 1.0;
                        continue;
                    }

                    float sampleDepth = GetLinearDepth(rawSample);
                    float3 sampleNormal = GetNormal(sampleUV);

                    // Relative depth difference is less dependent
                    // on the object's distance from the camera.
                    float depthDifference =
                        abs(centerDepth - sampleDepth)
                        / max(min(centerDepth, sampleDepth), 0.01);

                    float normalDifference =
                        1.0 - saturate(dot(centerNormal, sampleNormal));

                    depthEdge = max(depthEdge, depthDifference);
                    normalEdge = max(normalEdge, normalDifference);
                }

                float normalOutline = smoothstep(_NormalThreshold,_NormalThreshold + 0.1,normalEdge);

                float depthOutline = smoothstep(_DepthThreshold,_DepthThreshold * 2.0,depthEdge);

                float edge = saturate(
                    max(normalOutline, depthOutline) * _EdgeStrength);

                return lerp(sceneColor, _OutlineColor, edge);
            }
            ENDHLSL
        }
    }
}