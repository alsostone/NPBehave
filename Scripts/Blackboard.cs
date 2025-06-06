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
    
    [MemoryPackable]
    public partial class Blackboard : Receiver
    {
        private struct Notification
        {
            public readonly string key;
            public readonly NotifyType type;
            public readonly object value;

            public Notification(string key, NotifyType type, object value)
            {
                this.key = key;
                this.type = type;
                this.value = value;
            }
        }

        [MemoryPackInclude] private int parentGuid;
        [MemoryPackInclude] private HashSet<int> children = new HashSet<int>();
        [MemoryPackInclude] private Dictionary<string, object> data = new Dictionary<string, object>();
        
        [MemoryPackInclude] private bool isNotifying = false;
        [MemoryPackInclude] private List<Notification> notifications = new List<Notification>();
        [MemoryPackInclude] private List<Notification> notificationsDispatch = new List<Notification>();
        
        [MemoryPackInclude] private Dictionary<string, List<int>> observers = new Dictionary<string, List<int>>();
        [MemoryPackInclude] private Dictionary<string, List<int>> addObservers = new Dictionary<string, List<int>>();
        [MemoryPackInclude] private Dictionary<string, List<int>> removeObservers = new Dictionary<string, List<int>>();
        
        [MemoryPackIgnore] private BehaveWorld behaveWorld;
        [MemoryPackIgnore] private Blackboard parent;
        [MemoryPackIgnore] private Clock clock;

        [MemoryPackConstructor]
        private Blackboard() { }

        internal Blackboard(Blackboard parent)
        {
            parentGuid = parent?.Guid ?? 0;
        }

        internal void Set(BehaveWorld world)
        {
            behaveWorld = world;
            
            // 注册到黑板的意义：通过Guid找到该节点，后调用该节点的方法
            if (Guid < 0)
                Guid = world.GetNextGuid();
            world.IdNodeMapping.Add(Guid, this);
            
            parent = world.GetBlackboard(parentGuid);
            clock = world.Clock;
        }

        public void Enable()
        {
            if (parent != null)
            {
                parent.children.Add(Guid);
            }
        }

        public void Disable()
        {
            if (parent != null)
            {
                parent.children.Remove(Guid);
            }
            if (clock != null)
            {
                clock.RemoveTimer(Guid);
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
            if (parent != null && parent.Isset(key))
            {
                parent.Set(key, value);
            }
            else
            {
                if (!data.ContainsKey(key))
                {
                    data[key] = value;
                    notifications.Add(new Notification(key, NotifyType.ADD, value));
                    clock.AddTimer(0f, 0, Guid);
                }
                else
                {
                    if ((data[key] == null && value != null) || (data[key] != null && !data[key].Equals(value)))
                    {
                        data[key] = value;
                        notifications.Add(new Notification(key, NotifyType.CHANGE, value));
                        clock.AddTimer(0f, 0, Guid);
                    }
                }
            }
        }

        public void Unset(string key)
        {
            if (data.ContainsKey(key))
            {
                data.Remove(key);
                notifications.Add(new Notification(key, NotifyType.REMOVE, null));
                clock.AddTimer(0f, 0, Guid);
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
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            else if (parent != null)
            {
                return parent.Get(key);
            }
            else
            {
                return null;
            }
        }

        public bool Isset(string key)
        {
            return data.ContainsKey(key) || (parent != null && parent.Isset(key));
        }

#if UNITY_EDITOR
        [MemoryPackIgnore] public List<string> Keys
        {
            get
            {
                if (parent != null)
                {
                    List<string> keys = parent.Keys;
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
        
        public override void OnTimerReached()
        {
            if (notifications.Count == 0)
            {
                return;
            }

            notificationsDispatch.Clear();
            notificationsDispatch.AddRange(notifications);
            foreach (var child in children)
            {
                var childBlackboard = behaveWorld.GetBlackboard(child);
                childBlackboard.notifications.AddRange(notifications);
                childBlackboard.clock.AddTimer(0f, 0, child);
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
                    behaveWorld.IdNodeMapping[observer].OnObservingChanged(notification.type, notification.value);
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

    }
}
