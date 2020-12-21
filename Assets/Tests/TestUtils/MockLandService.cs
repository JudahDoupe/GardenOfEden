﻿using System;
using UnityEngine;

namespace Tests
{
    public class MockLandService : ILandService
    {
        public void AddSpring(Coordinate coord)
        {
            throw new NotImplementedException();
        }

        public Coordinate ClampAboveTerrain(Coordinate coord, float minHeight)
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

        public void PullMountain(Coordinate coord, float radius, float height)
        {
            throw new NotImplementedException();
        }

        public float SampleTerrainHeight(Coordinate coord)
        {
            throw new NotImplementedException();
        }
    }
}