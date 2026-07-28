using System;
using UnityEngine;

namespace TPSDemo
{
    public class BGMPlayer : MonoBehaviour
    {
        AudioPlayer m_Player = null;
        private void Start()
        {
            m_Player = Director.Instance.Borrow();
            EventManager.AddListener<Event.MapChangeEvent>(OnMapChenged);
            if(MapManager.Instance.CurrentMap) {
                var id = MapManager.Instance.CurrentMap.Config.MapId;
                Play($"BGM_Map{id}");
            }
        }

        private void OnMapChenged(Event.MapChangeEvent evt)
        {
            var mapId = evt.NewMapId;
            Play($"BGM_Map{mapId}");
        }

        private void OnDestroy()
        {
            if (m_Player) {
                m_Player.Release();
            }
        }

        public void Play(string name) => StartCoroutine(AssetCache.GetOrLoad<AudioClip>(name, SetBGM));

        private void SetBGM(AudioClip clip)
        {
            if(!clip) {
                Debug.Log("没有对应BGM");
                return;
            }
            m_Player.Play(clip, Director.Instance.GetGroup(AudioSystem.AudioGroup.BGM), float.PositiveInfinity);
            Debug.Log("播放BGM: " + name);
        }
    }
}
