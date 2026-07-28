using System;
using UnityEngine;

namespace TPSDemo.UI
{
    [Obsolete("废弃")]
    // 用于解放鼠标的特殊面板（问题，可以此时打开其他面板）
    public class EmptyPanelUI : MonoBehaviour, IPanel
    {
        void IPanel.Open()
        {
        }

        void IPanel.Close()
        {
        }
    }
}
