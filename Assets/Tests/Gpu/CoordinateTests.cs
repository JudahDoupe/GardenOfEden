using System;
using System.Linq;
using FsCheck;
using NUnit.Framework;

namespace Tests
{
    [Category("Gpu")]
    public class CoordinateTests : SystemTestBase
    {
        public Arbitrary<int[]> IntArray() => Gen.ArrayOf(Gen.Choose(0,9)).ToArbitrary();

        [Test]
        public void SampleFsCheckTest()
        {
            Prop.ForAll(IntArray(), x => RevRev(x).SequenceEqual(x)).QuickCheckThrowOnFailure();
        }

        public int[] RevRev(int[] ts)
        {
            return ts.Reverse().Reverse().ToArray();
        }
    }
}
