using UnityEngine;

namespace NPBehave.Examples
{
    public class ActionTowards : ActionRequest
    {
        private readonly Transform transform;

        public ActionTowards(Transform transform)
        {
            this.transform = transform;
        }
        
        protected override Result OnAction(Request request)
        {
            switch (request)
            {
                case Request.START:
                    return Result.PROGRESS;
                case Request.UPDATE:
                {
                    Vector3 pos = Blackboard.Get<Vector3>("playerLocalPos");
                    transform.localPosition += pos * (0.5f * Time.deltaTime);
                    return Result.PROGRESS;
                }
                case Request.CANCEL:
                    return Result.SUCCESS;
            }
            return Result.FAILED;
        }
    }
}