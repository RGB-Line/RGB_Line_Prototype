Shader "RGBLine/DashLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Interval ("Interval", Range(0, 1000)) = 500
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

                float scaledSize : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _MainTex_TexelSize;

            float _Interval;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.scaledSize = int(ceil(_MainTex_TexelSize.x * _Interval));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float hash = float(0.0);
                if(int(i.uv.x * i.scaledSize) % 2 == 0)
                {
                    hash = 1.0;
                }

                return fixed4(hash, hash, hash, hash);
            }
            ENDCG
        }
    }
}
