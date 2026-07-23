using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TPSDemo
{
    // 比AudioAndEffectPlayGlobal更好
    // 但是
    // 1. 目前不好管理audio和effect的生命周期
    // 2. 如果音效和特效过多会导致拥挤
    // 3. 不好选择Key（可以配置一个SO来设置Id到音效和特效的映射）
    public class NetworkEffectService: NetworkSingleton<NetworkEffectService>
	{
        private readonly Dictionary<string, AudioClip> m_Audios = new();
        private readonly Dictionary<string, GameObject> m_Effects = new();

        private readonly Dictionary<string, AudioClip> m_LoopAudio = new();
        private readonly Dictionary<string, AudioPlayer> m_LoopPlayer = new();

        // 使用audio和effect的name做key很不稳定，但是目前这样做最简单
        public string AddAudio(AudioClip audio)
        {
            m_Audios.TryAdd(audio.name, audio);
            return audio.name;
        }

        public string AddEffect(GameObject effect)
        {
            m_Effects.TryAdd(effect.name, effect);
            return effect.name;
        }

        public string LoopAudio(AudioClip audio)
        {
            m_LoopAudio.TryAdd(audio.name, audio);
            return audio.name;
        }

        //public string AddEffect(AssetReference effect)
        //{
        //    m_Effects.TryAdd(effect.AssetGUID, effect);
        //    return effect.AssetGUID;
        //}

        //public string LoopAudio(AssetReference audioClip)
        //{
        //    m_LoopAudio.TryAdd(audioClip.AssetGUID, audioClip);
        //    return audioClip.AssetGUID;
        //}

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
            if (m_LoopAudio.TryGetValue(name, out AudioClip audioRef)) {
                if (m_LoopPlayer.TryGetValue(name, out AudioPlayer player)) {
                    // 或许可以只置空
                    player.AudioSource.loop = false;
                    player.Release();
                    m_LoopPlayer.Remove(name);
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
                    .WithMixerGroup(AudioSystem.AudioGroup.SFX)
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
                if (!m_LoopPlayer.ContainsKey(name)) {
                    var player = Director.Instance.Borrow();
                    if (bindSelf) {
                        player.transform.SetParent(transform, false);
                        player.transform.SetLocalPositionAndRotation(position, rotation);
                    } else {
                        player.transform.SetPositionAndRotation(position, rotation);
                    }
                    player.AudioSource.loop = true;
                    m_LoopPlayer.Add(name, player);
                    player.Play(loopAudio,
                        Director.Instance.GetGroup(AudioSystem.AudioGroup.SFX),
                        float.PositiveInfinity
                    );
                }
            }
        }
    }
}
