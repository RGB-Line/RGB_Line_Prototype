Shader "RGBLine/RegionFadeout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // _EffectStartPos ("DeterminLinePos", Color) = (1,1,1,1)
        _EffectStartPos_X ("EffectStartPos_X", RAnge(0, 1)) = 0
        _EffectStartPos_Y ("EffectStartPos_Y", RAnge(0, 1)) = 0

        _Radius ("Radius", Range(0, 10000)) = 0.0

        _PrevBaseColor ("PrevBaseColor", Color) = (1,1,1,1)
        _BaseColor ("BaseColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _MainTex_TexelSize;

            // fixed4 _EffectStartPos;
            fixed _EffectStartPos_X;
            fixed _EffectStartPos_Y;
            half _Radius;

            fixed4 _PrevBaseColor;
            fixed4 _BaseColor;

            float getMetaball(fixed2 a, fixed2 b);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Transition Shape
                fixed2 startPos = fixed2(_EffectStartPos_X, _EffectStartPos_Y);
                float metaballValue = saturate(getMetaball(i.uv, startPos) / _Radius);

                fixed2 startPos2uv = i.uv - startPos;

                float cosValue = abs(dot(fixed2(1.0f, 0.0f), normalize(startPos2uv)));
                float sinValue = abs(sqrt(1.0f - pow(cosValue, 2.0f)));

                // By adjusting the constants added to each, the final shape changes.
                // 1.5 - 애매한 마름도
                // 1.2 - 프랑스 왕실 문장(백합) 느낌
                // 1.0 - 날렵한 십자가
                cosValue = -pow(cosValue, 0.5f) + 1.5f;
                sinValue = -pow(sinValue, 0.5f) + 1.5f;

                metaballValue *= max(cosValue, sinValue);

                // Background Color Mix
                float mx = max(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                fixed2 uv = i.uv / mx;
                fixed3 color = fixed3(uv, 0.25f + 0.5f * sin(_Time.y));
                
                fixed3 prevColor = lerp(color, _PrevBaseColor, 0.5f);
                fixed3 newColor = lerp(color, _BaseColor, 0.5f);

                // Output
                fixed4 output = fixed4(1,1,1,1);
                if(metaballValue >= 0.5f)
                {
                    output = fixed4(newColor, 1.0f);
                }
                else
                {
                    output = fixed4(prevColor, 0.0f);
                }

                return output;
            }

            float getMetaball(fixed2 a, fixed2 b)
            {
                return 1.0f / (pow(b.x - a.x, 2.0f) + pow(b.y - a.y, 2.0f));
            }
            ENDCG
        }
    }
}