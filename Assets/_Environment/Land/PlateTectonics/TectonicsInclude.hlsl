#include "../../../Common/Shaders/Includes/CoordinateTransforms.hlsl"
#include "../../../Common/Shaders/Includes/Quaternion.hlsl"
#include "../../../Common/Shaders/Includes/Noise.hlsl"

struct Plate
{
    float Id;
    int Idx;
    float4 Rotation;
}; 
RWStructuredBuffer<Plate> Plates;
uint NumPlates;
float MantleHeight;

uint3 xyz_to_xyp(float3 xyz, int p) {
    uint3 xyw = xyz_to_xyw(rotate_vector(xyz, q_inverse(Plates[p].Rotation)));
    return uint3(xyw.xy, xyw.z + (p * 6));
}
uint3 xyw_to_xyp(uint3 xyw, int p)
{
    return xyz_to_xyp(xyw_to_xyz(xyw, MantleHeight), p);
}
float3 xyp_to_xyz(uint3 xyp, int p) {

    uint3 xyw = uint3(xyp.xy, xyp.z % 6);
    return rotate_vector(xyw_to_xyz(xyw, MantleHeight), Plates[p].Rotation);
}
uint GetPlateIdx(float id) 
{
    uint pId = 0;
    for (uint x = 0; x < NumPlates; x++)
    {
        if (Plates[x].Id == id)
        {
            pId = Plates[x].Idx;
        }
    }
    return pId;
}