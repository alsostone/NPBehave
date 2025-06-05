using System;
using MemoryPack;

namespace NPBehave
{
    [MemoryPackable]
    public partial class BlackboardCondition<T> : ObservingDecorator where T : IComparable<T>
    {
        [MemoryPackInclude] private readonly string blackboardKey;
        [MemoryPackInclude] private readonly T value;
        [MemoryPackInclude] private readonly Operator op;

        [MemoryPackIgnore] public string BlackboardKey => blackboardKey;
        [MemoryPackIgnore] public T Value => value;
        [MemoryPackIgnore] public Operator Operator => op;

        [MemoryPackConstructor]
        public BlackboardCondition(string blackboardKey, Operator op, T value, Stops stopsOnChange, Node decoratee) : base("BlackboardCondition", stopsOnChange, decoratee)
        {
            this.op = op;
            this.blackboardKey = blackboardKey;
            this.value = value;
        }
        
        public BlackboardCondition(string blackboardKey, Operator op, Stops stopsOnChange, Node decoratee) : base("BlackboardCondition", stopsOnChange, decoratee)
        {
            this.op = op;
            this.blackboardKey = blackboardKey;
        }
        
        protected override void StartObserving()
        {
            Blackboard.AddObserver(blackboardKey, Guid);
        }

        protected override void StopObserving()
        {
            Blackboard.RemoveObserver(blackboardKey, Guid);
        }
        
        public override void OnObservingChanged(NotifyType type, object changedValue)
        {
            Evaluate();
        }

        protected override bool IsConditionMet()
        {
            if (op == Operator.ALWAYS_TRUE)
            {
                return true;
            }

            if (!Blackboard.Isset(blackboardKey))
            {
                return op == Operator.IS_NOT_SET;
            }

            var o = Blackboard.Get<T>(blackboardKey);
            switch (this.op)
            {
                case Operator.IS_SET: return true;
                case Operator.IS_EQUAL: return o.CompareTo(value) == 0;
                case Operator.IS_NOT_EQUAL: return o.CompareTo(value) != 0;
                case Operator.IS_GREATER_OR_EQUAL: return o.CompareTo(value) >= 0;
                case Operator.IS_GREATER: return o.CompareTo(value) > 0;
                case Operator.IS_SMALLER_OR_EQUAL: return o.CompareTo(value) <= 0;
                case Operator.IS_SMALLER: return o.CompareTo(value) < 0;
                default: return false;
            }
        }

        public override string ToString()
        {
            return "(" + this.op + ") " + this.blackboardKey + " ? " + this.value;
        }
    }
}