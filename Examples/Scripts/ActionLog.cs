using UnityEngine;

namespace NPBehave.Examples
{
    public class ActionLog : Action
    {
        private readonly string text;
        
        public ActionLog(string text)
        {
            this.text = text;
        }
        
        protected override bool OnAction()
        {
            Debug.Log(text);
            return true;
        }
    }
}