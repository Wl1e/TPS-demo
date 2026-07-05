using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Config/ItemData/AttachmentData", fileName = "AttachmentData")]
    public class AttachmentItemData: ItemData
    {
        [Header("组件相关")]
        [Tooltip("组件槽位")]
        public IAttachment.AttachmentSlot Slot;
	}
}
