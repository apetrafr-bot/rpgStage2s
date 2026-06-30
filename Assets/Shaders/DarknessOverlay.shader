Shader "Custom/DarknessOverlay"
{
    Properties
    {
        _DarknessColor ("Darkness Color", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 worldPos : TEXCOORD1;
            };

            float4 _DarknessColor;
            float4 _PlayerPos;
            float _TorchRadius;
            float _TorchIntensity;
            int _LightCount;
            float4 _LightData[32];

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex).xy;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 pos = i.worldPos;
                float darkness = 1.0;

                float2 d = pos - _PlayerPos.xy;
                float dist = length(d);
                float light = 1.0 - saturate(dist / _TorchRadius);
                light = smoothstep(0, 1, light) * _TorchIntensity;
                darkness = min(darkness, 1.0 - light);

                for (int j = 0; j < _LightCount; j++)
                {
                    float2 ld = pos - _LightData[j].xy;
                    float ldist = length(ld);
                    float ll = 1.0 - saturate(ldist / max(_LightData[j].z, 0.01));
                    ll = smoothstep(0, 1, ll) * _LightData[j].w;
                    darkness = min(darkness, 1.0 - ll);
                }

                return float4(_DarknessColor.rgb, saturate(darkness) * _DarknessColor.a);
            }
            ENDHLSL
        }
    }
}
