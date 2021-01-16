#include "Planet.hlsl"

float4 _BedRockColor;
float4 _SoilColor;
			 
half4 FragmentProgram(FragmentData input) : SV_Target
{
    InputData inputData = InitializeInputData(input);

    float4 color = _BedRockColor;
    color = addTopographyLines(color, inputData.positionWS, inputData.normalWS);
    color = addFocusRing(color, inputData.positionWS);
    color = UniversalFragmentBlinnPhong(inputData, color.xyz, 0, 0, 0, 1);

    return color;
}