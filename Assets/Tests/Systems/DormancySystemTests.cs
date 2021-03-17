using Assets.Scripts.Plants.Cleanup;
using Assets.Scripts.Plants.Growth;
using FluentAssertions;
using FsCheck;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

namespace Tests
{
    [Category("Systems")]
    public class DormancySystemTests : SystemTestBase
    {
        public static Gen<ParentDormancyTrigger> GenParentDormancyTrigger() =>
            from parent in FsCheckUtils.GenBool()
            from unparent in FsCheckUtils.GenBool()
            select new ParentDormancyTrigger { IsDormantWhenParented = parent, IsDormantWhenUnparented = unparent};

        [Test]
        public void ParentDormancyTest([Values(true, false)] bool shouldParent, 
                                       [Values(true, false)] bool shouldUnparent,
                                       [Values(true, false)] bool isDormant,
                                       [Values(true, false)] bool hasParent)
        {
            var entity = m_Manager.CreateEntity();
            m_Manager.AddSharedComponentData(entity, Singleton.LoadBalancer.CurrentChunk);
            m_Manager.AddComponentData(entity, new ParentDormancyTrigger{IsDormantWhenParented = shouldParent, IsDormantWhenUnparented = shouldUnparent });
            if (hasParent)
            {
                m_Manager.AddComponent<Parent>(entity);
            }
            if (isDormant)
            {
                m_Manager.AddComponent<Dormant>(entity);
            }

            World.GetOrCreateSystem<DormancySystem>().Update();
            World.GetOrCreateSystem<CleanupEcbSystem>().Update();

            isDormant = m_Manager.HasComponent<Dormant>(entity);
            var shouldBeDormant = (shouldParent && hasParent) || (shouldUnparent && !hasParent);
            isDormant.Should().Be(shouldBeDormant);
        }
    }
}
