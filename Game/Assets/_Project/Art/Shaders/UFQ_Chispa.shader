// =====================================================================
//  UFQ/Chispa — partículas luminosas aditivas (fuegos artificiales, polvo
//  dorado de las señales). Usa el color de vértice del sistema de partículas
//  y un disco suave procedural: no necesita textura.
// =====================================================================
Shader "UFQ/Chispa"
{
    Properties
    {
        _Intensity ("Intensidad", Range(0, 8)) = 2.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.color = input.color;
                o.uv = input.uv;
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float d = length(input.uv - 0.5) * 2.0;
                float disc = saturate(1.0 - d);
                disc *= disc;
                return half4(input.color.rgb * _Intensity, input.color.a * disc);
            }
            ENDHLSL
        }
    }
}
