namespace NPBehave
{
    public class TestRoot : Root
    {
        private bool didFinish = false;
        private bool wasSuccess = false;

        public bool DidFinish
        {
            get { return didFinish; }
        }

        public bool WasSuccess
        {
            get { return wasSuccess; }
        }

        public TestRoot(BehaveWorld behaveWorld, Blackboard blackboard, Node decoratee) : base(behaveWorld, blackboard, decoratee)
        {
        }

        protected override void DoStart()
        {
            this.didFinish = false;
            base.DoStart();
        }

        protected override void DoChildStopped(Node node, bool success)
        {
            didFinish = true;
            wasSuccess = success;
            Stopped(success);
        }
    }
}