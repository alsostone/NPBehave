using System.Collections.Generic;
using MemoryPack;

namespace NPBehave
{
    [MemoryPackable(GenerateType.CircularReference)]
    public partial class Blackboard
    {
        public enum Type
        {
            ADD,
            REMOVE,
            CHANGE
        }
        private struct Notification
        {
            public string key;
            public Type type;
            public object value;
            public Notification(string key, Type type, object value)
            {
                this.key = key;
                this.type = type;
                this.value = value;
            }
        }

        private Clock clock;
        private Dictionary<string, object> data = new Dictionary<string, object>();
        private Dictionary<string, List<System.Action<Type, object>>> observers = new Dictionary<string, List<System.Action<Type, object>>>();
        private bool isNotifying = false;
        private Dictionary<string, List<System.Action<Type, object>>> addObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private Dictionary<string, List<System.Action<Type, object>>> removeObservers = new Dictionary<string, List<System.Action<Type, object>>>();
        private List<Notification> notifications = new List<Notification>();
        private List<Notification> notificationsDispatch = new List<Notification>();
        private Blackboard parent;
        private HashSet<Blackboard> children = new HashSet<Blackboard>();
        
        [MemoryPackConstructor]
        private Blackboard() {}
        
        public Blackboard(Blackboard parent, Clock clock)
        {
            this.clock = clock;
            this.parent = parent;
        }
        public Blackboard(Clock clock)
        {
            this.parent = null;
            this.clock = clock;
        }

        public void Enable()
        {
            if (this.parent != null)
            {
                this.parent.children.Add(this);
            }
        }

        public void Disable()
        {
            if (this.parent != null)
            {
                this.parent.children.Remove(this);
            }
            if (this.clock != null)
            {
                this.clock.RemoveTimer(this.NotifyObservers);
            }
        }

        public object this[string key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public void Set(string key)
        {
            if (!Isset(key))
            {
                Set(key, null);
            }
        }

        public void Set(string key, object value)
        {
            if (this.parent != null && this.parent.Isset(key))
            {
                this.parent.Set(key, value);
            }
            else
            {
                if (!this.data.ContainsKey(key))
                {
                    this.data[key] = value;
                    this.notifications.Add(new Notification(key, Type.ADD, value));
                    this.clock.AddTimer(0f, 0, NotifyObservers);
                }
                else
                {
                    if ((this.data[key] == null && value != null) || (this.data[key] != null && !this.data[key].Equals(value)))
                    {
                        this.data[key] = value;
                        this.notifications.Add(new Notification(key, Type.CHANGE, value));
                        this.clock.AddTimer(0f, 0, NotifyObservers);
                    }
                }
            }
        }

        public void Unset(string key)
        {
            if (this.data.ContainsKey(key))
            {
                this.data.Remove(key);
                this.notifications.Add(new Notification(key, Type.REMOVE, null));
                this.clock.AddTimer(0f, 0, NotifyObservers);
            }
        }
        
        public T Get<T>(string key)
        {
            object result = Get(key);
            if (result == null)
            {
                return default(T);
            }
            return (T)result;
        }

        public object Get(string key)
        {
            if (this.data.ContainsKey(key))
            {
                return data[key];
            }
            else if (this.parent != null)
            {
                return this.parent.Get(key);
            }
            else
            {
                return null;
            }
        }

        public bool Isset(string key)
        {
            return this.data.ContainsKey(key) || (this.parent != null && this.parent.Isset(key));
        }

        public void AddObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(this.observers, key);
            if (!isNotifying)
            {
                if (!observers.Contains(observer))
                {
                    observers.Add(observer);
                }
            }
            else
            {
                if (!observers.Contains(observer))
                {
                    List<System.Action<Type, object>> addObservers = GetObserverList(this.addObservers, key);
                    if (!addObservers.Contains(observer))
                    {
                        addObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> removeObservers = GetObserverList(this.removeObservers, key);
                if (removeObservers.Contains(observer))
                {
                    removeObservers.Remove(observer);
                }
            }
        }

        public void RemoveObserver(string key, System.Action<Type, object> observer)
        {
            List<System.Action<Type, object>> observers = GetObserverList(this.observers, key);
            if (!isNotifying)
            {
                if (observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
            else
            {
                List<System.Action<Type, object>> removeObservers = GetObserverList(this.removeObservers, key);
                if (!removeObservers.Contains(observer))
                {
                    if (observers.Contains(observer))
                    {
                        removeObservers.Add(observer);
                    }
                }

                List<System.Action<Type, object>> addObservers = GetObserverList(this.addObservers, key);
                if (addObservers.Contains(observer))
                {
                    addObservers.Remove(observer);
                }
            }
        }
        
#if UNITY_EDITOR
        [MemoryPackIgnore] public List<string> Keys
        {
            get
            {
                if (this.parent != null)
                {
                    List<string> keys = this.parent.Keys;
                    keys.AddRange(data.Keys);
                    return keys;
                }
                else
                {
                    return new List<string>(data.Keys);
                }
            }
        }
        [MemoryPackIgnore] public int NumObservers
        {
            get
            {
                int count = 0;
                foreach (string key in observers.Keys)
                {
                    count += observers[key].Count;
                }
                return count;
            }
        }
#endif
        
        private void NotifyObservers()
        {
            if (notifications.Count == 0)
            {
                return;
            }

            notificationsDispatch.Clear();
            notificationsDispatch.AddRange(notifications);
            foreach (Blackboard child in children)
            {
                child.notifications.AddRange(notifications);
                child.clock.AddTimer(0f, 0, child.NotifyObservers);
            }
            notifications.Clear();

            isNotifying = true;
            foreach (Notification notification in notificationsDispatch)
            {
                if (!this.observers.ContainsKey(notification.key))
                {
                    continue;
                }

                List<System.Action<Type, object>> observers = GetObserverList(this.observers, notification.key);
                foreach (System.Action<Type, object> observer in observers)
                {
                    if (this.removeObservers.ContainsKey(notification.key) && this.removeObservers[notification.key].Contains(observer))
                    {
                        continue;
                    }
                    observer(notification.type, notification.value);
                }
            }

            foreach (string key in this.addObservers.Keys)
            {
                GetObserverList(this.observers, key).AddRange(this.addObservers[key]);
            }
            foreach (string key in this.removeObservers.Keys)
            {
                foreach (System.Action<Type, object> action in removeObservers[key])
                {
                    GetObserverList(this.observers, key).Remove(action);
                }
            }
            this.addObservers.Clear();
            this.removeObservers.Clear();

            isNotifying = false;
        }

        private List<System.Action<Type, object>> GetObserverList(Dictionary<string, List<System.Action<Type, object>>> target, string key)
        {
            List<System.Action<Type, object>> observers;
            if (target.ContainsKey(key))
            {
                observers = target[key];
            }
            else
            {
                observers = new List<System.Action<Type, object>>();
                target[key] = observers;
            }
            return observers;
        }
    }
}
