using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Tests
{
    public class MockLandService : ILandService
    {
        public void AddSpring(Coordinate coord)
        {
            throw new NotImplementedException();
        }

        public Coordinate ClampAboveTerrain(Coordinate coord)
        {
            throw new NotImplementedException();
        }

        public Coordinate ClampToTerrain(Coordinate coord)
        {
            throw new NotImplementedException();
        }

        public Texture2D GetLandMap()
        {
            return new Texture2D(EnvironmentDataStore.TextureSize, EnvironmentDataStore.TextureSize, TextureFormat.RGBAFloat, false);
        }

        public void PullMountain(Coordinate coord, float height)
        {
            throw new NotImplementedException();
        }

        public float SampleTerrainHeight(Coordinate coord)
        {
            throw new NotImplementedException();
        }
    }
}