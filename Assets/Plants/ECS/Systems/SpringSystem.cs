using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
[BurstCompile]
public struct SpringSystem : ISystem
{
    public EntityQuery SpringQuery;
    public ComponentHandles Handles;

    public struct ComponentHandles
    {
        public ComponentLookup<PhysicsVelocity> Velocities;
        public ComponentLookup<LocalTransform> LocalTransforms;
        public ComponentLookup<PhysicsMass> Masses;
        public ComponentLookup<LocalToWorld> LocalToWorlds;
        public ComponentTypeHandle<Spring> SpringHandle;

        public ComponentHandles(ref SystemState state)
        {
            Velocities = state.GetComponentLookup<PhysicsVelocity>();
            LocalTransforms = state.GetComponentLookup<LocalTransform>(true);

            Masses = state.GetComponentLookup<PhysicsMass>(true);
            LocalToWorlds = state.GetComponentLookup<LocalToWorld>(true);
            SpringHandle = state.GetComponentTypeHandle<Spring>(true);
        }

        public void Update(ref SystemState state)
        {
            Velocities.Update(ref state);
            LocalTransforms.Update(ref state);

            Masses.Update(ref state);
            LocalToWorlds.Update(ref state);
            SpringHandle.Update(ref state);
        }
    }


    [BurstCompile]
    private struct SpringJob : IJobChunk
    {
        public ComponentLookup<PhysicsVelocity> Velocities;

        [ReadOnly]
        public ComponentLookup<LocalTransform> LocalTransforms;

        [ReadOnly]
        public ComponentLookup<PhysicsMass> Masses;

        [ReadOnly]
        public ComponentLookup<LocalToWorld> LocalToWorlds;

        [ReadOnly]
        public ComponentTypeHandle<Spring> SpringHandle;

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var chunkSpring = chunk.GetNativeArray(ref SpringHandle);

            var hasChunkSpringType = chunk.Has(ref SpringHandle);

            if (!hasChunkSpringType)
                // should never happen
                return;

            var instanceCount = chunk.Count;

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, instanceCount);

            while (enumerator.NextEntityIndex(out var i))
            {
                var spring = chunkSpring[i];
                var node = spring.NodeEntity;
                var parentNode = spring.NodeParentEntity;

                if (spring.Strength == 0
                    || node == Entity.Null
                    || !Velocities.HasComponent(node))
                    continue;

                var localTransformA = LocalTransform.Identity;

                PhysicsVelocity velocityA = default;
                PhysicsMass massA = default;

                var localTransformB = localTransformA;
                var velocityB = velocityA;
                var massB = massA;

                if (LocalTransforms.HasComponent(node)) localTransformA = LocalTransforms[node];

                if (Velocities.HasComponent(node)) velocityA = Velocities[node];
                if (Masses.HasComponent(node)) massA = Masses[node];

                if (LocalToWorlds.HasComponent(parentNode))
                {
                    // parent could be static and not have a Translation or Rotation
                    var worldFromBody = Math.DecomposeRigidBodyTransform(LocalToWorlds[parentNode].Value);
                    localTransformB.Position = worldFromBody.pos;
                    localTransformB.Rotation = worldFromBody.rot;
                }

                if (LocalTransforms.HasComponent(parentNode)) localTransformB = LocalTransforms[parentNode];
                if (Velocities.HasComponent(parentNode)) velocityB = Velocities[parentNode];
                if (Masses.HasComponent(parentNode)) massB = Masses[parentNode];

                var posA = math.transform(new RigidTransform(localTransformA.Rotation, localTransformA.Position), float3.zero);
                var posB = math.transform(new RigidTransform(localTransformB.Rotation, localTransformB.Position), spring.EquilibriumPosition);
                var lvA = velocityA.GetLinearVelocity(massA, localTransformA.Position, localTransformA.Rotation, posA);
                var lvB = velocityB.GetLinearVelocity(massB, localTransformB.Position, localTransformB.Rotation, posB);

                var impulse = spring.Strength * (posB - posA) + spring.Damping * (lvB - lvA);
                impulse = math.clamp(impulse, new float3(-100.0f), new float3(100.0f));
                velocityA.ApplyImpulse(massA, localTransformA.Position, localTransformA.Rotation, impulse, posA);

                Velocities[node] = velocityA;
            }
        }
    }

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        Handles = new ComponentHandles(ref state);

        SpringQuery = state.GetEntityQuery(ComponentType.ReadOnly<Spring>());
        state.RequireForUpdate(SpringQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Handles.Update(ref state);

        state.Dependency = new SpringJob
        {
            Velocities = Handles.Velocities,
            LocalTransforms = Handles.LocalTransforms,
            Masses = Handles.Masses,
            LocalToWorlds = Handles.LocalToWorlds,
            SpringHandle = Handles.SpringHandle
        }.Schedule(SpringQuery, state.Dependency);
    }
}