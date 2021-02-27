using FsCheck;
using Unity.Mathematics;

namespace Tests
{
    public class FsCheckUtils
    {
        public static Configuration Config  {
            get
            {
                var config = Configuration.QuickThrowOnFailure;
                config.MaxNbOfTest = 100;
                return config;
            }
        }

        public static Gen<float3> GenFloat3(float3 min, float3 max)
        {
            return from x in GenFloat(min.x, max.x)
            from y in GenFloat(min.y, max.y)
            from z in GenFloat(min.z, max.z)
            select new  float3(x,y,z);
        }
        public static Gen<float> GenFloat(float min, float max)
        {
            return Gen0To1().Select(x => math.lerp(min, max, x));
        }
        public static Gen<float> Gen0To1()
        {
            return Gen.Choose(0, 1000000).Select(x => x / 1000000f);
        }
    }
}