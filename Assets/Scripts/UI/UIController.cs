using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] HUD m_HUD;
        [SerializeField] InventoryUI m_InventoryUI;
        [SerializeField] DialogUI m_DialogUI;
        [SerializeField] LoadoutUI m_LoadoutUI;

        [SerializeField] Image Frame;

        public void Initialize()
        {
            Frame.gameObject.SetActive(false);
            m_HUD.Initialze();
            m_InventoryUI.Initialize();
            m_LoadoutUI.Initialize();
        }
    }
}
