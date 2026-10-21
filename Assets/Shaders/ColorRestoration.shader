
Shader "Custom/ColorRestorationFullscreen"
{
    Properties
    {
        _RedThreshold ("Red Threshold", Range(0, 1)) = 0.2
        _RedTolerance ("Red Tolerance", Range(0, 1)) = 0.25
        _Contrast ("Grayscale Contrast", Range(0.5, 2)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ColorRestoration"

            ZWrite Off
            ZTest Always
            Cull Off
            Blend Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4 _ColorSource;
            float _ColorRadius;
            float _EdgeWidth;
            float _ColorAmount;

            float _RedThreshold;
            float _RedTolerance;
            float _Contrast;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture,sampler_LinearClamp,uv);

                float3 color = sceneColor.rgb;
                float redness = color.r - max(color.g, color.b);

                float redMask = smoothstep(_RedThreshold,_RedThreshold + max(_RedTolerance, 0.001),redness);

                float gray = dot(color,float3(0.299, 0.587, 0.114));
                gray = saturate((gray - 0.5) * _Contrast + 0.5);

                float3 grayscale = gray.xxx;
                float3 grayWithRed = lerp(grayscale,color,redMask);

                float rawDepth = SampleSceneDepth(uv);

                #if UNITY_REVERSED_Z
                    bool isSky = rawDepth <= 0.00001;
                #else
                    bool isSky = rawDepth >= 0.99999;
                #endif

                if (isSky)
                {
                    float3 skyResult = lerp(grayWithRed,color,saturate(_ColorAmount));

                    return half4(skyResult, sceneColor.a);
                }

                #if !UNITY_REVERSED_Z
                    rawDepth = lerp(UNITY_NEAR_CLIP_VALUE,1.0,rawDepth); 
                #endif

                float3 worldPosition = ComputeWorldSpacePosition(uv,rawDepth,UNITY_MATRIX_I_VP);
                float distanceToPlayer = distance(worldPosition.xz,_ColorSource.xz);

                float radius = max(_ColorRadius, 0.0);
                float edge = max(_EdgeWidth, 0.001);
                float colorMask = 0.0;

                if (radius > 0.001)
                {
                    colorMask = 1.0 - smoothstep(max(radius - edge, 0.0),radius + edge,distanceToPlayer);
                }

                // Blend grayscale and original color using the radius.
                float3 worldResult = lerp(grayWithRed,color,colorMask);

                return half4(worldResult, sceneColor.a);
            }
            ENDHLSL
        }
    }
}