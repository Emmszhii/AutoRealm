//===============================================================================
//Copyright (c) 2015 PTC Inc. All Rights Reserved.
//
//Confidential and Proprietary - Protected under copyright and other laws.
//Vuforia is a trademark of PTC Inc., registered in the United States and other
//countries.
//===============================================================================
//===============================================================================
//Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
//All Rights Reserved.
//Confidential and Proprietary - Qualcomm Connected Experiences, Inc.
//===============================================================================

Shader "DepthMask" {
    SubShader {
        Tags { "Queue"="Geometry-10" "RenderType"="Opaque" }

        Pass {
            Cull Off
            ZTest LEqual
            ZWrite On
            Lighting Off
            ColorMask 0
        }
    }
}
