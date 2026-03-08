using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Quest : MonoBehaviour
{
    public TMP_Text titleTextfield;
    [SerializeField] private TMP_Text _questDescription;
    [SerializeField] private Transform _objectivesPage;
    public int currentStage;
    [SerializeField] private bool _completed;
    [SerializeField] private int _currentStageProgressPoints;
    public List<Stage> stagesList = new List<Stage>();
    public QuestTemplate questInfo;

    public void UpdateQuestProgress(int objective, ObjectiveStatus status)
    {
        if (stagesList[currentStage].objectivesList.Count > 0) //Checks, is there at least a single objective for current stage.
        {
            stagesList[currentStage].objectivesList[objective].status = status; //Assigns the "completed" status to the specified objective.

            if (currentStage > 0)
            {
                if (stagesList[currentStage].objectivesList[objective].target.questMarker != null) stagesList[currentStage].objectivesList[objective].target.HideQuestMarker();

                stagesList[currentStage - 1].objectivesList[objective].labelText.text = "<s>" + stagesList[currentStage - 1].objectivesList[objective].labelText.text + "</s>";

                if (status == ObjectiveStatus.Completed)
                {
                    stagesList[currentStage - 1].objectivesList[objective].toggle.transform.GetChild(0).gameObject.SetActive(true);

                    stagesList[currentStage - 1].objectivesList[objective].toggle.graphic = stagesList[currentStage - 1].objectivesList[objective].toggle.transform.GetChild(0).GetComponent<Image>();
                }
                else if (status == ObjectiveStatus.Failed)
                {
                    stagesList[currentStage - 1].objectivesList[objective].toggle.transform.GetChild(1).gameObject.SetActive(true);

                    stagesList[currentStage - 1].objectivesList[objective].toggle.graphic = stagesList[currentStage - 1].objectivesList[objective].toggle.transform.GetChild(1).GetComponent<Image>();
                }

                stagesList[currentStage - 1].objectivesList[objective].toggle.isOn = true;
            }

            _currentStageProgressPoints += questInfo.stagesList[currentStage].objectivesList[objective].progressPointsValue; //Appends progress points for the current stage.
        }

        if (_currentStageProgressPoints >= questInfo.stagesList[currentStage].requiredStageProgressPoints) ChangeStage(objective); //If now amount of accumulated points is equal or greater than required for transition to the next stage, moves quest state to the next phase.
    }

    private void ChangeStage(int objective)
    {
        currentStage++;

        _currentStageProgressPoints = 0;

        if (stagesList[currentStage - 1].objectivesList.Count > 0)
        {
            for (int i = 0; i < questInfo.stagesList[currentStage - 1].objectivesList.Count; i++)
            {
                var NewObjective = Instantiate(questInfo.objectivePrefab, _objectivesPage);

                stagesList[currentStage - 1].objectivesList[i].labelText = NewObjective.GetComponentInChildren<TMP_Text>();

                stagesList[currentStage - 1].objectivesList[i].labelText.text = stagesList[currentStage - 1].objectivesList[i].topic;

                stagesList[currentStage - 1].objectivesList[i].toggle = NewObjective.GetComponentInChildren<Toggle>();

                if (stagesList[currentStage].objectivesList[objective].target.questMarker != null) stagesList[currentStage].objectivesList[objective].target.ShowQuestMarker();
            }
        }

        GameMaster.instance.SM.PlaySound("Write down a note");

        _questDescription.text += "\n" + questInfo.stagesList[currentStage - 1].stageDetailsText;
    }
}