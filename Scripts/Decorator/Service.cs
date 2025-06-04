using MemoryPack;

namespace NPBehave
{
    public abstract class Service : Decorator
    {
        [MemoryPackInclude] protected readonly float interval = -1.0f;
        [MemoryPackInclude] protected readonly float randomVariation;

        protected Service(float interval, float randomVariation, Node decoratee) : base("Service", decoratee)
        {
            this.interval = interval;
            this.randomVariation = randomVariation;
            this.Label = "" + (interval - randomVariation) + "..." + (interval + randomVariation) + "s";
        }

        protected Service(float interval, Node decoratee) : base("Service", decoratee)
        {
            this.interval = interval;
            this.randomVariation = interval * 0.05f;
            this.Label = "" + (interval - randomVariation) + "..." + (interval + randomVariation) + "s";
        }

        protected Service(Node decoratee) : base("Service", decoratee)
        {
            this.Label = "every tick";
        }

        protected override void DoStart()
        {
            if (this.interval <= 0f)
            {
                this.Clock.AddUpdateObserver(OnService);
                OnService();
            }
            else if (randomVariation <= 0f)
            {
                Clock.AddTimer(this.interval, -1, OnService);
                OnService();
            }
            else
            {
                InvokeServiceMethodWithRandomVariation();
            }
            Decoratee.Start();
        }

        protected override void DoStop()
        {
            Decoratee.Stop();
        }

        protected override void DoChildStopped(Node child, bool result)
        {
            if (this.interval <= 0f)
            {
                this.Clock.RemoveUpdateObserver(OnService);
            }
            else if (randomVariation <= 0f)
            {
                this.Clock.RemoveTimer(OnService);
            }
            else
            {
                this.Clock.RemoveTimer(InvokeServiceMethodWithRandomVariation);
            }
            Stopped(result);
        }

        private void InvokeServiceMethodWithRandomVariation()
        {
            OnService();
            Clock.AddTimer(interval, randomVariation, 0, InvokeServiceMethodWithRandomVariation);
        }
        
        protected abstract void OnService();
    }
}