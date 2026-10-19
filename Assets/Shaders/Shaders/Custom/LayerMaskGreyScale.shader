Shader "Oxysoft/LayerMaskedGrayscale"
{
    Properties
    {
        _MaskTexture ("Layer Mask", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "LayerMaskedGrayscale"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_MaskTexture);
            SAMPLER(sampler_MaskTexture);

            half4 Frag(Varyings input) : SV_Target
            {
                half4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                half4 mask = SAMPLE_TEXTURE2D_X(
                    _MaskTexture,
                    sampler_MaskTexture,
                    input.texcoord
                );

                half gray = dot(
                    source.rgb,
                    half3(0.299, 0.587, 0.114)
                );

                half3 grayscale = gray.xxx;

                // Mask alpha: 0 = grayscale, 1 = preserve color.
                half3 result = lerp(
					grayscale,
					source.rgb,
					saturate(mask.r)
				);

                return half4(result, source.a);
            }
            ENDHLSL
        }
    }
}