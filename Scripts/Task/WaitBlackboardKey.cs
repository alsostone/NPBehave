namespace NPBehave
{
    public class WaitBlackboardKey : Task
    {
        private readonly string blackboardKey = null;
        private readonly float randomVariance;
        
        public WaitBlackboardKey(string blackboardKey, float randomVariance = 0f) : base("WaitBlackboardKey")
        {
            this.blackboardKey = blackboardKey;
            this.randomVariance = randomVariance;
        }
        
        protected override void DoStart()
        {
            float delay = Blackboard.Get<float>(this.blackboardKey);
            if (delay < 0)
            {
                delay = 0;
            }

            if (randomVariance >= 0f)
            {
                Clock.AddTimer(delay, randomVariance, 0, OnTimerReached);
            }
            else
            {
                Clock.AddTimer(delay, 0, OnTimerReached);
            }
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(OnTimerReached);
            this.Stopped(false);
        }

        private void OnTimerReached()
        {
            Clock.RemoveTimer(OnTimerReached);
            this.Stopped(true);
        }
    }
}