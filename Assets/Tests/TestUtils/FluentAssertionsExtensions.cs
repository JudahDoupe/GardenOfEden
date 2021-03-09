using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Unity.Mathematics;

namespace Tests
{
    public static class FluentAssertionsExtensions
    {
        public static Float3Assertions Should(this float3 instance)
        {
            return new Float3Assertions(instance);
        }
    }

    public class Float3Assertions : ReferenceTypeAssertions<float3, Float3Assertions>
    {
        public Float3Assertions(float3 instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "float3";

        public AndConstraint<Float3Assertions> BeApproximately(
            float3 expectedValue,
            float precision,
            string because = "",
            params object[] becauseArgs)
        {
            var val = Subject;
            var maxDiff = new[] {
                math.abs(val.x - expectedValue.x),
                math.abs(val.y - expectedValue.y),
                math.abs(val.z - expectedValue.z)
            }.Max();


            Execute.Assertion.ForCondition(maxDiff < precision)
                .BecauseOf(because, becauseArgs)
                .FailWith($"Expected {val} to approximate {expectedValue} +/- {precision}{because}, but {val} differed by {maxDiff}.");
            return new AndConstraint<Float3Assertions>(this);
        }
    }
}