using System.Collections.Generic;
using MemoryPack;
using UnityEngine.Playables;

namespace NPBehave
{
    public enum NotifyType
    {
        ADD,
        REMOVE,
        CHANGE
    }
    
    [MemoryPackable]
    public partial struct Notification
    {
        public readonly string key;
        public readonly NotifyType type;

        public Notification(string key, NotifyType type)
        {
            this.key = key;
            this.type = type;
        }
    }
    
    [MemoryPackable]
    public partial class Blackboard : Receiver
    {
        [MemoryPackInclude] private int parentGuid;
        [MemoryPackInclude] private HashSet<int> children = new HashSet<int>();
        [MemoryPackInclude] private Dictionary<string, bool> dataBool = new Dictionary<string, bool>();
        [MemoryPackInclude] private Dictionary<string, int> dataInt = new Dictionary<string, int>();
        [MemoryPackInclude] private Dictionary<string, float> dataFloat = new Dictionary<string, float>();
        
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
        
        #region Set 
        public void SetBool(string key, bool value)
        {
            if (parent != null && parent.IsSetBool(key))
            {
                parent.SetBool(key, value);
            }
            else
            {
                if (!dataBool.ContainsKey(key))
                {
                    dataBool[key] = value;
                    notifications.Add(new Notification(key, NotifyType.ADD));
                    clock.AddTimer(0f, 0, Guid);
                }
                else
                {
                    if (!dataBool[key].Equals(value))
                    {
                        dataBool[key] = value;
                        notifications.Add(new Notification(key, NotifyType.CHANGE));
                        clock.AddTimer(0f, 0, Guid);
                    }
                }
            }
        }
        
        public void SetInt(string key, int value)
        {
            if (parent != null && parent.IsSetInt(key))
            {
                parent.SetInt(key, value);
            }
            else
            {
                if (!dataInt.ContainsKey(key))
                {
                    dataInt[key] = value;
                    notifications.Add(new Notification(key, NotifyType.ADD));
                    clock.AddTimer(0f, 0, Guid);
                }
                else
                {
                    if (!dataInt[key].Equals(value))
                    {
                        dataInt[key] = value;
                        notifications.Add(new Notification(key, NotifyType.CHANGE));
                        clock.AddTimer(0f, 0, Guid);
                    }
                }
            }
        }
        
        public void SetFloat(string key, float value)
        {
            if (parent != null && parent.IsSetFloat(key))
            {
                parent.SetFloat(key, value);
            }
            else
            {
                if (!dataFloat.ContainsKey(key))
                {
                    dataFloat[key] = value;
                    notifications.Add(new Notification(key, NotifyType.ADD));
                    clock.AddTimer(0f, 0, Guid);
                }
                else
                {
                    if (!dataFloat[key].Equals(value))
                    {
                        dataFloat[key] = value;
                        notifications.Add(new Notification(key, NotifyType.CHANGE));
                        clock.AddTimer(0f, 0, Guid);
                    }
                }
            }
        }
        #endregion
        
        #region UnSet
        public void UnSetBool(string key)
        {
            if (dataBool.ContainsKey(key))
            {
                dataBool.Remove(key);
                notifications.Add(new Notification(key, NotifyType.REMOVE));
                clock.AddTimer(0f, 0, Guid);
            }
        }

        public void UnSetInt(string key)
        {
            if (dataInt.ContainsKey(key))
            {
                dataInt.Remove(key);
                notifications.Add(new Notification(key, NotifyType.REMOVE));
                clock.AddTimer(0f, 0, Guid);
            }
        }
        
        public void UnSetFloat(string key)
        {
            if (dataFloat.ContainsKey(key))
            {
                dataFloat.Remove(key);
                notifications.Add(new Notification(key, NotifyType.REMOVE));
                clock.AddTimer(0f, 0, Guid);
            }
        }
        #endregion
        
        #region IsSet
        public bool IsSetBool(string key)
        {
            return dataBool.ContainsKey(key) || (parent != null && parent.IsSetBool(key));
        }
        public bool IsSetInt(string key)
        {
            return dataInt.ContainsKey(key) || (parent != null && parent.IsSetInt(key));
        }
        public bool IsSetFloat(string key)
        {
            return dataFloat.ContainsKey(key) || (parent != null && parent.IsSetFloat(key));
        }
        #endregion

        #region Get
        public bool GetBool(string key)
        {
            if (dataBool.TryGetValue(key, out var value))
            {
                return value;
            }
            if (parent != null)
            {
                return parent.GetBool(key);
            }
            return false;
        }
        public int GetInt(string key)
        {
            if (dataInt.TryGetValue(key, out var value))
            {
                return value;
            }
            if (parent != null)
            {
                return parent.GetInt(key);
            }
            return 0;
        }
        public float GetFloat(string key)
        {
            if (dataFloat.TryGetValue(key, out var value))
            {
                return value;
            }
            if (parent != null)
            {
                return parent.GetFloat(key);
            }
            return 0f;
        }
        #endregion

#if UNITY_EDITOR
        [MemoryPackIgnore] public List<string> BoolKeys
        {
            get
            {
                if (parent != null)
                {
                    List<string> keys = parent.BoolKeys;
                    keys.AddRange(dataBool.Keys);
                    return keys;
                }
                else
                {
                    return new List<string>(dataBool.Keys);
                }
            }
        }
        [MemoryPackIgnore] public List<string> IntKeys
        {
            get
            {
                if (parent != null)
                {
                    List<string> keys = parent.IntKeys;
                    keys.AddRange(dataInt.Keys);
                    return keys;
                }
                else
                {
                    return new List<string>(dataInt.Keys);
                }
            }
        }
        [MemoryPackIgnore] public List<string> FloatKeys
        {
            get
            {
                if (parent != null)
                {
                    List<string> keys = parent.FloatKeys;
                    keys.AddRange(dataFloat.Keys);
                    return keys;
                }
                else
                {
                    return new List<string>(dataFloat.Keys);
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
                    behaveWorld.IdNodeMapping[observer].OnObservingChanged(notification.type);
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
