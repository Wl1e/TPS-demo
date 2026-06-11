using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TPSDemo.UI
{
	public class QuestTrackerUI: MonoBehaviour
	{
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
            // ...
        }
	}
}
