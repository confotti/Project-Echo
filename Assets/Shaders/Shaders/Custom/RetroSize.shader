Shader "Oxysoft/RetroSize"
{
    Properties
    {
        _HorizontalResolution ("Horizontal Resolution", Int) = 160
        _VerticalResolution ("Vertical Resolution", Int) = 144
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "RetroSize"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            int _HorizontalResolution;
            int _VerticalResolution;

            half4 Frag(Varyings input) : SV_Target
            {
                float2 resolution = float2(
                    max(_HorizontalResolution, 1),
                    max(_VerticalResolution, 1)
                );

                // Snap UVs to the centre of each virtual pixel.
                float2 pixelUV =
                    (floor(input.texcoord * resolution) + 0.5)
                    / resolution;

                half4 color = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    pixelUV
                );

                return color;
            }

            ENDHLSL
        }
    }
}