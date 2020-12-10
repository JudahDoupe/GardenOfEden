using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Environment.Coordinates
{
    struct CartesianCoord
    {
        public float X;
        public float Y;
        public float Z;

        public CartesianCoord(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public CartesianCoord(Vector3 v)
        {
            X = v.x;
            Y = v.y;
            Z = v.z;
        }

        public CartesianCoord(SphericalCoord sc)
        {
            X = math.cos(sc.Phi) * math.cos(sc.Theta) * sc.Height;
            Y = math.sin(sc.Phi) * sc.Height;
            Z = math.cos(sc.Phi) * math.sin(sc.Theta) * sc.Height;
        }
    }
    struct SphericalCoord
    {
        public float Theta;
        public float Phi;
        public float Height;

        public SphericalCoord(float theta, float phi, float height)
        {
            Theta = theta;
            Phi = phi;
            Height = height;
        }

        public SphericalCoord(CartesianCoord cc)
        {
            Height = math.sqrt(math.pow(cc.X, 2) + math.pow(cc.Y, 2) + math.pow(cc.Z, 2));
            Theta = math.acos(cc.Z / Height);
            Phi = math.atan(cc.Y / cc.X);
        }
    }
}
