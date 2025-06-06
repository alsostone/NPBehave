﻿namespace NPBehave
{
    public class MockNode : Node
    {
        private bool succedsOnExplictStop;

        public MockNode(bool succedsOnExplictStop = false) : base("MockNode")
        {
            this.succedsOnExplictStop = succedsOnExplictStop;
        }

        protected override void DoStop()
        {
            this.Stopped(succedsOnExplictStop);
        }

        public void Finish(bool success)
        {
            this.Stopped(success);
        }
    }
}