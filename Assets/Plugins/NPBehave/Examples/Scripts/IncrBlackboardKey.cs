using MemoryPack;

namespace NPBehave
{
    [MemoryPackable]
    public partial class IncrBlackboardKey : Node
    {
        [MemoryPackInclude] private string blackboardKey;

        public IncrBlackboardKey(string blackboardKey) : base("IncrBlackboardKey")
        {
            this.blackboardKey = blackboardKey;
        }
        protected override void DoStart()
        {
            Clock.AddUpdateObserver(Guid);
        }
        protected override void DoStop()
        {
            Clock.RemoveUpdateObserver(Guid);
        }
        public override void OnTimerReached()
        {
            Blackboard.SetInt(blackboardKey, Blackboard.GetInt(blackboardKey) + 1);
        }
    }

}