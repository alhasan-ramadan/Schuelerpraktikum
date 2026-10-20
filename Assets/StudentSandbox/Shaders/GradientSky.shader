// Einfacher, stilisierter Himmel für die Spielwiese:
// Farbverlauf (oben / Horizont / unten), ein weicher Sonnenglanz und optional Sterne.
// Die Sonnenrichtung kommt automatisch vom Haupt-Licht der Universal Render Pipeline.
Shader "StudentSandbox/GradientSky"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.30, 0.58, 0.88, 1)
        _HorizonColor ("Horizon Color", Color) = (0.78, 0.89, 0.97, 1)
        _BottomColor ("Bottom Color", Color) = (0.55, 0.75, 0.88, 1)
        _HorizonSoftness ("Horizon Softness", Range(0.05, 1)) = 0.45
        _SunColor ("Sun Color", Color) = (1, 0.92, 0.75, 1)
        _SunGlow ("Sun Glow", Range(0, 2)) = 0.5
        _SunSize ("Sun Size", Range(0, 0.1)) = 0.03
        _StarDensity ("Stars", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopColor;
            fixed4 _HorizonColor;
            fixed4 _BottomColor;
            half _HorizonSoftness;
            fixed4 _SunColor;
            half _SunGlow;
            half _SunSize;
            half _StarDensity;
            // Wird von URP für das Haupt-Licht (die Sonne) gesetzt: Richtung zum Licht.
            float4 _MainLightPosition;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            float Hash(float3 p)
            {
                return frac(sin(dot(p, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                float y = dir.y;

                float up = pow(saturate(y / _HorizonSoftness), 0.6);
                float down = pow(saturate(-y / _HorizonSoftness), 0.6);
                float3 col = lerp(_HorizonColor.rgb, _TopColor.rgb, saturate(up));
                col = lerp(col, _BottomColor.rgb, saturate(down));

                float3 sunDir = _MainLightPosition.xyz;
                if (dot(sunDir, sunDir) > 0.0001)
                {
                    sunDir = normalize(sunDir);
                    float d = saturate(dot(dir, sunDir));
                    float glow = pow(d, 24.0) * _SunGlow;
                    float disc = smoothstep(1.0 - _SunSize * 0.02, 1.0 - _SunSize * 0.012, d);
                    col += _SunColor.rgb * (glow + disc * 1.5) * step(-0.05, y);
                }

                if (_StarDensity > 0.001 && y > 0.0)
                {
                    float3 cell = floor(dir * 260.0);
                    float h = Hash(cell);
                    float star = step(1.0 - 0.004 * _StarDensity, h);
                    float twinkle = 0.6 + 0.4 * Hash(cell + 7.0);
                    col += star * twinkle * saturate(y * 3.0) * _StarDensity;
                }

                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}
