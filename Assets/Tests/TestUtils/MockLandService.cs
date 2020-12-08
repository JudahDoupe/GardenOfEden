using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Tests
{
    public class MockLandService : ILandService
    {
        public Texture2D GetLandMap()
        {
            return new Texture2D(EnvironmentalChunkService.TextureSize, EnvironmentalChunkService.TextureSize, TextureFormat.RGBAFloat, false);
        }

        public Vector3 ClampAboveTerrain(Vector3 location) => location;

        public Vector3 ClampToTerrain(Vector3 location) => location;

        public float SampleRootDepth(Vector3 location) => 0;

        public float SampleSoilDepth(Vector3 location) => 0;

        public float SampleTerrainHeight(Vector3 location) => 0;

        public float SampleWaterDepth(Vector3 location) => 0;

        public void PullMountain(Vector3 location, float height)
        {
            throw new NotImplementedException();
        }

        public void AddSpring(Vector3 location)
        {
            throw new NotImplementedException();
        }
    }
}