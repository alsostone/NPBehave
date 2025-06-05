using MemoryPack;

namespace NPBehave
{
    public class Root : Decorator
    {
        private readonly Blackboard blackboard;
        private readonly Clock clock;
        
        [MemoryPackIgnore] public Blackboard RootBlackboard => blackboard;
        [MemoryPackIgnore] public Clock RootClock => clock;

#if UNITY_EDITOR
        [MemoryPackIgnore] public int TotalNumStartCalls = 0;
        [MemoryPackIgnore] public int TotalNumStopCalls = 0;
        [MemoryPackIgnore] public int TotalNumStoppedCalls = 0;
#endif

        public Root(Clock clock, Node decoratee) : base("Root", decoratee)
        {
            this.blackboard = new Blackboard(clock);
            this.clock = clock;
            this.SetRoot(this);
        }

        [MemoryPackConstructor]
        public Root(Blackboard blackboard, Clock clock, Node decoratee) : base("Root", decoratee)
        {
            this.blackboard = blackboard;
            this.clock = clock;
            this.SetRoot(this);
        }

        public sealed override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);
        }

        protected override void DoStart()
        {
            this.blackboard.Enable();
            this.Decoratee.Start();
        }

        protected override void DoStop()
        {
            if (this.Decoratee.IsActive)
            {
                this.Decoratee.Stop();
            }
            else
            {
                this.clock.RemoveTimer(this.Decoratee.Start);
            }
        }
        
        protected override void DoChildStopped(Node node, bool success)
        {
            if (!IsStopRequested)
            {
                // wait one tick, to prevent endless recursions
                Clock.AddTimer(0, 0, this.Decoratee.Start);
            }
            else
            {
                this.blackboard.Disable();
                Stopped(success);
            }
        }
    }
}
