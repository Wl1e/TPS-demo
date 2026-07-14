using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace TPSDemo.UI
{
    public class LoadingScreenManager : MonoBehaviour
    {
        public TextMeshProUGUI m_ProgressText;
        public Slider m_Progress;

        void Awake()
        {
            EventManager.AddListener<Event.MapLoadProgressEvent>(OnSceneEvent);
            Hide();
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<Event.MapLoadProgressEvent>(OnSceneEvent);
        }

        void OnSceneEvent(Event.MapLoadProgressEvent evt)
        {
            if(evt.IsCompleted) {
                Hide();
            } else {
                Show();
            }
            SetProgress(evt.Progress);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            SetProgress(0f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            m_Progress.value = progress;
            m_ProgressText.text = $"{(int)(progress * 100f)}%";
        }
    }
}
