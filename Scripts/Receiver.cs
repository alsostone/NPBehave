using MemoryPack;

namespace NPBehave
{
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