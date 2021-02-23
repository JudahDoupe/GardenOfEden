using System;
using System.Linq;
using FsCheck;
using NUnit.Framework;
using Unity.Mathematics;

namespace Tests
{
    [Category("Gpu")]
    public class CoordinateTests : SystemTestBase
    {
        public static Gen<int3> GenXyw(int textureWidth)
        {
            return from x in Gen.Choose(0, textureWidth - 1)
                   from y in Gen.Choose(0, textureWidth - 1)
                   from w in Gen.Choose(0, 5)
                   select new int3();
        }
    }
}
