#ifndef RayTrace_INCLUDE
#define RayTrace_INCLUDE
#include "../lit.hlsl"
#include "UnityRayTracingMeshUtils.cginc"
struct Hit
{
    int instanceID;
    uint primitiveIndex;
    float2 uvBarycentrics;
    float hitDistance;
    bool isFrontFace;
    float3 position;
    float3 normal;
    float4 baseColor;

    bool IsValid()
    {
        return instanceID != -1;
    }

};
struct IntersectionVertex
{
    // Object space normal of the vertex
    float3 normalOS;
    // Object space tangent of the vertex
    float4 tangentOS;
    // UV coordinates
    float4 texCoord0;
};
// Fetch the intersetion vertex data for the target vertex
void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex) {
    outVertex.normalOS   = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
    outVertex.tangentOS  = UnityRayTracingFetchVertexAttribute4(vertexIndex, kVertexAttributeTangent);
    outVertex.texCoord0  = UnityRayTracingFetchVertexAttribute4(vertexIndex, kVertexAttributeTexCoord0);

}
#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)
void GetCurrentIntersectionVertex(float2 barycentrics, out IntersectionVertex outVertex) {
    // Fetch the indices of the currentr triangle
    uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

    // Fetch the 3 vertices
    IntersectionVertex v0, v1, v2;
    FetchIntersectionVertex(triangleIndices.x, v0);
    FetchIntersectionVertex(triangleIndices.y, v1);
    FetchIntersectionVertex(triangleIndices.z, v2);

    // Compute the full barycentric coordinates
    float3 barycentricCoordinates = float3(1.0 - barycentrics.x - barycentrics.y, barycentrics.x, barycentrics.y);

    // Interpolate all the data
    outVertex.normalOS   = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
    outVertex.tangentOS  = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.tangentOS, v1.tangentOS, v2.tangentOS, barycentricCoordinates);
    outVertex.texCoord0  = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.texCoord0, v1.texCoord0, v2.texCoord0, barycentricCoordinates);
}

#endif