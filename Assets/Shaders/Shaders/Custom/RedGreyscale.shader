Shader "Oxysoft/RedGrayscale"
{
    Properties
    {
        _RedThreshold ("Red Threshold", Range(0, 1)) = 0.2
        _RedTolerance ("Red Tolerance", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "RedGrayscale"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _RedThreshold;
            float _RedTolerance;

            half4 Frag(Varyings input) : SV_Target
            {
                half4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                float3 color = source.rgb;

                // Convert the pixel to grayscale.
                float grayscale = dot(
                    color,
                    float3(0.299, 0.587, 0.114)
                );

                float3 grayColor = grayscale.xxx;

                // Detect red: red must be strong and dominate green/blue.
                float redness = color.r - max(color.g, color.b);

                float redMask = smoothstep(
                    _RedThreshold,
                    _RedThreshold + max(_RedTolerance, 0.001),
                    redness
                );

                float3 finalColor = lerp(
                    grayColor,
                    color,
                    redMask
                );

                return half4(finalColor, source.a);
            }

            ENDHLSL
        }
    }
}