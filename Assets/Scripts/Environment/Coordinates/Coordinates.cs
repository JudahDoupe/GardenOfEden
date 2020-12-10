using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Environment.Coordinates
{
    public struct CartesianCoord
    {
        public float3 XYZ;
        public float X => XYZ.x;
        public float Y => XYZ.y;
        public float Z => XYZ.z;

        public CartesianCoord(float x, float y, float z)
        {
            XYZ = new float3(x, y, z);
        }
        public CartesianCoord(float3 v)
        {
            XYZ = v;
        }
        public CartesianCoord(SphericalCoord sc)
        {
            XYZ = new float3(
                math.cos(sc.Phi) * math.cos(sc.Theta) * sc.Height,
                math.sin(sc.Phi) * sc.Height,
                math.cos(sc.Phi) * math.sin(sc.Theta) * sc.Height);
        }
        
        public SphericalCoord ToSpherical() => new SphericalCoord(this);
        public CubeMapCoord ToCubeMap() => new CubeMapCoord(this);
    }

    public struct SphericalCoord
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

        public CartesianCoord ToCartesian() => new CartesianCoord(this);
        public CubeMapCoord ToCubeMap() => new CubeMapCoord(ToCartesian());
    }

    public struct CubeMapCoord
    {
        public CubemapFace Face;

        public float2 UV;
        public float U => UV.x;
        public float V => UV.y;

        public int2 XY => math.int2(math.floor(UV * EnvironmentalChunkService.TextureSize));
        public int X => XY.x;
        public int Y => XY.y;

        public CubeMapCoord(float u, float v, CubemapFace face)
        {
            UV = new float2(u, v);
            Face = face;
        }
        public CubeMapCoord(int x, int y, CubemapFace face)
        {
            UV = new float2(x, y) / EnvironmentalChunkService.TextureSize;
            Face = face;
        }
        public CubeMapCoord(CartesianCoord cc)
        {
            var v = cc.XYZ;
            var abs = math.abs(v);

            int greatestIndex = 0;
            for (int i = 1; i < 3; i++)
                if (abs[i] > abs[greatestIndex])
                    greatestIndex = i;

            v /= abs[greatestIndex];

            switch (greatestIndex)
            {
                case 0:
                    if (v.x > 0)
                    {
                        Face = CubemapFace.PositiveX;
                        UV = new float2(v.z, v.y);
                    }
                    else
                    {
                        Face = CubemapFace.NegativeX;
                        UV = new float2(-v.z, v.y);
                    }
                    break;
                case 1:
                    if (v.y > 0)
                    {
                        Face = CubemapFace.PositiveY;
                        UV = new float2(-v.x, v.z);
                    }
                    else
                    {
                        Face = CubemapFace.NegativeY;
                        UV = new float2(-v.x, -v.z);
                    }
                    break;
                case 2:
                    if (v.z > 0)
                    {
                        Face = CubemapFace.PositiveZ;
                        UV = new float2(-v.x, v.y);
                    }
                    else
                    {
                        Face = CubemapFace.NegativeZ;
                        UV = new float2(v.x, v.y);
                    }
                    break;
                default:
                    Face = CubemapFace.PositiveX;
                    UV = new float2(v.z, v.y);
                    break;
            }
        }
    }
}
