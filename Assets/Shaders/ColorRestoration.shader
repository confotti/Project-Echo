Shader "Custom/ColorRestorationFullscreen"
{
    Properties //values som kan sättas i inspektorn
    {
        _RedThreshold ("Red Threshold", Range(0, 1)) = 0.2
        _RedTolerance ("Red Tolerance", Range(0, 1)) = 0.25
        _Contrast ("Grayscale Contrast", Range(0.5, 2)) = 1.5
    }

    SubShader
    {
        Tags //berättar att shadern är menad för URP 
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ColorRestoration"

            ZWrite Off //ifall den ska write to the depth buffer
            ZTest Always //alltid draw the fullscreen pass oberoende på the depth buffer 
            Cull Off //disable face culling för det är en fullscreen bild som renderas
            Blend Off //byter ut hela bilden istället för att blenda den 

            HLSLPROGRAM

            #pragma target 3.5 //Anger vilka GPU-funktioner som krävs för att köra shadern 
            #pragma vertex Vert //vertex function som låter dig rita ut bilden 
            #pragma fragment Frag //fragment låter dig rita ut the final color för varje pixel

			//ger dig Unity core shader functions and rendering definitions 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            //ger dig utilities to sample the fullscreen blit texture 
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			// tillåter dig att nå kamerans scene depth texture
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl" 

            float4 _ColorSource; //Pos av spelaren i world space, supplied by the controller 
            float _ColorRadius; //nuvarnade radius, animerad av the controller 
            float _EdgeWidth; //bredden på the transition mellan greyscale och color 
            float _ColorAmount; //

			//defined in the Properties Block 
            float _RedThreshold;
            float _RedTolerance;
            float _Contrast;

            float _EdgeIrregularity; //bestämmer the uneven borders 
            float _EdgeFrequency; //bestämmer the uneven borders 
            float4 _OutlineColor; //bestämmer color o alpha of the outline 
            float _OutlineWidth; //bestämmer bredden på the outline 

			//Fragment shader: runs på each pixel i fullscreen bild
			//sätter the final pixel color efter the restoration 
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
                float2 offset = worldPosition.xz - _ColorSource.xz;

                float distanceToPlayer = length(offset);
                float angle = atan2(offset.y, offset.x);

                float frequency = max(_EdgeFrequency, 1.0);

                float distortion =
                    sin(angle * frequency) * 0.50 +
                    sin(angle * (frequency * 2.1) + 1.7) * 0.30 +
                    sin(angle * (frequency * 0.55) + 3.2) * 0.20;

                float radius = max(_ColorRadius, 0.0);

                float distortedRadius = max(radius + distortion * _EdgeIrregularity,0.0);

                float edge = max(_EdgeWidth, 0.001);

                float colorMask = 0.0;

                if (radius > 0.001)
                {
                    colorMask = 1.0 - smoothstep(max(distortedRadius - edge, 0.0),distortedRadius + edge,distanceToPlayer);
                }

                float3 worldResult = lerp(grayWithRed,color,colorMask);

                float distanceToEdge = abs(distanceToPlayer - distortedRadius);

                float outlineWidth = max(_OutlineWidth, 0.001);
                float outlineMask = 1.0 - smoothstep(outlineWidth,outlineWidth + max(fwidth(distanceToEdge), 0.001),distanceToEdge);

                outlineMask *= step(0.001, radius);

				float3 surfaceNormal = normalize(SampleSceneNormals(uv));
				outlineMask *= step(0.7, surfaceNormal.y); 
                worldResult = lerp(worldResult,_OutlineColor.rgb,outlineMask * _OutlineColor.a);
                //return the final pixel color, preserve the original scene alpha 
				return half4(worldResult, sceneColor.a); 
            }

            ENDHLSL
        }
    }
}