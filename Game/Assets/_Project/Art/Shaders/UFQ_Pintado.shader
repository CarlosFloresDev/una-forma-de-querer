// =====================================================================
//  UFQ/Pintado — sombreado pictórico de «Una forma de querer» (URP 17)
//
//  · Luz en bandas suaves (no fotorrealista): la superficie pasa de luz a
//    sombra en pocos tonos, como pintura de acuarela/gouache.
//  · La sombra NO es gris: se tiñe con _ShadowTint (cálido en Arequipa,
//    azulado en Lima…), lo que da el aire de libro ilustrado.
//  · Ruido de «pincel» en espacio de mundo: varía levemente el color para
//    que las paredes de sillar no se vean planas ni de plástico.
//  · Luz de borde (rim) suave para separar personajes del fondo.
//  · Brillo dorado opcional (_GlowColor/_GlowStrength) para las SEÑALES.
//
//  Pases: UniversalForward, ShadowCaster, DepthOnly, DepthNormals
//  (los dos últimos los necesita la profundidad de campo y el SSAO).
// =====================================================================
Shader "UFQ/Pintado"
{
    Properties
    {
        [MainColor] _BaseColor ("Color base", Color) = (0.93, 0.90, 0.85, 1)
        [MainTexture] _BaseMap ("Textura (opcional)", 2D) = "white" {}

        [Header(Luz en bandas)]
        _Bands ("Número de bandas", Range(1, 6)) = 3
        _BandSoftness ("Suavidad entre bandas", Range(0.001, 0.5)) = 0.18
        _ShadowTint ("Tinte de la sombra", Color) = (0.62, 0.48, 0.52, 1)
        _ShadowStrength ("Fuerza de la sombra", Range(0, 1)) = 0.75

        [Header(Pincel)]
        _BrushScale ("Escala del pincel", Range(0.1, 20)) = 3.5
        _BrushStrength ("Variación del pincel", Range(0, 0.4)) = 0.08

        [Header(Borde)]
        _RimColor ("Color de borde", Color) = (1, 0.92, 0.80, 1)
        _RimPower ("Ancho del borde", Range(0.5, 8)) = 3.5
        _RimStrength ("Fuerza del borde", Range(0, 1)) = 0.25

        [Header(Senal dorada)]
        [HDR] _GlowColor ("Color de brillo", Color) = (1.6, 1.1, 0.35, 1)
        _GlowStrength ("Brillo (0 = apagado)", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _BaseMap_ST;
            float _Bands;
            float _BandSoftness;
            float4 _ShadowTint;
            float _ShadowStrength;
            float _BrushScale;
            float _BrushStrength;
            float4 _RimColor;
            float _RimPower;
            float _RimStrength;
            float4 _GlowColor;
            float _GlowStrength;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float  fogFactor  : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // Ruido de valor 3D barato: suficiente para «manchas de pincel».
            float Hash31(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }

            float ValueNoise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float n000 = Hash31(i);
                float n100 = Hash31(i + float3(1, 0, 0));
                float n010 = Hash31(i + float3(0, 1, 0));
                float n110 = Hash31(i + float3(1, 1, 0));
                float n001 = Hash31(i + float3(0, 0, 1));
                float n101 = Hash31(i + float3(1, 0, 1));
                float n011 = Hash31(i + float3(0, 1, 1));
                float n111 = Hash31(i + float3(1, 1, 1));
                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);
                return lerp(lerp(nx00, nx10, f.y), lerp(nx01, nx11, f.y), f.z);
            }

            // Cuantiza 0..1 en bandas con bordes suaves.
            float Banded(float x)
            {
                float bands = max(1.0, _Bands);
                float scaled = saturate(x) * bands;
                float baseBand = floor(scaled);
                float t = smoothstep(0.5 - _BandSoftness, 0.5 + _BandSoftness, frac(scaled));
                return saturate((baseBand + t) / bands);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(input.normalOS);
                output.positionCS = pos.positionCS;
                output.positionWS = pos.positionWS;
                output.normalWS = nrm.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float3 n = normalize(input.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(input.positionWS));

                float3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;

                // Pincel: dos octavas, centrado en 0 para no oscurecer en promedio.
                float brush = ValueNoise(input.positionWS * _BrushScale) * 0.65
                            + ValueNoise(input.positionWS * _BrushScale * 2.7) * 0.35;
                albedo *= 1.0 + (brush - 0.5) * 2.0 * _BrushStrength;

                // Luz principal con sombras.
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                float ndl = dot(n, mainLight.direction) * 0.5 + 0.5;              // half-lambert
                float lit = Banded(ndl * mainLight.shadowAttenuation * mainLight.distanceAttenuation);

                // Sombra teñida: mezcla entre el color en sombra y el iluminado.
                float3 shadowColor = albedo * lerp(1.0.xxx, _ShadowTint.rgb, _ShadowStrength);
                float3 color = lerp(shadowColor, albedo * mainLight.color, lit);

                // Luces adicionales (lámparas, faroles), también en bandas.
                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = n;
                inputData.viewDirectionWS = v;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                LIGHT_LOOP_BEGIN(count)
                    Light l = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    float a = Banded(saturate(dot(n, l.direction)) * l.distanceAttenuation * l.shadowAttenuation);
                    color += albedo * l.color * a;
                LIGHT_LOOP_END
                #endif

                // Ambiente (cielo/sonda) para que las sombras no queden muertas.
                color += albedo * SampleSH(n) * 0.35;

                // Luz de borde.
                float rim = pow(1.0 - saturate(dot(n, v)), _RimPower) * _RimStrength;
                color += _RimColor.rgb * rim * lit;

                // Brillo dorado de las señales.
                color += _GlowColor.rgb * _GlowStrength;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings ShadowVert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDir = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDir = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionCS = positionCS;
                return o;
            }

            half4 ShadowFrag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings DepthVert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return o;
            }

            half DepthFrag(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex DNVert
            #pragma fragment DNFrag
            #pragma multi_compile_instancing

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };

            Varyings DNVert(Attributes input)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return o;
            }

            half4 DNFrag(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
