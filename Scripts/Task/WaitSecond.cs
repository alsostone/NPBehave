namespace NPBehave
{
    public class WaitSecond : Task
    {
        private readonly float seconds;
        private readonly float randomVariance;
        
        public WaitSecond(float seconds, float randomVariance) : base("WaitSecond")
        {
            this.seconds = seconds;
            this.randomVariance = randomVariance;
        }

        public WaitSecond(float seconds) : base("WaitSecond")
        {
            this.seconds = seconds;
            this.randomVariance = this.seconds * 0.05f;
        }

        protected override void DoStart()
        {
            float delay = this.seconds;
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