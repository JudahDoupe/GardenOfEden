#ifndef RAYSPHERE_INCLUDED
#define RAYSPHERE_INCLUDED

inline float2 RaySphere(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir)
{
	float3 offset = rayOrigin - sphereCenter;
	float a = 1; // Set to dot(rayDir, rayDir) if rayDir might not be normalized
	float b = 2 * dot(offset, rayDir);
	float c = dot (offset, offset) - sphereRadius * sphereRadius;
	float d = b * b - 4 * a * c; // Discriminant from quadratic formula

	// Number of intersections: 0 when d < 0; 1 when d = 0; 2 when d > 0
	if (d > 0) {
		float s = sqrt(d);
		float dstToSphereNear = max(0, (-b - s) / (2 * a));
		float dstToSphereFar = (-b + s) / (2 * a);

		// Ignore intersections that occur behind the ray
		if (dstToSphereFar >= 0) {
			return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
		}
	}

	return float2(3.402823466e+38, 0);
}

inline void RaySphere_float(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir, out float distanceToSphere, out float distanceThroughSphere) {
	float2 result = RaySphere(sphereCenter, sphereRadius, rayOrigin, rayDir);
	distanceToSphere = result.x;
	distanceThroughSphere = result.y;
}

inline void RaySphere_half(float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir, out float distanceToSphere, out float distanceThroughSphere) {
	float2 result = RaySphere(sphereCenter, sphereRadius, rayOrigin, rayDir);
	distanceToSphere = result.x;
	distanceThroughSphere = result.y;
}

float densityAtPoint(float3 planetCenter, float planetRadius, float atmosphereRadius, float3 densitySamplePoint, float densityFalloff) {
	float heightAboveSurface = length(densitySamplePoint - planetCenter) - planetRadius;
	float height01 = heightAboveSurface / (atmosphereRadius - planetRadius);
	float localDensity = exp(-height01 * densityFalloff) * (1 - height01);
	return localDensity;
}
			
float opticalDepth(float3 rayOrigin, float3 rayDir, float rayLength, float3 planetCenter, float planetRadius, float atmosphereRadius, float densityFalloff, float numOpticalDepthPoints) {
	float3 densitySamplePoint = rayOrigin;
	float stepSize = rayLength / (numOpticalDepthPoints - 1);
	float opticalDepth = 0;

	for (int i = 0; i < numOpticalDepthPoints; i ++) {
		float localDensity = densityAtPoint(planetCenter, planetRadius, atmosphereRadius, densitySamplePoint, densityFalloff);
		opticalDepth += localDensity * stepSize;
		densitySamplePoint += rayDir * stepSize;
	}
	return opticalDepth;
}

float opticalDepthBaked(float3 rayOrigin, float3 rayDir, float3 planetCenter, float planetRadius, float atmosphereRadius, UnityTexture2D  BakedOpticalDepth) {
	float height = length(rayOrigin - planetCenter) - planetRadius;
	float height01 = saturate(height / (atmosphereRadius - planetRadius));

	float uvX = 1 - (dot(normalize(rayOrigin - planetCenter), rayDir) * .5 + .5);
	return tex2Dlod(BakedOpticalDepth, float4(uvX, height01,0,0));
}

float opticalDepthBaked2(float3 rayOrigin, float3 rayDir, float rayLength, float3 planetCenter, float planetRadius, float atmosphereRadius, UnityTexture2D  BakedOpticalDepth) {
	float3 endPoint = rayOrigin + rayDir * rayLength;
	float d = dot(rayDir, normalize(rayOrigin-planetCenter));
	float opticalDepth = 0;

	const float blendStrength = 1.5;
	float w = saturate(d * blendStrength + .5);
				
	float d1 = opticalDepthBaked(rayOrigin, rayDir, planetCenter, planetRadius, atmosphereRadius, BakedOpticalDepth) - opticalDepthBaked(endPoint, rayDir, planetCenter, planetRadius, atmosphereRadius, BakedOpticalDepth);
	float d2 = opticalDepthBaked(endPoint, -rayDir, planetCenter, planetRadius, atmosphereRadius, BakedOpticalDepth) - opticalDepthBaked(rayOrigin, -rayDir, planetCenter, planetRadius, atmosphereRadius, BakedOpticalDepth);

	opticalDepth = lerp(d2, d1, w);
	return opticalDepth;
}

inline void InScatteredLight_float(float3 planetCenter,
							 float planetRadius,
							 float atmosphereRadius,
							 float3 scatteringCoefficients,
							 float intensity,
							 float densityFalloff,
							 float numInScatteringPoints,
							 float ditherStrength,
							 float rayLength,
							 float3 rayOrigin,
							 float3 rayDir,
							 float3 sunDir,
							 float blueNoise,
							 float3 originalColor,
							 UnityTexture2D  BakedOpticalDepth,
							 out float3 atmosphereColor)
{
	float3 inScatterPoint = rayOrigin;
	float stepSize = rayLength / (numInScatteringPoints - 1);
	float3 inScatteredLight = float3(0,0,0);
	float viewRayOpticalDepth = 0;

	for (int i = 0; i < numInScatteringPoints; i ++) {
		float sunRayOpticalDepth = opticalDepthBaked(inScatterPoint + sunDir * ditherStrength, sunDir, planetCenter, planetRadius, atmosphereRadius, BakedOpticalDepth);
		float localDensity = densityAtPoint(planetCenter, planetRadius, atmosphereRadius, inScatterPoint, densityFalloff);
		viewRayOpticalDepth = opticalDepthBaked2(rayOrigin, rayDir, stepSize * i, planetCenter, planetRadius,		                                               atmosphereRadius, BakedOpticalDepth);
		float3 transmittance = exp(-(sunRayOpticalDepth + viewRayOpticalDepth) * scatteringCoefficients);
	
		inScatteredLight += localDensity * transmittance;
		inScatterPoint += rayDir * stepSize;
	}
	inScatteredLight *= scatteringCoefficients * intensity * stepSize / planetRadius;
	inScatteredLight += blueNoise * 0.01;

	// Attenuate brightness of original col (i.e light reflected from planet surfaces)
	// This is a hacky mess, TODO: figure out a proper way to do this
	const float brightnessAdaptionStrength = 0.15;
	const float reflectedLightOutScatterStrength = 3;
	float brightnessAdaption = dot (inScatteredLight,1) * brightnessAdaptionStrength;
	float brightnessSum = viewRayOpticalDepth * intensity * reflectedLightOutScatterStrength + brightnessAdaption;
	float reflectedLightStrength = exp(-brightnessSum);
	float hdrStrength = saturate(dot(originalColor,1)/3-1);
	reflectedLightStrength = lerp(reflectedLightStrength, 1, hdrStrength);
	float3 reflectedLight = originalColor * reflectedLightStrength;

	atmosphereColor = reflectedLight + inScatteredLight;
}

#endif