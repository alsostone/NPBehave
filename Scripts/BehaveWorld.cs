using System.Collections.Generic;
using MemoryPack;

namespace NPBehave
{
    [MemoryPackable]
    public partial class BehaveWorld
    {
        [MemoryPackInclude] public Clock Clock { get; private set; }
        
        [MemoryPackInclude] private int currentGuid = 0;
        [MemoryPackInclude] private List<Blackboard> blackboards;
        [MemoryPackInclude] private readonly Dictionary<string, int> sharedBlackboards;
        
        [MemoryPackIgnore] public readonly Dictionary<int, Receiver> IdNodeMapping = new Dictionary<int, Receiver>();
        
        public BehaveWorld()
        {
            this.Clock = new Clock();
            this.Clock.Set(this);
            this.blackboards = new List<Blackboard>();
            this.sharedBlackboards = new Dictionary<string, int>();
        }
        
        [MemoryPackConstructor]
        private BehaveWorld(Clock clock, List<Blackboard> blackboards, Dictionary<string, int> sharedBlackboards)
        {
            this.Clock = clock;
            this.Clock.Set(this);
            this.blackboards = blackboards;
            this.sharedBlackboards = sharedBlackboards;
        }
        
        [MemoryPackOnDeserialized]
        private void OnDeserialized() 
        {
            foreach (var blackboard in blackboards)
            {
                blackboard.Set(this);
            }
        }
        
        public int GetNextGuid()
        {
            return ++currentGuid;
        }
        
        internal Blackboard GetBlackboard(int guid)
        {
            if (IdNodeMapping.TryGetValue(guid, out var receiver))
            {
                return receiver as Blackboard;
            }
            return null;
        }

        public Blackboard CreateBlackboard(Blackboard parent = null)
        {
            var blackboard = new Blackboard(parent);
            blackboard.Set(this);
            blackboards.Add(blackboard);
            return blackboard;
        }
        
        public Blackboard GetSharedBlackboard(string key)
        {
            Blackboard blackboard;
            if (sharedBlackboards.TryGetValue(key, out var guid))
            {
                blackboard = GetBlackboard(guid);
            }
            else
            {
                blackboard = CreateBlackboard();
                sharedBlackboards.Add(key, blackboard.Guid);
            }
            return blackboard;
        }

        public void Update(float deltaTime)
        {
            Clock.Update(deltaTime);
        }
        
    }
}