namespace NPBehave
{
    public class Test
    {
        protected TestRoot Root;
        protected Blackboard Blackboard;
        protected Clock Timer;

        protected TestRoot CreateBehaviorTree(Node sut)
        {
            this.Timer = new Clock();
            this.Blackboard = new Blackboard(this.Timer);
            this.Root = new TestRoot(Blackboard, Timer, sut);
            return Root;
        }
    }
}