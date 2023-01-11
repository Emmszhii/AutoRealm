/*========================================================================
Copyright (c) 2022 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
=========================================================================*/
Shader "Custom/UnlitTextureHeightClip" {
    Properties {
        _MainTex ("Base (RGBA)", 2D) = "white" {}
        _MinHeight("Min Height", Float) = -10
        _MaxHeight("Max Height", Float) = 10
        _Center("Center", Vector) = (0,0,0,1)
        _AxisY("Axis Y", Vector) = (0,1,0,0)
    }

    SubShader {
        Tags  { "Queue"="Geometry-11" "RenderType"="Opaque" }

        Pass {
            ZWrite On
            Cull Back
            Lighting Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MinHeight;
            float _MaxHeight;
            float3 _AxisY;
            float3 _Center;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            void axial_delta(float3 p, float3 center, float3 axis, float scale, float min, float max, out float deltaMin, out float deltaMax)
            {
                float3 centerToPoint = p - center;
                float proj = dot(centerToPoint, normalize(axis)) / scale;
                deltaMin = proj - min;
                deltaMax = proj - max;
            }

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos (v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                float dy1, dy2;
                axial_delta(i.worldPos, _Center, _AxisY, 1.0, _MinHeight, _MaxHeight, dy1, dy2); 
                clip(dy1);
                clip(-dy2);

                half4 color = tex2D(_MainTex, i.uv);
                return color;
            }

            ENDCG
        }
    }

    FallBack "Diffuse"
}
