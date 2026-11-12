// =====================================================================
//  UFQ/Vidrio — vidrio de ventana estilizado (URP 17)
//  Transparente con un tinte leve y un reflejo de borde (fresnel), para que
//  se vea el exterior (el Misti) sin perder la sensación de cristal.
// =====================================================================
Shader "UFQ/Vidrio"
{
    Properties
    {
        [MainColor] _BaseColor ("Tinte", Color) = (0.62, 0.72, 0.78, 0.12)
        _EdgeColor ("Color del reflejo", Color) = (0.85, 0.92, 1.0, 0.55)
        _EdgePower ("Ancho del reflejo", Range(0.5, 8)) = 3
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _EdgeColor;
                float _EdgePower;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                o.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return o;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 n = normalize(input.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(input.positionWS));
                float edge = pow(1.0 - abs(dot(n, v)), _EdgePower);
                float3 color = lerp(_BaseColor.rgb, _EdgeColor.rgb, edge);
                float alpha = saturate(_BaseColor.a + edge * _EdgeColor.a);
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }
}
