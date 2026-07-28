using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace TPSDemo.UI
{
	public class QuestTrackerSlotUI: MonoBehaviour
	{
        public ObjectiveProcessUI ObjectiveProcessPrefab;
        private QuestDatabase m_QuestDB => ResourceManager.Instance.GetResource<QuestDatabase>("Quest");
        private ObjectiveDatabase m_ObjectiveDB => ResourceManager.Instance.GetResource<ObjectiveDatabase>("Objective");

        public Image BackGround;
        public TextMeshProUGUI QuestName;
        public TextMeshProUGUI QuestDescription;
        public Transform ObjectiveRoot;
        public List<ObjectiveProcessUI> Objectives = new();

        public void UpdateQuestInfo(QuestProcess process)
        {
            int questId = process.Id;
            var questConfig = m_QuestDB.GetQuestConfig(questId);
            QuestName.text = questConfig.QuestName;
            QuestDescription.text = questConfig.QuestDescription;
            UpdateObjective(0, process.Obj0);
            UpdateObjective(1, process.Obj1);
            UpdateObjective(2, process.Obj2);
            UpdateObjective(3, process.Obj3);
            //Debug.Log($"UpdateQuest {questId}, Obj0:{JsonUtility.ToJson(process.Obj0)}\n" +
            //    $"Obj1:{JsonUtility.ToJson(process.Obj1)}\n" +
            //    $"Obj2:{JsonUtility.ToJson(process.Obj2)}\n" +
            //    $"Obj3:{JsonUtility.ToJson(process.Obj3)}\n");
        }

        private void UpdateObjective(int idx, ObjectiveProgress process)
        {
            if(process.IsEmpty) {
                return;
            }
            while(Objectives.Count <= idx) {
                Objectives.Add(Instantiate(ObjectiveProcessPrefab, ObjectiveRoot));
            }
            var config = m_ObjectiveDB.GetConfig(process.ObjectiveId);
            Objectives[idx].Text.text = config.GetObjectiveText();
            Objectives[idx].Process.text = $"{process.Cur} / {process.Max}";
        }
    }
}
