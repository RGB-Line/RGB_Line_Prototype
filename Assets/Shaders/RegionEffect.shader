Shader "RGBLine/RegionEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MousePos ("MousePos", Color) = (1,1,1,1)
        _MouseClicked ("MouseClicked", int) = 0
        _BaseColor ("BaseColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            fixed4 _MousePos;
            int _MouseClicked;

            fixed4 _MainTex_TexelSize;

            fixed4 _BaseColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mx = max(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                fixed2 uv = i.uv.xy / mx;
                fixed3 color = fixed3(uv, 0.25f + 0.5f * sin(_Time.y));

                if(_MouseClicked == 1)
                {
                    float3 holeEle = sin(1.5f - distance(uv, _MousePos.xy / mx) * 8.0f);
                    color = lerp(color, holeEle, -0.5f);
                }

                color = lerp(color, _BaseColor, 0.5f);

                return fixed4(color, 1.0f);
            }
            ENDCG
        }
    }
}
