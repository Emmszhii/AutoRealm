#ifndef VUFORIA_VB_YUV420P_YUVYV12_INCLUDED
#define VUFORIA_VB_YUV420P_YUVYV12_INCLUDED

struct v2f {
    float4  pos : SV_POSITION;
    float2  uv  : TEXCOORD0;
    float2  uv2 : TEXCOORD1;
    float2  uv3 : TEXCOORD2;
};

sampler2D _MainTex;
float4 _MainTex_ST;
sampler2D _UVTex1;
float4 _UVTex1_ST;
sampler2D _UVTex2;
float4 _UVTex2_ST;

v2f vuforiaConvertRGBVert(appdata_base v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv  = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.uv2 = TRANSFORM_TEX(v.texcoord, _UVTex1);
    o.uv3 = TRANSFORM_TEX(v.texcoord, _UVTex2);

    return o;
}
/*===============================================================================
Copyright 2019 PTC Inc.

Licensed under the Apache License, Version 2.0 (the "License"); you may not
use this file except in compliance with the License. You may obtain a copy of
the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed
under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied. See the License for the
specific language governing permissions and limitations under the License.
===============================================================================*/

half4 vuforiaConvertRGBFrag(v2f i) : COLOR
{
    // Color conversion from YUV to RGB (ITU.BT-601):
    // R = Y                       + 1.40200 * (V - 0.5)
    // G = Y - 0.34414 * (U - 0.5) - 0.71414 * (V - 0.5)
    // B = Y + 1.77200 * (U - 0.5)
    //
    // Last column of the transformation matrix is computed as: -0.5 * (C_U + C_Y)    
    
    half4 c;
    half y = tex2D(_MainTex, i.uv ).r;
#if VUFORIA_YUV420P
    half u = tex2D(_UVTex1, i.uv2).r;
    half v = tex2D(_UVTex2, i.uv3).r;
#else
    half v = tex2D(_UVTex1, i.uv2).r;
    half u = tex2D(_UVTex2, i.uv3).r;
#endif
			
    half4 v4yuv1 = half4(y, u, v, 1.0);

    c.r = dot(half4(1.00000,  0.00000,  1.40200, -0.70100), v4yuv1);
    c.g = dot(half4(1.00000, -0.34414, -0.71414,  0.52914), v4yuv1);
    c.b = dot(half4(1.00000,  1.77200,  0.00000, -0.88600), v4yuv1);
    c.a = 1.0;

    return c;
}

#endif //VUFORIA_VB_YUV420P_YUVYV12_INCLUDED