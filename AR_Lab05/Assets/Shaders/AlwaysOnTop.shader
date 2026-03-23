Shader "Custom/AlwaysOnTopLine" {
    Properties {
        _Color ("Line Color", Color) = (0, 1, 0, 1) // Зелений за замовчуванням
    }
    SubShader {
        // Кажемо малювати це в самому кінці (Overlay), поверх усього
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 100
        
        // ГОЛОВНА МАГІЯ: Ігнорувати глибину!
        ZTest Always 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                return _Color;
            }
            ENDCG
        }
    }
}