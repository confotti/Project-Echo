Shader "Oxysoft/Posterize"
{
    Properties
    {
        _Red ("Red Levels", Int) = 8
        _Green ("Green Levels", Int) = 8
        _Blue ("Blue Levels", Int) = 8
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "Posterize"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            int _Red;
            int _Green;
            int _Blue;

            half4 Frag(Varyings input) : SV_Target
            {
                half4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                float3 color = source.rgb;

                float3 levels = max(
                    float3(_Red, _Green, _Blue),
                    float3(2.0, 2.0, 2.0)
                );

                color = floor(color * levels) / levels;

                return half4(color, source.a);
            }

            ENDHLSL
        }
    }
}