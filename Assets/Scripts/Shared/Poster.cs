using UnityEngine;

namespace TPSDemo
{
    public class Poster: MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_Shower;
        private Texture2D m_Texture2D;
        public string ImageName;

        private void Awake()
        {
            UpdateImage();
        }

        private void OnEnable()
        {
            EventManager.AddListener<Event.AssetUpdateEvent>(OnPostUpdate);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Event.AssetUpdateEvent>(OnPostUpdate);
        }

        public void SetImage(Sprite sprite) => m_Shower.sprite = sprite;

        private void UpdateImage()
        {
            StartCoroutine(AssetCache.GetOrLoad<Texture2D>(ImageName,
                texture => {
                    if(!texture) {
                        Debug.LogError($"不存在{ImageName}图像");
                        return;
                    }
                    m_Texture2D = texture;
                    SetImage(Sprite.Create(
                        m_Texture2D,
                        new Rect(0, 0, m_Texture2D.width, m_Texture2D.height),
                        Vector2.one / 2)
                    );
                }
            ));
        }

        private void OnPostUpdate(Event.AssetUpdateEvent evt)
        {
            if(!evt.Keys.Contains(ImageName)) {
                return;
            }
            UpdateImage();
        }
    }
}
