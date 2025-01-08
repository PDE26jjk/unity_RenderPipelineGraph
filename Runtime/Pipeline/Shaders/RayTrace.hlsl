#ifndef RayTrace_INCLUDE
#define RayTrace_INCLUDE

struct Hit
{
    int instanceID;
    uint primitiveIndex;
    float2 uvBarycentrics;
    float hitDistance;
    bool isFrontFace;

    bool IsValid()
    {
        return instanceID != -1;
    }

};

#endif