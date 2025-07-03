
void GetLightData_float(float3 ObjectPos, out float3 Direction, out float3 Color, out float ShadowAtten, out float DistanceAtten)
{
#ifdef SHADERGRAPH_PREVIEW
	Direction = float3(0.5, 0.5, 0.0);
	Color = float3(1.0, 1.0, 1.0);
	ShadowAtten = 0.0;
	DistanceAtten = 0.0;
#else
	VertexPositionInputs vertexInput = GetVertexPositionInputs(ObjectPos);
	float4 shadowCoord = GetShadowCoord(vertexInput);
	Light light = GetMainLight(shadowCoord);

	Direction = light.direction;
	Color = light.color;
	DistanceAtten = light.distanceAttenuation;
	ShadowAtten = light.shadowAttenuation;
#endif
}