Shader "Oxysoft/RetroPixelMax"
{
    Properties
    {
        _ColorCount ("Color Count", Int) = 8
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "RetroPalette"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4 _Colors[256];
            int _ColorCount;

            half4 Frag(Varyings input) : SV_Target
            {
                float4 source = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                float4 closest = float4(0, 0, 0, 1);
                float bestDistance = 1e10;

                int count = min(_ColorCount, 256);

                for (int i = 0; i < count; i++)
                {
                    float4 candidate = _Colors[i];

                    float d = distance(
                        source.rgb,
                        candidate.rgb
                    );

                    if (d < bestDistance)
                    {
                        bestDistance = d;
                        closest = candidate;
                    }
                }

                return closest;
            }
            ENDHLSL
        }
    }
}