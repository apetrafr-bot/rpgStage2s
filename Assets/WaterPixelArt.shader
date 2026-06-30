Shader "Custom/WaterPixelArt"
{
    Properties
    {
        _WaterColorA ("Couleur eau (foncé)",  Color) = (0.04, 0.22, 0.52, 1)
        _WaterColorB ("Couleur eau (clair)",  Color) = (0.10, 0.48, 0.82, 1)
        _ShoreColor  ("Couleur bord (clair)", Color) = (0.45, 0.75, 1.00, 1)
        _WaveSpeed   ("Vitesse vagues",       Float) = 1.2
        _PixelSize   ("Taille pixel",         Float) = 20.0
        _WaveScale   ("Échelle vagues",       Float) = 8.0
        _ShoreWidth  ("Largeur bord eau",     Float) = 0.06
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "WaterPixelArt"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColorA;
                float4 _WaterColorB;
                float4 _ShoreColor;
                float  _WaveSpeed;
                float  _PixelSize;
                float  _WaveScale;
                float  _ShoreWidth;
            CBUFFER_END

            struct Attributes { float4 pos : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings vert(Attributes IN)
            {
                Varyings O;
                O.pos = TransformObjectToHClip(IN.pos.xyz);
                O.uv  = IN.uv;
                return O;
            }

            // Pixelisation sur grille
            float2 pixSnap(float2 uv, float n)
            {
                return floor(uv * n) / n;
            }

            // Hash 2D → [0,1]
            float hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            // Bruit de valeur lisse
            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash21(i),               hash21(i + float2(1,0)), u.x),
                    lerp(hash21(i + float2(0,1)), hash21(i + float2(1,1)), u.x),
                    u.y);
            }

            // Une mini-vague : crête pixel art nette dans une direction donnée
            // dir = direction (normalisée), speed = vitesse propre, phase = décalage
            float miniWave(float2 puv, float2 dir, float speed, float phase, float scale, float t)
            {
                float proj   = dot(puv * scale, dir);           // projection sur la direction
                float raw    = sin(proj + t * speed + phase);   // sinusoïde
                float snapped = floor(raw * 3.0) / 3.0;         // quantification → crêtes nettes
                return snapped * 0.5 + 0.5;                     // → [0,1]
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y * _WaveSpeed;
                float  s  = _WaveScale;

                // Pixelisation
                float2 puv = pixSnap(uv, _PixelSize);

                // ── 8 mini-vagues dans des directions aléatoires ──────
                // dir, speed, phase — toutes différentes
                float w0 = miniWave(puv, normalize(float2( 1.00,  0.00)), 1.00, 0.00, s,        t);
                float w1 = miniWave(puv, normalize(float2( 0.80,  0.60)), 1.30, 1.10, s * 1.3,  t);
                float w2 = miniWave(puv, normalize(float2(-0.60,  0.80)), 0.90, 2.40, s * 0.8,  t);
                float w3 = miniWave(puv, normalize(float2( 0.40, -1.00)), 1.60, 0.70, s * 1.6,  t);
                float w4 = miniWave(puv, normalize(float2(-1.00,  0.20)), 0.70, 3.50, s * 0.9,  t);
                float w5 = miniWave(puv, normalize(float2( 0.70,  0.70)), 1.10, 1.80, s * 1.1,  t);
                float w6 = miniWave(puv, normalize(float2(-0.30, -0.95)), 1.40, 4.20, s * 1.4,  t);
                float w7 = miniWave(puv, normalize(float2( 0.95, -0.30)), 0.80, 2.90, s * 0.75, t);

                // Moyenne pondérée des 8 vagues
                float wave = w0 * 0.18 + w1 * 0.16 + w2 * 0.14 + w3 * 0.12
                           + w4 * 0.12 + w5 * 0.11 + w6 * 0.10 + w7 * 0.07;

                // Bruit de surface fin (scroll dans deux directions opposées)
                float2 sA = puv * s * 0.4 + float2( t * 0.04,  t * 0.02);
                float2 sB = puv * s * 0.7 + float2(-t * 0.02,  t * 0.05);
                float  noise = vnoise(sA) * 0.55 + vnoise(sB) * 0.45;

                // Valeur finale
                float waterVal = wave * 0.65 + noise * 0.35;

                // Couleur
                float4 col = lerp(_WaterColorA, _WaterColorB, waterVal);

                // ── Bordure de transition eau/sable ───────────────────
                // Détecte la proximité des 4 bords du sprite (UV 0 et 1)
                // et applique une couleur claire animée qui simule la mousse de rivage
                float bL = uv.x;
                float bR = 1.0 - uv.x;
                float bB = uv.y;
                float bT = 1.0 - uv.y;
                float edgeDist = min(min(bL, bR), min(bB, bT)); // distance au bord le plus proche

                // Ondulation de la bordure : elle n'est pas uniforme
                float shoreWave = sin(uv.x * s * 2.0 + t * 0.8) * 0.015
                                + sin(uv.y * s * 2.0 - t * 0.6) * 0.015;
                float shoreEdge = _ShoreWidth + shoreWave;

                // Pixelisation du bord
                float edgeSnap = floor(edgeDist * _PixelSize) / _PixelSize;

                float shoreMask = 1.0 - smoothstep(0.0, shoreEdge, edgeSnap);

                // Scintillement sur la bordure
                float2 shoreHash = floor(float2(uv.x * _PixelSize, uv.y * _PixelSize)
                                 + float2(t * 1.5, t * 0.8));
                float shimmer = step(0.78, hash21(shoreHash)) * step(0.5, sin(t * 6.0 + hash21(shoreHash) * 6.28));

                col = lerp(col, _ShoreColor, shoreMask * 0.80);
                col = lerp(col, float4(1,1,1,1), shoreMask * shimmer * 0.5);

                col.a = 1.0;
                return half4(col);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
