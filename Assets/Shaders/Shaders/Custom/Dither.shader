Shader "Oxysoft/Dither"
{
    Properties
    {
        _Threshold ("Threshold", Float) = 0.45
        _Strength ("Strength", Float) = 0.45
        _Width ("Pattern Width", Int) = 4
        _Height ("Pattern Height", Int) = 4
        _Dither ("Dither Pattern", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Dither"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_Dither);
            SAMPLER(sampler_Dither);

            float _Threshold;
            float _Strength;
            int _Width;
            int _Height;

            float Luma(float3 color)
            {
                return dot(
                    color,
                    float3(0.299, 0.587, 0.114)
                );
            }

            float PatternValue(float2 screenPosition)
            {
                float width = max((float)_Width, 1.0);
                float height = max((float)_Height, 1.0);

                float2 pixel = floor(screenPosition);

                float2 patternPixel = float2(
                    fmod(pixel.x, width),
                    fmod(pixel.y, height)
                );

                // Sample the center of the selected pattern texel.
                float2 uv = (patternPixel + 0.5) /
                    float2(width, height);

                return SAMPLE_TEXTURE2D(
                    _Dither,
                    sampler_Dither,
                    uv
                ).r;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                float2 screenPosition = input.positionCS.xy;

                float brightness = Luma(source.rgb);
                float patternValue = PatternValue(screenPosition);

                float darken = brightness <
                    patternValue * _Threshold
                    ? _Strength
                    : 1.0;

                return half4(
                    source.rgb * darken,
                    source.a
                );
            }

            ENDHLSL
        }
    }
}