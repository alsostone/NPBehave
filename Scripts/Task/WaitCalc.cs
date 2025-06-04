namespace NPBehave
{
    public abstract class WaitCalc : Task
    {
        private readonly float randomVariance;
        
        protected WaitCalc(float randomVariance = 0f) : base("WaitCalc")
        {
            this.randomVariance = randomVariance;
        }

        protected override void DoStart()
        {
            float delay = CalcSeconds();
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

        protected abstract float CalcSeconds();
    }
}