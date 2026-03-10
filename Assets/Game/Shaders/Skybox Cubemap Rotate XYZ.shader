Shader "CodeCraft/Skybox/6 Sided Rotate XYZ"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
        _Exposure ("Exposure", Range(0, 8)) = 1.0

        _RotationX ("Rotation X", Range(-180,180)) = 0
        _RotationY ("Rotation Y", Range(-180,180)) = 0
        _RotationZ ("Rotation Z", Range(-180,180)) = 0

        _FrontTex ("Front [+Z] (HDR)", 2D) = "grey" {}
        _BackTex  ("Back [-Z] (HDR)", 2D)  = "grey" {}
        _LeftTex  ("Left [+X] (HDR)", 2D)  = "grey" {}
        _RightTex ("Right [-X] (HDR)", 2D) = "grey" {}
        _UpTex    ("Up [+Y] (HDR)", 2D)    = "grey" {}
        _DownTex  ("Down [-Y] (HDR)", 2D)  = "grey" {}
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Tint;
            half _Exposure;

            sampler2D _FrontTex, _BackTex, _LeftTex, _RightTex, _UpTex, _DownTex;

            float _RotationX, _RotationY, _RotationZ;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            float3 RotateX(float3 v, float a)
            {
                float s = sin(a), c = cos(a);
                return float3(v.x, c*v.y - s*v.z, s*v.y + c*v.z);
            }

            float3 RotateY(float3 v, float a)
            {
                float s = sin(a), c = cos(a);
                return float3(c*v.x + s*v.z, v.y, -s*v.x + c*v.z);
            }

            float3 RotateZ(float3 v, float a)
            {
                float s = sin(a), c = cos(a);
                return float3(c*v.x - s*v.y, s*v.x + c*v.y, v.z);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // направление луча в мировых координатах (как в стандартных skybox)
                float3 dir = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

                float rx = radians(_RotationX);
                float ry = radians(_RotationY);
                float rz = radians(_RotationZ);

                // порядок вращений — можно поменять при желании
                dir = RotateX(dir, rx);
                dir = RotateY(dir, ry);
                dir = RotateZ(dir, rz);

                o.dir = dir;
                return o;
            }

            inline fixed4 DecodeHDRSample(fixed4 data)
            {
                // Unity обычно упаковывает HDR через RGBM/движковую декодировку.
                // Для большинства HDR skybox-текстур этого достаточно: просто используем цвет.
                return data;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 d = normalize(i.dir);

                float3 ad = abs(d);
                fixed4 col;

                if (ad.z >= ad.x && ad.z >= ad.y)
                {
                    // Z major
                    float2 uv = (d.z > 0) ? float2( d.x,  d.y) : float2(-d.x,  d.y);
                    uv = uv / ad.z * 0.5 + 0.5;
                    col = (d.z > 0) ? tex2D(_FrontTex, uv) : tex2D(_BackTex, uv);
                }
                else if (ad.x >= ad.y)
                {
                    // X major
                    float2 uv = (d.x > 0) ? float2(-d.z,  d.y) : float2( d.z,  d.y);
                    uv = uv / ad.x * 0.5 + 0.5;
                    col = (d.x > 0) ? tex2D(_LeftTex, uv) : tex2D(_RightTex, uv);
                }
                else
                {
                    // Y major
                    float2 uv = (d.y > 0) ? float2( d.x, -d.z) : float2( d.x,  d.z);
                    uv = uv / ad.y * 0.5 + 0.5;
                    col = (d.y > 0) ? tex2D(_UpTex, uv) : tex2D(_DownTex, uv);
                }

                col = DecodeHDRSample(col);
                col.rgb = col.rgb * _Tint.rgb * _Exposure;
                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}
