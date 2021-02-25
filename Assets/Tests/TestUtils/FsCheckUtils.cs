using FsCheck;
using NUnit.Framework;
using Unity.Entities.Tests;

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

        public static Gen<float> Gen0To1()
        {
            return Gen.Choose(0, 1000000).Select(x => x / 1000000f);
        }
        public static Gen<float> GenNeg1To1()
        {
            return Gen.Choose(-1000000, 1000000).Select(x => x / 1000000f);
        }
    }
}