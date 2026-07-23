using UnityEngine;

namespace TPSDemo
{

    public interface IInteractive
    {
        //public void Interact(GameObject obj);
        public float HoldDuration { get; }  // 0 = 瞬间完成, >0 = 需要长按秒数
        public string Hint { get; }
        public void OnInteractPress(GameObject interactor);
        public void OnInteractHold(GameObject interactor); // 按下保持的时间
        public void OnInteractRelease(GameObject interactor, bool completed); // completed=true表示按满
    }
}
