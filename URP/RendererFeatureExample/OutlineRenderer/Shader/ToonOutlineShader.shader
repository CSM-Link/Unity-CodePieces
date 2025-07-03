Shader "Hidden/ToonOutline"
{
    Properties
    {
        _StrokeTexture("Stroke Texture", 2D) = "white" {}
        _PrepassTexture("Prepass Texture", 2D) = "white" {}

        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness("Outline Thickness", Range(0, 100)) = 4
        _OutlineThicknessDifference("Outline Thickness Difference", Range(0, 100)) = 2
        _MinThreshold("Edge Threshold", Range(0.0, 1.0)) = 0.25
        _StrokeGraininess("Stroke Graininess", Range(0.0, 1.0)) = 0.5
    }

    HLSLINCLUDE

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "ToonOutline"

            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment FragToonOutline

                #pragma enable_d3d11_debug_symbols

                #include "ToonOutlineShader.hlsl"

            ENDHLSL
        }
    }
}
