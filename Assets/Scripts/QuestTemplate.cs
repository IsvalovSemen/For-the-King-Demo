using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest")]
public class QuestTemplate : ScriptableObject
{
    public string ID;
    public string title;
    public List<Stage> stagesList;
    public GameObject objectivePrefab;
    public Image checkDone;
    public Image checkFailed;
    public Image checkTrack;
}

[System.Serializable]
public class Stage
{
    public int requiredStageProgressPoints;
    public string stageDetailsText;
    public List<Objective> objectivesList;
    [SerializeField] private bool _complition;
}

public enum ObjectiveStatus { Completed, Failed, Tracking }

[System.Serializable]
public class Objective
{
    public string topic;
    
    public ObjectiveStatus status;
    public int progressPointsValue;
    public TMP_Text labelText;
    public Toggle toggle;
    public QuestUpdate target;
}
