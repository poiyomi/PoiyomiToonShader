Shader "Unlit/DecalGizmos"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Position("Position", Vector) = (0.5,0.5,0,0)
        _Rotation("Rotation", Float) = 0
        _Scale("Scale", Vector) = (1,1,0,0)
        _Offset("Scale Offset", Vector) = (0,0,0,0)
        _DecalTex("Decal Texture", 2D) = "white" {}
        _UVChannel("UV Channel", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            sampler2D _DecalTex;

            float4 _Position;
            float _Rotation;
            float4 _Scale;
            float4 _Offset;
            int _UVChannel;

            float2 remap(float2 x, float2 minOld, float2 maxOld, float2 minNew = 0, float2 maxNew = 1)
            {
                return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
            }

            float2 decalUV(float2 uv, float2 position, half rotation, half rotationSpeed, half2 scale, float4 scaleOffset, float depth)
            {
                scaleOffset = float4(-scaleOffset.x, scaleOffset.y, -scaleOffset.z, scaleOffset.w);
                float2 centerOffset = float2((scaleOffset.x + scaleOffset.y)/2, (scaleOffset.z + scaleOffset.w)/2);
                float2 decalCenter = position + centerOffset;
                float theta = radians(rotation + _Time.z * rotationSpeed);
                float cs = cos(theta);
                float sn = sin(theta);
                uv = float2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
                uv = remap(uv, float2(0, 0) - scale / 2 + position + scaleOffset.xz, scale / 2 + position + scaleOffset.yw, float2(0, 0), float2(1, 1));
                return uv;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = decalUV(v.uv0, _Position.xy, _Rotation, 0, _Scale.xy, _Offset, 0);
                return o;
            }

            float2 isBox(float2 uv, float2 p1, float2 p2, float2 width)
            {
                if (uv.x >= p1.x - width.x && uv.x <= p2.x + width.x &&
                    uv.y >= p1.y - width.y && uv.y <= p2.y + width.y)
                {
                    bool isInside = uv.x >= p1.x + width.x && uv.x <= p2.x - width.x &&
                        uv.y >= p1.y + width.y && uv.y <= p2.y - width.y;
                    return float2(1, !isInside ? 1 : 0);
                }
                return float2(0, 0);
            }

            #define LINE_WIDTH 0.003
            #define CORNER_BOX_SIZE 0.02
            #define EDGE_BOX_SIZE 0.01

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                col.a = 0;

                float2 aspectAdjust = float2(_Scale.x + _Offset.x + _Offset.y, _Scale.y + _Offset.z + _Offset.w);
                float2 smallBoxSizeScaled = float2(CORNER_BOX_SIZE / aspectAdjust.x, CORNER_BOX_SIZE / aspectAdjust.y);
                float2 edgeBoxSizeScaled = float2(EDGE_BOX_SIZE / aspectAdjust.x, EDGE_BOX_SIZE / aspectAdjust.y);
                float2 lineWidthScaled = float2(LINE_WIDTH / aspectAdjust.x, LINE_WIDTH / aspectAdjust.y);

                float2 isSmallBox = isBox(i.uv, -smallBoxSizeScaled + float2(0,0), smallBoxSizeScaled + float2(0,0), lineWidthScaled);
                isSmallBox += isBox(i.uv, -smallBoxSizeScaled + float2(0,1), smallBoxSizeScaled + float2(0,1), lineWidthScaled);
                isSmallBox += isBox(i.uv, -smallBoxSizeScaled + float2(1,0), smallBoxSizeScaled + float2(1,0), lineWidthScaled);
                isSmallBox += isBox(i.uv, -smallBoxSizeScaled + float2(1,1), smallBoxSizeScaled + float2(1,1), lineWidthScaled);

                float2 isEdgeBox = isBox(i.uv, -edgeBoxSizeScaled + float2(0.5,0), edgeBoxSizeScaled + float2(0.5,0), lineWidthScaled);
                isEdgeBox += isBox(i.uv, -edgeBoxSizeScaled + float2(0,0.5), edgeBoxSizeScaled + float2(0,0.5), lineWidthScaled);
                isEdgeBox += isBox(i.uv, -edgeBoxSizeScaled + float2(1,0.5), edgeBoxSizeScaled + float2(1,0.5), lineWidthScaled);
                isEdgeBox += isBox(i.uv, -edgeBoxSizeScaled + float2(0.5,1), edgeBoxSizeScaled + float2(0.5,1), lineWidthScaled);

                if (isSmallBox.y > 0)
                    col.a = 1;
                else if (isEdgeBox.x > 0)
                    col.a = 1;
                else if (isSmallBox.x == 0 && isBox(i.uv, float2(0, 0), float2(1, 1), lineWidthScaled).y)
                    col.a = 1;

        	    if(col.a == 0 && i.uv.x >= 0 && i.uv.x <= 1 && i.uv.y >= 0 && i.uv.y <= 1)
                    col = tex2D(_DecalTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
