/*===============================================================================
Copyright (c) 2022 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/
Shader "PointClouds/PointSplatNoGeom"
{
    Properties {
        _PointSize("Point Size", Float) = 0.01
        _MinHeight("Min Height", Float) = -10
        _MaxHeight("Max Height", Float) = 10
        _Center("Center", Vector) = (0,0,0,1)
        _AxisY("Axis Y", Vector) = (0,1,0,0)
        [Toggle(USE_NORMALS)] _UseNormals("Use Normals", Float) = 0.0
    }

    SubShader
    {
        Tags {"Queue" = "Geometry-11" }
        Pass
        {
            Lighting Off
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define CUBE_DIAGONAL 1.73

            float _PointSize;
            float _UseNormals;

            float _MinHeight;
            float _MaxHeight;
            float3 _AxisY;
            float3 _Center;

            struct VertexInput
            {
                float4 pos : POSITION;
                half4 color : COLOR;
                half4 normal : NORMAL;
                half2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                half4 color : COLOR;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            void axial_delta(float3 p, float3 center, float3 axis, float scale, float min, float max, out float deltaMin, out float deltaMax)
            {
                float3 centerToPoint = p - center;
                float proj = dot(centerToPoint, normalize(axis)) / scale;
                deltaMin = proj - min;
                deltaMax = proj - max;
            }


            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                // Convert quad center position from object space to camera space
                float3 quadCenter = v.pos;
                float3 quadCenterViewPos = UnityObjectToViewPos(quadCenter);

                // Compute quad right and up dir (in camera space)
                float3 quadRight = float3(1.0, 0.0, 0.0);
                float3 quadUp = float3(0.0, 1.0, 0.0);

                // Compute displaced vertex (in camera space)
                const float splatSize = CUBE_DIAGONAL * _PointSize;
                float3 radialVec = 0.5 * splatSize * (quadRight * v.uv.x + quadUp * v.uv.y);

                // Compute vertex world position
                float3 worldPos = mul(unity_ObjectToWorld, v.pos).xyz;

                // Compute vertex position in camera spaces
                float3 viewPos = quadCenterViewPos + radialVec;

                o.pos = mul(UNITY_MATRIX_P, float4(viewPos.x, viewPos.y, viewPos.z, 1.0));
                o.color = v.color;
                o.worldPos = worldPos.xyz;
                o.worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal.xyz, 0.0)).xyz);
                return o;
            }

            float4 frag(VertexOutput v) : COLOR
            {
                float dy1, dy2;
                axial_delta(v.worldPos, _Center, _AxisY, 1.0, _MinHeight, _MaxHeight, dy1, dy2);
                clip(dy1);
                clip(-dy2);

                // Back face culling:
                if (_UseNormals > 0.5)
                {
                    float3 worldPointToCam = normalize(_WorldSpaceCameraPos.xyz - v.worldPos);
                    clip(dot(worldPointToCam, v.worldNormal));
                }

                return v.color;
            }


            ENDCG
        }
    }
}
