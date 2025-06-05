using System;
using MemoryPack;

namespace NPBehave
{
    /// 接收器ID生成器 需要定期重置以使ID从0开始
    /// 常规重置时机：每次战斗开始时
    public static class ReceiverIdGenerator
    {
        private static int sCurrentGuid = 0;
        
        public static void Reset()
        {
            sCurrentGuid = 0;
        }
        
        public static int GetNextGuid()
        {
            return ++sCurrentGuid;
        }
    }
    
    /// 接收器只支持 1个Timer + 1个Observer
    /// 可不用但不可多
    public abstract class Receiver
    {
        [MemoryPackInclude, MemoryPackOrder(0)] protected int Guid = -1;

        /// 定时器到达时被调用
        /// 使用Blackboard.AddTimer注册当前节点后才能被调用
        /// Override this method
        public virtual void OnTimerReached()
        {
        }

        /// 监视的值发生变化时被调用
        /// 使用Blackboard.AddObserver注册当前节点后才能被调用
        /// Override this method
        public virtual void OnObservingChanged(NotifyType type, object changedValue)
        {
            
        }
        
        
    }
}