#ifndef TOON_OUTLINE_INCLUDED
#define TOON_OUTLINE_INCLUDED

CBUFFER_START(UnityPerMaterial)
float _OutlineThickness;
half _MinNormalThreshold;
half _NormalOutlineThickness;
half4 _OutlineColor;
half _StrokeGraininess;
half _LozengeTiling;
half _CrossHatchTiling;
half _CrossHatchThreshold;
half _CrossHatchAngleThreshold;
CBUFFER_END

TEXTURE2D(_NormalTexture);
TEXTURE2D(_IndexLightTexture);
TEXTURE2D(_RandomTexture);
TEXTURE3D(_LozengeTexture);
TEXTURE2D(_SSAOTexture);
TEXTURE2D(_CrossHatchTexture1);
TEXTURE2D(_CrossHatchTexture2);

const static int kThicknessMultiplier = 1000;

bool float4_equals(const float one, const float two, const float three, const float four, const float delta)
{
    return abs(one - two) < delta && abs(two - three) < delta && abs(three - four) < delta;
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////
half GetNormalDifference(const float2 uv, float2 uv_offset[4], float3 base_normal)
{
    if (_OutlineThickness == 0)
    {
        return 1.0;
    }

    _OutlineThickness *= kThicknessMultiplier;
    _NormalOutlineThickness *= kThicknessMultiplier;

    // Sample the index light texture to get the thick object outlines and lighting
    const half light_index1 = SAMPLE_TEXTURE2D_X(_IndexLightTexture, sampler_LinearClamp,  uv + uv_offset[0] * _OutlineThickness).x;
    const half light_index2 = SAMPLE_TEXTURE2D_X(_IndexLightTexture, sampler_LinearClamp,  uv + uv_offset[1] * _OutlineThickness).x;
    const half light_index3 = SAMPLE_TEXTURE2D_X(_IndexLightTexture, sampler_LinearClamp,  uv + uv_offset[2] * _OutlineThickness).x;
    const half light_index4 = SAMPLE_TEXTURE2D_X(_IndexLightTexture, sampler_LinearClamp,  uv + uv_offset[3] * _OutlineThickness).x;

    // Sample the normal texture to find edges where normal change is drastic (creases)
    const half3 normal_texture1 = SAMPLE_TEXTURE2D_X(_NormalTexture, sampler_LinearClamp,  uv + uv_offset[0] * _NormalOutlineThickness).xyz;
    const half3 normal_texture2 = SAMPLE_TEXTURE2D_X(_NormalTexture, sampler_LinearClamp,  uv + uv_offset[1] * _NormalOutlineThickness).xyz;
    const half3 normal_texture3 = SAMPLE_TEXTURE2D_X(_NormalTexture, sampler_LinearClamp,  uv + uv_offset[2] * _NormalOutlineThickness).xyz;
    const half3 normal_texture4 = SAMPLE_TEXTURE2D_X(_NormalTexture, sampler_LinearClamp,  uv + uv_offset[3] * _NormalOutlineThickness).xyz;

    float edges;

    // gather the normals
    const half3 normal1 = normal_texture1 * 2.0 - 1.0;
    const half3 normal2 = normal_texture2 * 2.0 - 1.0;
    const half3 normal3 = normal_texture3 * 2.0 - 1.0;
    const half3 normal4 = normal_texture4 * 2.0 - 1.0;

    const half3 normal_average = normalize(normal1 + normal2 + normal3 + normal4);
    const float normal_offset = abs(dot(normal_average, base_normal));
    
    const float normal_edges = step(_MinNormalThreshold, normal_offset);

    // test against normals for the internals of each object
    if (float4_equals(light_index1, light_index2, light_index3, light_index4, 0.0001))
    {
        edges = normal_edges;
    }
    else
    {
        // outline edges that separate objects
        edges = 0.0;
    }

    return edges;
}

half4 FragToonOutline(const Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    const float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

    // Sample offset kernel - test the four corners around the pixel with random thickness
    const half random = (SAMPLE_TEXTURE2D_X(_RandomTexture, sampler_LinearClamp, uv) - 0.5).x * _StrokeGraininess;
    const half3 base_light_index = SAMPLE_TEXTURE2D_X(_IndexLightTexture, sampler_LinearClamp,  uv).xyz;

    // use the index to offset the sampling for each object
    const half2 uv_offset = half2(base_light_index.x, base_light_index.x) * 10;
    const half3 adjusted_uv = half3(uv * _LozengeTiling + uv_offset, clamp(smoothstep(1.0, 0.0, base_light_index.y), 0.05, 0.95));

    const half4 lozenge_pattern = SAMPLE_TEXTURE3D(_LozengeTexture, sampler_LinearRepeat, adjusted_uv);

    const float2 pixel_size = float2(1.0 / _ScreenParams.x, 1.0 / _ScreenParams.y);
    float2 pixel_offset_kernel[4] =
    {
        float2(0.0, random) * pixel_size,
        float2(random, 0.0) * pixel_size,
        float2(0.0, -random) * pixel_size,
        float2(-random, 0.0) * pixel_size,
    };

    const half3 base_normal = SAMPLE_TEXTURE2D_X(_NormalTexture, sampler_LinearClamp,  uv).xyz * 2.0 - 1.0;
    const half stroke = saturate(GetNormalDifference(uv, pixel_offset_kernel, base_normal));

    // use the ssao texture and cross hatch textures to create AO effects
    const half ssao = SAMPLE_TEXTURE2D_X(_SSAOTexture, sampler_LinearClamp, uv).x;
    const half cross_hatch1 = step(_CrossHatchThreshold, lerp(SAMPLE_TEXTURE2D_X(_CrossHatchTexture1, sampler_LinearRepeat, uv * _CrossHatchTiling).x, 1.0, ssao));
    const half cross_hatch2 = step(_CrossHatchThreshold, lerp(SAMPLE_TEXTURE2D_X(_CrossHatchTexture2, sampler_LinearRepeat, uv * _CrossHatchTiling).x, 1.0, ssao));

    // choose between the two texture based on vertical angle
    const half normal_blend = step(abs(dot(base_normal, half3(0,1,0))), _CrossHatchAngleThreshold);
    const half cross_hatch = lerp(cross_hatch1, cross_hatch2, normal_blend);

    half4 color = lozenge_pattern * stroke * cross_hatch + _OutlineColor * (1 - lozenge_pattern * stroke * cross_hatch);
    
    return color;
}

#endif