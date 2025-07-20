Shader "Custom/IslandHeightColor"
{
    Properties
    {
        _LowColor ("Low Color", Color) = (0.2, 0.8, 0.3, 1)
        _MidColor ("Mid Color", Color) = (0.7, 0.7, 0.4, 1)
        _HighColor ("High Color", Color) = (0.6, 0.6, 0.6, 1)
        _TopColor ("Top Color", Color) = (1, 1, 1, 1)
        _LowY ("Low Height", Float) = 0
        _MidY ("Mid Height", Float) = 2
        _HighY ("High Height", Float) = 5
        _TopY ("Top Height", Float) = 8
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
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _LowColor, _MidColor, _HighColor, _TopColor;
            float _LowY, _MidY, _HighY, _TopY;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float y = i.worldPos.y;
                fixed4 color;

                // Мягкая интерполяция между зонами
                if (y < _MidY)
                    color = lerp(_LowColor, _MidColor, saturate((y-_LowY)/max(0.01,_MidY-_LowY)));
                else if (y < _HighY)
                    color = lerp(_MidColor, _HighColor, saturate((y-_MidY)/max(0.01,_HighY-_MidY)));
                else if (y < _TopY)
                    color = lerp(_HighColor, _TopColor, saturate((y-_HighY)/max(0.01,_TopY-_HighY)));
                else
                    color = _TopColor;

                return color;
            }
            ENDCG
        }
    }
}
