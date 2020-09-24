using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace Assets.Scripts.Plants.ECS.Components
{
    public struct Internode : IComponentData
    {
        public Entity HeadNode { get; set; }
        public Entity TailNode { get; set; }
    }

    public struct InternodeReference : IComponentData
    {
        public Entity Internode { get; set; }
    }
}
