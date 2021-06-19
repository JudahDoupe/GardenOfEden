﻿namespace Tests
{
    public class MockLandService : ILandService
    {
        public Coordinate ClampAboveLand(Coordinate coord, float minHeight = 1)
        {
            return coord;
        }

        public Coordinate ClampToLand(Coordinate coord)
        {
            return coord;
        }

        public float SampleHeight(Coordinate coord)
        {
            return coord.Altitude;
        }
    }
}