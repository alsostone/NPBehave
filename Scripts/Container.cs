
namespace NPBehave
{
    public abstract class Container : Node
    {
        private bool collapse = false;
        public bool Collapse
        {
            get
            {
                return collapse;
            }
            set
            {
                collapse = value;
            }
        }

        public Container(string name) : base(name)
        {
        }

        public void ChildStopped(Node child, bool succeeded)
        {
            this.DoChildStopped(child, succeeded);
        }

        protected abstract void DoChildStopped(Node child, bool succeeded);

#if UNITY_EDITOR
        public abstract Node[] DebugChildren
        {
            get;
        }
#endif
    }
}