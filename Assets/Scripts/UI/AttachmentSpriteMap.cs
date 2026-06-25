using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo
{
    [CreateAssetMenu(menuName = "Other/AttachmentSpriteMap", fileName = "AttachmentSpriteMap")]
    public class AttachmentSpriteMap: GameResource
    {
        [Serializable]
        public struct SpriteEntry
        {
            [Tooltip("配件 ItemData")]
            public ItemData Attachment;
            [Tooltip("该配件的展示图")]
            public Sprite Sprite;
        }

        [Serializable]
        public struct WeaponOverride
        {
            [Tooltip("武器 ItemData")]
            public ItemData Weapon;
            [Tooltip("该武器上各配件的展示图")]
            public List<SpriteEntry> Sprites;
        }

        [Tooltip("每个武器的配件展示图映射")]
        [SerializeField] List<WeaponOverride> m_Weapons;

        /// <summary>获取指定武器上指定配件的展示图，无配置则兜底到配件的 ItemData.Icon</summary>
        public Sprite GetSprite(ItemData weapon, ItemData attachment)
        {
            if (weapon == null || attachment == null)
                return null;

            foreach (var wo in m_Weapons) {
                if (wo.Weapon?.Id != weapon.Id) {
                    continue;
                }
                foreach (var se in wo.Sprites) {
                    if (se.Attachment?.Id == attachment.Id && se.Sprite != null) {
                        return se.Sprite;
                    }
                }
                break; // 找到武器了，没找到匹配的 Sprite → 退出循环走兜底
            }
            return attachment.Icon; // 兜底
        }
    }
}
