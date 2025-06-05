using System.Collections.Generic;
using MemoryPack;

namespace NPBehave
{
    [MemoryPackable]
    public partial class Clock
    {
        private class Timer
        {
            public double scheduledTime = 0f;
            public int repeat = 0;
            public double delay = 0f;
            public float randomVariance = 0.0f;

            public void ScheduleAbsoluteTime(double elapsedTime)
            {
                scheduledTime = elapsedTime + delay - randomVariance * 0.5f + randomVariance * UnityEngine.Random.value;
            }
        }
        [MemoryPackInclude] private double elapsedTime = 0f;
        [MemoryPackInclude] private bool isInUpdate = false;
        
        [MemoryPackInclude] private Dictionary<int, Timer> timers = new Dictionary<int, Timer>();
        [MemoryPackInclude] private Dictionary<int, Timer> addTimers = new Dictionary<int, Timer>();
        [MemoryPackInclude] private HashSet<int> removeTimers = new HashSet<int>();
        
        [MemoryPackInclude] private HashSet<int> updateObservers = new HashSet<int>();
        [MemoryPackInclude] private HashSet<int> addObservers = new HashSet<int>();
        [MemoryPackInclude] private HashSet<int> removeObservers = new HashSet<int>();
        
        [MemoryPackIgnore] public readonly Dictionary<int, Receiver> IdNodeMapping = new Dictionary<int, Receiver>();
        [MemoryPackIgnore] private readonly Queue<Timer> timerPool = new Queue<Timer>();
        
        /// <summary>Register a timer function</summary>
        /// <param name="time">time in milliseconds</param>
        /// <param name="repeat">number of times to repeat, set to -1 to repeat until unregistered.</param>
        /// <param name="action">method to invoke</param>
        public void AddTimer(float time, int repeat, int action)
        {
            AddTimer(time, 0f, repeat, action);
        }

        /// <summary>Register a timer function with random variance</summary>
        /// <param name="delay">time in milliseconds</param>
        /// <param name="randomVariance">deviate from time on a random basis</param>
        /// <param name="repeat">number of times to repeat, set to -1 to repeat until unregistered.</param>
        /// <param name="action">method to invoke</param>
        public void AddTimer(float delay, float randomVariance, int repeat, int action)
        {
            Timer timer = null;

            if (!isInUpdate)
            {
                if (!this.timers.ContainsKey(action))
                {
					this.timers[action] = GetTimerFromPool();
                }
				timer = this.timers[action];
            }
            else
            {
                if (!this.addTimers.ContainsKey(action))
                {
					this.addTimers[action] = GetTimerFromPool();
                }
				timer = this.addTimers [action];

                if (this.removeTimers.Contains(action))
                {
                    this.removeTimers.Remove(action);
                }
            }

			timer.delay = delay;
			timer.randomVariance = randomVariance;
			timer.repeat = repeat;
			timer.ScheduleAbsoluteTime(elapsedTime);
        }

        public void RemoveTimer(int action)
        {
            if (!isInUpdate)
            {
                if (this.timers.ContainsKey(action))
                {
                    timerPool.Enqueue(timers[action]);
                    this.timers.Remove(action);
                }
            }
            else
            {
                if (this.timers.ContainsKey(action))
                {
                    this.removeTimers.Add(action);
                }
                if (this.addTimers.ContainsKey(action))
                {
                    timerPool.Enqueue(addTimers[action]);
                    this.addTimers.Remove(action);
                }
            }
        }

        public bool HasTimer(int action)
        {
            if (!isInUpdate)
            {
                return this.timers.ContainsKey(action);
            }
            else
            {
                if (this.removeTimers.Contains(action))
                {
                    return false;
                }
                else if (this.addTimers.ContainsKey(action))
                {
                    return true;
                }
                else
                {
                    return this.timers.ContainsKey(action);
                }
            }
        }

        /// <summary>Register a function that is called every frame</summary>
        /// <param name="action">function to invoke</param>
        public void AddUpdateObserver(int action)
        {
            if (!isInUpdate)
            {
                this.updateObservers.Add(action);
            }
            else
            {
                if (!this.updateObservers.Contains(action))
                {
                    this.addObservers.Add(action);
                }
                if (this.removeObservers.Contains(action))
                {
                    this.removeObservers.Remove(action);
                }
            }
        }

        public void RemoveUpdateObserver(int action)
        {
            if (!isInUpdate)
            {
                this.updateObservers.Remove(action);
            }
            else
            {
                if (this.updateObservers.Contains(action))
                {
                    this.removeObservers.Add(action);
                }
                if (this.addObservers.Contains(action))
                {
                    this.addObservers.Remove(action);
                }
            }
        }

        public bool HasUpdateObserver(int action)
        {
            if (!isInUpdate)
            {
                return this.updateObservers.Contains(action);
            }
            else
            {
                if (this.removeObservers.Contains(action))
                {
                    return false;
                }
                else if (this.addObservers.Contains(action))
                {
                    return true;
                }
                else
                {
                    return this.updateObservers.Contains(action);
                }
            }
        }

        public void Update(float deltaTime)
        {
            this.elapsedTime += deltaTime;
            this.isInUpdate = true;

            foreach (var action in updateObservers)
            {
                if (!removeObservers.Contains(action))
                {
                    IdNodeMapping[action].OnTimerReached();
                }
            }

            var keys = timers.Keys;
			foreach (var callback in keys)
            {
                if (this.removeTimers.Contains(callback))
                {
                    continue;
                }

				Timer timer = timers[callback];
                if (timer.scheduledTime <= this.elapsedTime)
                {
                    if (timer.repeat == 0)
                    {
                        RemoveTimer(callback);
                    }
                    else if (timer.repeat >= 0)
                    {
                        timer.repeat--;
                    }
                    
                    IdNodeMapping[callback].OnTimerReached();
					timer.ScheduleAbsoluteTime(elapsedTime);
                }
            }

            foreach (var action in this.addObservers)
            {
                this.updateObservers.Add(action);
            }
            foreach (var action in this.removeObservers)
            {
                this.updateObservers.Remove(action);
            }
            foreach (var action in this.addTimers.Keys)
            {
                if (this.timers.TryGetValue(action, out var timer))
                {
                    timerPool.Enqueue(timer);
                }
                this.timers[action] = this.addTimers[action];
            }
            foreach (var action in this.removeTimers)
            {
                timerPool.Enqueue(timers[action]);
                this.timers.Remove(action);
            }
            this.addObservers.Clear();
            this.removeObservers.Clear();
            this.addTimers.Clear();
            this.removeTimers.Clear();

            this.isInUpdate = false;
        }
        
#if UNITY_EDITOR
        [MemoryPackIgnore] public int NumUpdateObservers => updateObservers.Count;
        [MemoryPackIgnore] public int NumTimers => timers.Count;
        [MemoryPackIgnore] public double ElapsedTime => elapsedTime;
        [MemoryPackIgnore] public int DebugPoolSize => this.timerPool.Count;
#endif
        
        private Timer GetTimerFromPool()
        {
            if (!timerPool.TryDequeue(out var timer))
            {
                timer = new Timer();
            }
            return timer;
        }

    }
}