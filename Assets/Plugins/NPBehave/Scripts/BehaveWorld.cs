using System;
using System.Collections.Generic;
using MemoryPack;

namespace NPBehave
{
    [MemoryPackable]
    public partial class BehaveWorld : IDisposable
    {
        [MemoryPackInclude] public Clock Clock { get; private set; }
        
        [MemoryPackInclude] private int currentGuid = 0;
        [MemoryPackInclude] private List<Blackboard> blackboards;
        [MemoryPackInclude] private readonly Dictionary<string, int> sharedBlackboards;
        
        [MemoryPackIgnore] public readonly Dictionary<int, Receiver> GuidReceiverMapping = new Dictionary<int, Receiver>();
        [MemoryPackIgnore] private System.Random random;
        
        public BehaveWorld()
        {
            Clock = new Clock(this);
            blackboards = new List<Blackboard>();
            sharedBlackboards = new Dictionary<string, int>();
        }
        
        [MemoryPackConstructor]
        private BehaveWorld(Clock clock, List<Blackboard> blackboards, Dictionary<string, int> sharedBlackboards)
        {
            Clock = clock;
            this.blackboards = blackboards;
            this.sharedBlackboards = sharedBlackboards;
        }
        
        [MemoryPackOnDeserialized]
        private void OnDeserialized() 
        {
            Clock.SetWorld(this);
            foreach (var blackboard in blackboards)
            {
                blackboard.SetWorld(this);
            }
        }
        
        // 防止因为循环依赖导致无法GC
        public void Dispose()
        {
            Clock.Dispose();
            Clock = null;
            foreach (var blackboard in blackboards)
            {
                blackboard.Dispose();
            }
            blackboards.Clear();
            sharedBlackboards.Clear();
            GuidReceiverMapping.Clear();
            random = null;
        }
        
        public void SetRandom(System.Random random)
        {
            this.random = random;
        }
        
        public int GetNextGuid()
        {
            return ++currentGuid;
        }
        
        public Blackboard GetBlackboard(int guid)
        {
            if (GuidReceiverMapping.TryGetValue(guid, out var receiver))
            {
                return receiver as Blackboard;
            }
            return null;
        }

        public Blackboard CreateBlackboard(Blackboard parent = null)
        {
            var blackboard = new Blackboard(this, parent);
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

        public int GetRandomNext(int range)
        {
            if (random == null)
                random = new System.Random(Environment.TickCount);
            return random.Next(range);
        }

    }
}