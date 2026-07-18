using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    /// <summary>
    /// 音效和特效默认只能在本地播放，若需要在全局播放，推荐使用该组件
    /// </summary>
	public class AudioAndEffectPlayGlobal : NetworkBehaviour
    {
        private readonly System.Collections.Generic.Dictionary<string, AudioClip> m_Audios = new();
        private readonly System.Collections.Generic.Dictionary<string, GameObject> m_Effects = new();

        private readonly System.Collections.Generic.Dictionary<string, AudioClip> m_LoopAudio = new();
        private readonly System.Collections.Generic.Dictionary<AudioClip, AudioPlayer> m_LoopPlayer = new();

        public void AddAudio(string name, AudioClip audioClip) => m_Audios[name] = audioClip;
        public void AddEffect(string name, GameObject effect) => m_Effects[name] = effect;

        public void LoopAudio(string name, AudioClip audioClip) => m_LoopAudio[name] = audioClip;

        public void Play(string name, float duration, Vector3 position, Quaternion rotation, bool bindSelf = false)
        {
            if (IsServer) {
                PlayAudioAndEffectClientRpc(name, duration, position, rotation, bindSelf);
            } else {
                PlayAudioAndEffectServerRpc(name, duration, position, rotation, bindSelf);
            }
        }

        public void StopLoopAudio(string name)
        {
            if (IsServer) {
                StopLoopAudioClientRpc(name);
            } else if (IsOwner) {
                StopLoopAudioServerRpc(name);
            }
        }

        [ServerRpc]
        private void StopLoopAudioServerRpc(string name) => StopLoopAudioClientRpc(name);

        [ClientRpc]
        private void StopLoopAudioClientRpc(string name)
        {
            if (m_LoopAudio.TryGetValue(name, out AudioClip audioClip)) {
                if (m_LoopPlayer.TryGetValue(audioClip, out AudioPlayer player)) {
                    // 或许可以只置空
                    player.AudioSource.loop = false;
                    player.Release();
                    m_LoopPlayer.Remove(audioClip);
                }
            }
        }

        [ServerRpc]
        public void PlayAudioAndEffectServerRpc(string name, float duration, Vector3 position, Quaternion rotation, bool bindSelf)
            => PlayAudioAndEffectClientRpc(name, duration, position, rotation, bindSelf);

        [ClientRpc]
        public void PlayAudioAndEffectClientRpc(string name, float duration, Vector3 position, Quaternion rotation, bool bindSelf)
        {
            if (m_Audios.TryGetValue(name, out var audio)) {
                Director.Instance.RequestAudio(audio)
                    .WithPosition(position)
                    .WithDuration(duration)
                    .Play();
            }

            if (m_Effects.TryGetValue(name, out var effect)) {
                Director.Instance.RequestEffect(effect)
                    .WithPosition(position)
                    .WithRotation(rotation)
                    .WithDuration(duration)
                    .Create();
            }
            if (m_LoopAudio.TryGetValue(name, out var loopAudio)) {
                if (!m_LoopPlayer.ContainsKey(loopAudio)) {
                    var player = Director.Instance.Borrow();
                    if (bindSelf) {
                        player.transform.SetParent(transform, false);
                        player.transform.SetLocalPositionAndRotation(position, rotation);
                    } else {
                        player.transform.SetPositionAndRotation(position, rotation);
                    }
                    player.AudioSource.loop = true;
                    m_LoopPlayer.Add(loopAudio, player);
                    // 绝大部分使用该组件播放的都是音效
                    player.Play(loopAudio, Director.Instance.GetGroup(2), float.PositiveInfinity);
                }
            }
        }

        public void Clear()
        {
            foreach (var player in m_LoopPlayer) {
                player.Value.Release();
            }
            m_LoopPlayer.Clear();
            m_Audios.Clear();
            m_Effects.Clear();
            m_LoopAudio.Clear();
        }
    }
}
