using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Windows;

namespace TPSDemo
{
	public class DebugLayer: MonoBehaviour
	{
        PlayerController m_Player = null;
        public TextMeshProUGUI StatusText;

        public bool ShowState = true;
        public bool ShowVelocity = true;
        public bool ShowClimbState = true;
        public bool ShowCombatState = true;

        public int QuestId;

        private void Start()
        {
            m_Player = PlayerDataProxy.Instance.GetPlayer();
            if(!m_Player) {
                EventManager.AddListener<Event.PlayerFinishedInitialzeEvent>(OnPlayerInit);
            }
            //StartCoroutine(TextMessageLog());
        }

        private void LateUpdate()
        {
            if (!m_Player) {
                return;
            }
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            string text = "";
            if (ShowState) {
                text += $"State: {m_Player.RuntimeData.State}\n";
            }
            if (ShowVelocity) {
                text += $"Velocity: {m_Player.Movement.Velocity}\n";
                text += $"IsGrounded: {m_Player.Movement.IsGrounded}\n";
            }
            if (ShowClimbState) {
                text += $"CanClimb: {m_Player.ClimbController.CanClimb}\nCanLedge: {m_Player.ClimbController.CanLedge}\n";
            }
            if (ShowCombatState) {
                text += $"CombatState: {m_Player.CombatController.CurrentActiveSlot.ToString()}";
            }
            StatusText.text = text;
        }

        private void OnPlayerInit(Event.PlayerFinishedInitialzeEvent evt)
        {
            EventManager.RemoveListener<Event.PlayerFinishedInitialzeEvent>(OnPlayerInit);
            m_Player = PlayerDataProxy.Instance.GetPlayer();
        }

        int id = 0;
        private IEnumerator TextMessageLog()
        {
            yield return new WaitForSeconds(0.5f);
            EventManager.Broadcast(new Event.MessageLogEvent { Message = $"Test Message {id++}", Duration = 2f });
            StartCoroutine(TextMessageLog());
        }

        //private void OnGUI()
        //{
        //    GUILayout.BeginArea(new Rect(0, 0, 100, 100));
        //    GUILayout.EndArea();
        //}
    }
}
