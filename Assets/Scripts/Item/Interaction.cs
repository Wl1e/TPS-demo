using UnityEngine;

namespace TPSDemo
{

    public interface IInteractive
    {
        //public void Interact(GameObject obj);
        float HoldDuration { get; }  // 0 = 瞬间完成, >0 = 需要长按秒数
        void OnInteractPress(GameObject interactor);
        void OnInteractHold(GameObject interactor); // 按下保持的时间
        void OnInteractRelease(GameObject interactor, bool completed); // completed=true表示按满
    }
}
