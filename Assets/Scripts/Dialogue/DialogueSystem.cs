using System;
using UnityEngine;

namespace TPSDemo
{
using Event;

    public enum DialogState
    {
        None,
        Active,
        Interrupted,
        Completed
    }

    public class DialogueSystem : Singleton<DialogueSystem>
    {
        DialogueData m_Data;
        int m_CurrentNodeId;
        DialogState m_State = DialogState.None;
        public bool IsCompleted() => m_State == DialogState.Completed;

        PlayerController m_CurrentPlayer;
        NpcBase m_Npc;

        private void OnEnable()
        {
            EventManager.AddListener<SelectOptionEvent>(SelectOption);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<SelectOptionEvent>(SelectOption);
        }

        public void SelectOption(SelectOptionEvent evt)
        {
            int optionId = evt.Option;
            if (GetCurrentNode().Options.Count <= optionId || optionId < 0) {
                Debug.LogError("err optionId: " + optionId);
                return;
            }
            var option = GetCurrentNode().Options[optionId];
            HandleAction(option);
        }

        void HandleAction(OptionEntry option)
        {
            var action = option.Action;
            if (action == OptionAction.Complete) {
                Complete();
                return;
            } else if (action == OptionAction.Interrupt) {
                Interrupt();
                return;
            } else if (action == OptionAction.Goto) {
                int nextNodeId = option.GotoIdx;
                if (!ValidNodeId(nextNodeId)) {
                    Debug.LogError("err node idx");
                    return;
                }
                ChangeNode(nextNodeId);
            } else if (option.Action == OptionAction.AcceptQuest) {
            } else if (option.Action == OptionAction.OpenShop) {
                EventManager.Broadcast(
                    new OpenShopEvent {
                        playerId = m_CurrentPlayer.ID,
                        ShopId = int.Parse(option.ActionData)
                    }
                );
                Interrupt();
            } else {
                Debug.LogError("Err Action: " + action.ToString());
            }
        }

        public bool ValidNodeId(int nodeId)
        { return m_Data.Nodes.Count > nodeId && nodeId >= 0; }

        public DialogueNode GetCurrentNode() => m_Data.Nodes[m_CurrentNodeId];
        public void ChangeNode(int NodeId)
        {
            m_CurrentNodeId = NodeId;
            EventManager.Broadcast(new UpdateDialogEvent { DialogueNode = GetCurrentNode() });
        }

        void ChangeState(DialogState state)
        {
            m_State = state;
            if (state == DialogState.Active) {
                EventManager.Broadcast(new StartDialogEvent { Npc = m_Npc, PlayerId = m_CurrentPlayer.ID });
            } else if (state == DialogState.Interrupted || state == DialogState.Completed) {
                m_CurrentPlayer.SetInputActive(true, true);
                m_Npc.StopChat();
                EventManager.Broadcast(new EndDialogEvent { State = m_State });
            }
        }

        public void Complete() => ChangeState(DialogState.Completed);

        public void Interrupt() => ChangeState(DialogState.Interrupted);

        public void Enter(PlayerController player, NpcBase npc)
        {
            if (m_State == DialogState.Completed) {
                return;
            }

            m_CurrentPlayer = player;
            m_Npc = npc;

            m_Data = m_Npc.DialogueData;
            m_CurrentPlayer.SetInputActive(false, false);

            ChangeState(DialogState.Active);
            ChangeNode(m_Data.InitializeIdx);
        }
    }
}
