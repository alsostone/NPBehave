
namespace NPBehave
{
    public enum Result
    {
        SUCCESS,
        FAILED,
        BLOCKED,
        PROGRESS
    }

    public enum Request
    {
        START,
        UPDATE,
        CANCEL,
    }

    public abstract class ActionRequest : Task
    {
        private bool bWasBlocked = false;

        protected ActionRequest() : base("ActionRequest")
        {
        }

        protected override void DoStart()
        {
            Result result = OnAction(Request.START);
            if (result == Result.PROGRESS)
            {
                Clock.AddUpdateObserver(OnUpdate);
            }
            else if (result == Result.BLOCKED)
            {
                this.bWasBlocked = true;
                Clock.AddUpdateObserver(OnUpdate);
            }
            else
            {
                this.Stopped(result == Result.SUCCESS);
            }
        }

        private void OnUpdate()
        {
            Result result = OnAction(bWasBlocked ? Request.START : Request.UPDATE);
            if (result == Result.BLOCKED)
            {
                bWasBlocked = true;
            }
            else if (result == Result.PROGRESS)
            {
                bWasBlocked = false;
            }
            else
            {
                Clock.RemoveUpdateObserver(OnUpdate);
                this.Stopped(result == Result.SUCCESS);
            }
        }

        protected override void DoStop()
        {
            Result result = OnAction(Request.CANCEL);
            Clock.RemoveUpdateObserver(OnUpdate);
            this.Stopped(result == Result.SUCCESS);
        }
        
        protected abstract Result OnAction(Request request);
    }
}