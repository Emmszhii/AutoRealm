Shader "Unlit/MeshVisualization"
{
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha

        Tags {
            "Queue" = "AlphaTest"
            "RenderType" = "Transparent"
            }
        LOD 100

        Pass
        {
            ZTest LEqual
            ZWrite On
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                half3 worldNormal : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata input)
            {
                v2f output;

                UNITY_SETUP_INSTANCE_ID(input); //Insert
                UNITY_INITIALIZE_OUTPUT(v2f, output); //Insert
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); //Insert

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                return output;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input); //Insert

                fixed4 color;
                color.rgb = input.worldNormal * 0.5f + 0.5f;
                color.a = 0.5;
                return color;
            }
            ENDCG
        }
    }
}
