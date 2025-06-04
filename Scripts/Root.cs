
namespace NPBehave
{
    public class Root : Decorator
    {
        private Blackboard blackboard;
        public Blackboard RootBlackboard
        {
            get
            {
                return blackboard;
            }
        }
        
        private Clock clock;
        public Clock RootClock
        {
            get
            {
                return clock;
            }
        }

#if UNITY_EDITOR
        public int TotalNumStartCalls = 0;
        public int TotalNumStopCalls = 0;
        public int TotalNumStoppedCalls = 0;
#endif

        public Root(Node decoratee) : base("Root", decoratee)
        {
            this.clock = UnityContext.GetClock();
            this.blackboard = new Blackboard(this.clock);
            this.SetRoot(this);
        }
        
        public Root(Blackboard blackboard, Node decoratee) : base("Root", decoratee)
        {
            this.blackboard = blackboard;
            this.clock = UnityContext.GetClock();
            this.SetRoot(this);
        }

        public Root(Blackboard blackboard, Clock clock, Node decoratee) : base("Root", decoratee)
        {
            this.blackboard = blackboard;
            this.clock = clock;
            this.SetRoot(this);
        }

        public override void SetRoot(Root rootNode)
        {
            base.SetRoot(rootNode);
            this.Decoratee.SetRoot(rootNode);
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
                this.clock.AddTimer(0, 0, this.Decoratee.Start);
            }
            else
            {
                this.blackboard.Disable();
                Stopped(success);
            }
        }
    }
}
