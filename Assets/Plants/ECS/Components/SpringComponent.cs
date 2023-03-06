using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

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
            var worldIndex = GetWorldIndexFromBaseJoint(authoring);

            var forward = authoring.ConnectedBody == null
                          ? Vector3.forward
                          : authoring.ConnectedBody.transform.InverseTransformDirection(authoring.transform.forward);
            var internodeLength = math.max(authoring.transform.localPosition.magnitude, 0.001f);

            var ballAndSocket = PhysicsJoint.CreateBallAndSocket(new float3(0, 0, -internodeLength), float3.zero);
            ballAndSocket.SetConstraints(new FixedList512Bytes<Constraint>
            {
                Length = 3,
                [0] = Constraint.Cone(2, new Math.FloatRange(0, math.PI / 4f)),
                [1] = Constraint.BallAndSocket(),
                [2] = Constraint.Twist(2, new Math.FloatRange(0, 0))
            });
            ballAndSocket.SetImpulseEventThresholdAllConstraints(authoring.MaxImpulse);

            var constrainedBodyPair = GetConstrainedBodyPair(authoring);
            var jointEntity = CreateJointEntity(worldIndex, constrainedBodyPair, ballAndSocket);

            AddComponent(jointEntity,
                         new Spring
                         {
                             NodeEntity = GetEntity(),
                             NodeParentEntity = GetEntity(authoring.ConnectedBody),
                             EquilibriumPosition = forward * internodeLength,
                             Strength = authoring.Strength,
                             Damping = authoring.Damping
                         });
        }
    }
}