using System.Collections.Generic;
using UnityEngine;

namespace TPSDemo.UI
{
	public class QuestTrackerUI: MonoBehaviour
	{
        public QuestTrackerSlotUI QuestPrefab;
        public Transform QuestRoot;
        private readonly List<QuestTrackerSlotUI> m_Slots = new();
		private void OnEnable()
		{
            EventManager.AddListener<Event.QuestUpdateEvent>(OnQuestUpdate);
		}

        private void OnDisable()
        {
            EventManager.RemoveListener<Event.QuestUpdateEvent>(OnQuestUpdate);
        }

        private void OnQuestUpdate(Event.QuestUpdateEvent evt)
        {
            var processList = PlayerDataProxy.Instance.GetQuestProcesses();
            ResizeSlotCount(processList.Count);

            for (int i = 0; i < processList.Count; i++) {
                var processes = processList[i];
                m_Slots[i].UpdateQuestInfo(processes);
            }
        }

        private void ResizeSlotCount(int newSlotCount)
        {
            int slotCount = m_Slots.Count;
            for (int i = slotCount; i < newSlotCount; i++) {
                var slot = Instantiate(QuestPrefab, QuestRoot);
                m_Slots.Add(slot);
            }
            if (newSlotCount < slotCount) {
                for (int i = newSlotCount; i < slotCount; i++) {
                    Destroy(m_Slots[newSlotCount].gameObject);
                }
                m_Slots.RemoveRange(newSlotCount, slotCount - newSlotCount);
            }
        }
	}
}
