using UnityEngine;

namespace NPBehave
{
    public class UnityContext : MonoBehaviour
    {
        private static UnityContext instance = null;

        public static UnityContext GetInstance()
        {
            if (instance == null)
            {
                GameObject gameObject = new GameObject();
                gameObject.name = "~Context";
                instance = (UnityContext)gameObject.AddComponent(typeof(UnityContext));
                gameObject.isStatic = true;
#if !UNITY_EDITOR
            gameObject.hideFlags = HideFlags.HideAndDontSave;
#endif
            }
            return instance;
        }
        
        public static Blackboard GetSharedBlackboard(string key)
        {
            UnityContext context = GetInstance();
            return context.BehaveWorld.GetSharedBlackboard(key);
        }
        
        public static Blackboard CreateBlackboard(Blackboard parent = null)
        {
            UnityContext context = GetInstance();
            return context.BehaveWorld.CreateBlackboard(parent);
        }
        
        public readonly BehaveWorld BehaveWorld = new BehaveWorld();

        void Update()
        {
            BehaveWorld.Update(Time.deltaTime);
        }
    }
}