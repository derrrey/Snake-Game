using Leopotam.Ecs;

namespace SnaekGaem.Src.Tools
{
    // Struct with an ecs entity and a deletion flag
    struct EntityWithFlag
    {
        public EcsEntity entity { get; set; }
        public bool deletionFlag { get; set; }

        public EntityWithFlag(EcsEntity entity, bool deletionFlag)
        {
            this.entity = entity;
            this.deletionFlag = deletionFlag;
        }
    }
}
