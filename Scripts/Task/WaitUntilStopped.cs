namespace NPBehave
{
    public class WaitUntilStopped : Task
    {
        private readonly bool successWhenStopped;
        
        public WaitUntilStopped(bool successWhenStopped = false) : base("WaitUntilStopped")
        {
            this.successWhenStopped = successWhenStopped;
        }

        protected override void DoStop()
        {
            this.Stopped(successWhenStopped);
        }
    }
}