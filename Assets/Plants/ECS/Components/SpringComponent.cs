using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;

public struct Spring : IComponentData
{
    public Entity NodeEntity;
    public Entity NodeParentEntity;
    public float3 EquilibriumPosition;

    public float Strength;
    public float Damping;
}

public class SpringComponent : BaseJoint
{
    public float Strength;
    public float Damping;

    private void OnEnable() { }
}

internal class SpringBaker : JointBaker<SpringComponent>
{
    public override void Bake(SpringComponent authoring)
    {
        if (authoring.enabled)
        {
            uint worldIndex = GetWorldIndexFromBaseJoint(authoring);

            var ballAndSocket = PhysicsJoint.CreateBallAndSocket(new float3(0, 0, authoring.transform.localPosition.magnitude), float3.zero);
            var constraints = ballAndSocket.GetConstraints();
            constraints.Add(Constraint.Twist(2, new Math.FloatRange(0, 0)));
            ballAndSocket.SetConstraints(constraints);
            ballAndSocket.SetImpulseEventThresholdAllConstraints(authoring.MaxImpulse);

            var constrainedBodyPair = GetConstrainedBodyPair(authoring);
            var jointEntity = CreateJointEntity(worldIndex, constrainedBodyPair, ballAndSocket);

            AddComponent(jointEntity, new Spring()
            {
                NodeEntity = GetEntity(),
                NodeParentEntity = GetEntity(authoring.ConnectedBody),
                EquilibriumPosition = authoring.transform.localPosition,
                Strength = authoring.Strength,
                Damping = authoring.Damping
            });
        }
    }
}
