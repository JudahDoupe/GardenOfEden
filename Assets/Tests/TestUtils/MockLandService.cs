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

        public float SampleTerrainHeight(SphericalCoord location)
        {
            throw new NotImplementedException();
        }

        public CartesianCoord ClampAboveTerrain(CartesianCoord location)
        {
            throw new NotImplementedException();
        }

        public CartesianCoord ClampToTerrain(CartesianCoord location)
        {
            throw new NotImplementedException();
        }

        public void PullMountain(SphericalCoord location, float height)
        {
            throw new NotImplementedException();
        }

        public void AddSpring(SphericalCoord location)
        {
            throw new NotImplementedException();
        }
    }
}