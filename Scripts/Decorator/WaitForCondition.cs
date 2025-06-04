using System;

namespace NPBehave
{
    public abstract class WaitForCondition : Decorator
    {
        private float checkInterval;
        private float checkVariance;

        protected WaitForCondition(float checkInterval, float randomVariance, Node decoratee) : base("WaitForCondition", decoratee)
        {
            this.checkInterval = checkInterval;
            this.checkVariance = randomVariance;
            this.Label = "" + (checkInterval - randomVariance) + "..." + (checkInterval + randomVariance) + "s";
        }

        protected WaitForCondition(Node decoratee) : base("WaitForCondition", decoratee)
        {
            this.checkInterval = 0.0f;
            this.checkVariance = 0.0f;
            this.Label = "every tick";
        }

        protected override void DoStart()
        {
            if (!IsConditionMet())
            {
                Clock.AddTimer(checkInterval, checkVariance, -1, OnTimerReached);
            }
            else
            {
                Decoratee.Start();
            }
        }

        private void OnTimerReached()
        {
            if (IsConditionMet())
            {
                Clock.RemoveTimer(OnTimerReached);
                Decoratee.Start();
            }
        }

        protected override void DoStop()
        {
            Clock.RemoveTimer(OnTimerReached);
            if (Decoratee.IsActive)
            {
                Decoratee.Stop();
            }
            else
            {
                Stopped(false);
            }
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            Stopped(result);
        }
        
        protected abstract bool IsConditionMet();
    }
}