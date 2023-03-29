Shader "Hidden/Thry/ChannelPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Channel ("Channel", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
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
            float _Channel;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                if(_Channel == -1.0) return fixed4(col.r, col.g, col.b, col.a);
                if(_Channel == 0.0) return fixed4(col.r, 0, 0, 1);
                if(_Channel == 1.0) return fixed4(0, col.g, 0, 1);
                if(_Channel == 2.0) return fixed4(0, 0, col.b, 1);
                if(_Channel == 3.0) return fixed4(col.a, col.a, col.a, 1);
                if(_Channel == 4.0)
                {
                    float val = max(col.r, max(col.g, col.b));
                    return fixed4(val, val, val, 1);
                }
                return fixed4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}
