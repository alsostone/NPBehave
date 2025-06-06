using System;
using MemoryPack;

namespace NPBehave
{
    public class Root : Decorator
    {
        private readonly BehaveWorld behaveWorld;
        private readonly Blackboard blackboard;

        [MemoryPackIgnore] public Blackboard RootBlackboard => blackboard;
        [MemoryPackIgnore] public Clock RootClock => behaveWorld.Clock;
        [MemoryPackIgnore] public BehaveWorld RootBehaveWorld => behaveWorld;

#if UNITY_EDITOR
        [MemoryPackIgnore] public int TotalNumStartCalls = 0;
        [MemoryPackIgnore] public int TotalNumStopCalls = 0;
        [MemoryPackIgnore] public int TotalNumStoppedCalls = 0;
#endif
        
        [MemoryPackConstructor]
        public Root(Node decoratee) : base("Root", decoratee)
        {
        }

        public Root(BehaveWorld behaveWorld, Blackboard blackboard, Node decoratee) : base("Root", decoratee)
        {
            this.behaveWorld = behaveWorld;
            this.blackboard = blackboard;
            SetRoot(this);
        }

        public sealed override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);
        }

        protected override void DoStart()
        {
            blackboard.Enable();
            Decoratee.Start();
        }

        protected override void DoStop()
        {
            if (Decoratee.IsActive)
            {
                Decoratee.Stop();
            }
            else
            {
                RootClock.RemoveTimer(Guid);
            }
        }
        
        protected override void DoChildStopped(Node node, bool success)
        {
            if (!IsStopRequested)
            {
                // wait one tick, to prevent endless recursions
                Clock.AddTimer(0, 0, Guid);
            }
            else
            {
                blackboard.Disable();
                Stopped(success);
            }
        }

        public override void OnTimerReached()
        {
            Decoratee.Start();
        }
    }
}
