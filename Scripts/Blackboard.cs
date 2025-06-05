using System.Collections.Generic;
using MemoryPack;

namespace NPBehave
{
    public enum NotifyType
    {
        ADD,
        REMOVE,
        CHANGE
    }
    
    [MemoryPackable(GenerateType.CircularReference)]
    public partial class Blackboard : Receiver
    {
        private struct Notification
        {
            public string key;
            public NotifyType type;
            public object value;
            public Notification(string key, NotifyType type, object value)
            {
                this.key = key;
                this.type = type;
                this.value = value;
            }
        }

        private Clock clock;

        [MemoryPackInclude, MemoryPackOrder(1)] private Blackboard parent;
        [MemoryPackInclude, MemoryPackOrder(2)] private HashSet<Blackboard> children = new HashSet<Blackboard>();
        [MemoryPackInclude, MemoryPackOrder(3)] private Dictionary<string, object> data = new Dictionary<string, object>();
        
        [MemoryPackInclude, MemoryPackOrder(4)] private bool isNotifying = false;
        [MemoryPackInclude, MemoryPackOrder(5)] private List<Notification> notifications = new List<Notification>();
        [MemoryPackInclude, MemoryPackOrder(6)] private List<Notification> notificationsDispatch = new List<Notification>();
        
        [MemoryPackInclude, MemoryPackOrder(7)] private Dictionary<string, List<int>> observers = new Dictionary<string, List<int>>();
        [MemoryPackInclude, MemoryPackOrder(8)] private Dictionary<string, List<int>> addObservers = new Dictionary<string, List<int>>();
        [MemoryPackInclude, MemoryPackOrder(9)] private Dictionary<string, List<int>> removeObservers = new Dictionary<string, List<int>>();

        [MemoryPackInclude, MemoryPackOrder(10)] private int currentGuid = 0;
        [MemoryPackIgnore] public readonly Dictionary<int, Receiver> IdNodeMapping = new Dictionary<int, Receiver>();

        [MemoryPackConstructor]
        private Blackboard()
        {
        }

        [MemoryPackOnDeserialized]
        private void OnDeserialized()
        {
            IdNodeMapping.Add(this.Guid, this);
        }
        
        public Blackboard(Blackboard parent, Clock clock)
        {
            this.clock = clock;
            this.parent = parent;
            this.Guid = GetNextGuid();
            IdNodeMapping.Add(this.Guid, this);
        }
        public Blackboard(Clock clock)
        {
            this.clock = clock;
            this.parent = null;
            this.Guid = GetNextGuid();
            IdNodeMapping.Add(this.Guid, this);
        }

        public int GetNextGuid()
        {
            return ++currentGuid;
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
                    this.notifications.Add(new Notification(key, NotifyType.ADD, value));
                    this.clock.AddTimer(0f, 0, NotifyObservers);
                }
                else
                {
                    if ((this.data[key] == null && value != null) || (this.data[key] != null && !this.data[key].Equals(value)))
                    {
                        this.data[key] = value;
                        this.notifications.Add(new Notification(key, NotifyType.CHANGE, value));
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
                this.notifications.Add(new Notification(key, NotifyType.REMOVE, null));
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

        public void AddObserver(string key, int observer)
        {
            var keyObservers = GetKeyObservers(observers, key);
            if (!isNotifying)
            {
                if (!keyObservers.Contains(observer))
                {
                    keyObservers.Add(observer);
                }
            }
            else
            {
                if (!keyObservers.Contains(observer))
                {
                    var keyAddObservers = GetKeyObservers(addObservers, key);
                    if (!keyAddObservers.Contains(observer))
                    {
                        keyAddObservers.Add(observer);
                    }
                }

                var keyRemoveObservers = GetKeyObservers(removeObservers, key);
                if (keyRemoveObservers.Contains(observer))
                {
                    keyRemoveObservers.Remove(observer);
                }
            }
        }

        public void RemoveObserver(string key, int observer)
        {
            var keyObservers = GetKeyObservers(observers, key);
            if (!isNotifying)
            {
                if (keyObservers.Contains(observer))
                {
                    keyObservers.Remove(observer);
                }
            }
            else
            {
                var keyRemoveObservers = GetKeyObservers(removeObservers, key);
                if (!keyRemoveObservers.Contains(observer))
                {
                    if (keyObservers.Contains(observer))
                    {
                        keyRemoveObservers.Add(observer);
                    }
                }

                var keyAddObservers = GetKeyObservers(addObservers, key);
                if (keyAddObservers.Contains(observer))
                {
                    keyAddObservers.Remove(observer);
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
            foreach (var child in children)
            {
                child.notifications.AddRange(notifications);
                child.clock.AddTimer(0f, 0, child.NotifyObservers);
            }
            notifications.Clear();

            isNotifying = true;
            foreach (var notification in notificationsDispatch)
            {
                if (!observers.ContainsKey(notification.key))
                {
                    continue;
                }

                var keyObservers = GetKeyObservers(observers, notification.key);
                foreach (var observer in keyObservers)
                {
                    if (removeObservers.ContainsKey(notification.key) && removeObservers[notification.key].Contains(observer))
                    {
                        continue;
                    }
                    IdNodeMapping[observer].OnObservingChanged(notification.type, notification.value);
                }
            }

            foreach (var key in addObservers.Keys)
            {
                GetKeyObservers(observers, key).AddRange(addObservers[key]);
            }
            foreach (var key in removeObservers.Keys)
            {
                foreach (var action in removeObservers[key])
                {
                    GetKeyObservers(observers, key).Remove(action);
                }
            }
            addObservers.Clear();
            removeObservers.Clear();

            isNotifying = false;
        }

        private List<int> GetKeyObservers(Dictionary<string, List<int>> target, string key)
        {
            List<int> keyObservers;
            if (target.ContainsKey(key))
            {
                keyObservers = target[key];
            }
            else
            {
                keyObservers = new List<int>();
                target[key] = keyObservers;
            }
            return keyObservers;
        }
    }
}
