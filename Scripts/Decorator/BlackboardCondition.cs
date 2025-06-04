
using System;

namespace NPBehave
{
    public class BlackboardCondition<T> : ObservingDecorator where T : IComparable<T>
    {
        private string key;
        private T value;
        private Operator op;

        public string Key
        {
            get
            {
                return key;
            }
        }

        public T Value
        {
            get
            {
                return value;
            }
        }

        public Operator Operator
        {
            get
            {
                return op;
            }
        }

        public BlackboardCondition(string key, Operator op, T value, Stops stopsOnChange, Node decoratee) : base("BlackboardCondition", stopsOnChange, decoratee)
        {
            this.op = op;
            this.key = key;
            this.value = value;
            this.stopsOnChange = stopsOnChange;
        }
        
        public BlackboardCondition(string key, Operator op, Stops stopsOnChange, Node decoratee) : base("BlackboardCondition", stopsOnChange, decoratee)
        {
            this.op = op;
            this.key = key;
            this.stopsOnChange = stopsOnChange;
        }


        protected override void StartObserving()
        {
            Blackboard.AddObserver(key, onValueChanged);
        }

        protected override void StopObserving()
        {
            Blackboard.RemoveObserver(key, onValueChanged);
        }

        private void onValueChanged(Blackboard.Type type, object newValue)
        {
            Evaluate();
        }

        protected override bool IsConditionMet()
        {
            if (op == Operator.ALWAYS_TRUE)
            {
                return true;
            }

            if (!Blackboard.Isset(key))
            {
                return op == Operator.IS_NOT_SET;
            }

            var o = Blackboard.Get<T>(key);
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
            return "(" + this.op + ") " + this.key + " ? " + this.value;
        }
    }
}