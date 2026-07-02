Shader "Custom/BillBoard"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [Enum(Spherical, 0, CylinDrical, 1)]_BillboardMode ("BillboardMode", int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color: COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _BillboardMode;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 upCam = lerp(UNITY_MATRIX_V._m10_m11_m12, float3(0, 1, 0), _BillboardMode);
                float4x4 newRotation = float4x4(
                    UNITY_MATRIX_V._m00_m01_m02, 0,
                    upCam, 0,
                    -1 * UNITY_MATRIX_V._m20_m21_m22, 0,
                    0, 0, 0, 1
                );

                float4 newPosition = IN.positionOS;

                newPosition.x *= length(UNITY_MATRIX_M._m00_m10_m20);
                newPosition.y *= length(UNITY_MATRIX_M._m01_m11_m21);
                newPosition.z *= length(UNITY_MATRIX_M._m02_m12_m22);


                newPosition = (
                    newPosition.x * newRotation._m00_m01_m02_m03 +
                    newPosition.y * newRotation._m10_m11_m12_m13 +
                    newPosition.z * newRotation._m20_m21_m22_m23 +
                    newPosition.w * newRotation._m30_m31_m32_m33
                );

                newPosition.xyz += UNITY_MATRIX_M._m03_m13_m23;

                OUT.positionHCS = TransformWorldToHClip(newPosition.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                return color * IN.color;
            }
            ENDHLSL
        }
    }
}
