using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestUpdate : MonoBehaviour
{
    [SerializeField] private QuestTemplate _questInfo;
    [SerializeField] private int _objectiveIndex;
    [SerializeField] private int _stageIndex;
    [SerializeField] private QuestLog _questLog;
    [SerializeField] private Quest _questLink;
    public Transform questMarker;
    [SerializeField] private GameObject _questMarkerPrefab;
    [SerializeField] ObjectiveStatus _statusUpdate;
    //using UnityEditor;

    public void Start()
    {
        _questLog.QuestAdded += AssignQuestMarker;

        if (_stageIndex != 0) questMarker = Instantiate(_questMarkerPrefab, GameMaster.instance.HUD.transform).transform;

        HideQuestMarker();
    }

    private void LateUpdate()
    {
        if (questMarker != null)
        {
            float distanceToPlayer = Vector3.Distance(Player.instance.transform.position, transform.position);

            float minX = questMarker.GetComponent<Image>().GetPixelAdjustedRect().width / 2;

            float maxX = Screen.width;

            float minY = questMarker.GetComponent<Image>().GetPixelAdjustedRect().height / 2;

            float maxY = Screen.height;

            Vector2 pos = CameraControl.instance.mainCam.WorldToScreenPoint(transform.position);

            if (Vector3.Dot((transform.position - Player.instance.transform.position), transform.forward) < 0)
            {
                if (pos.x < Screen.width / 2) pos.x = maxX;
                else pos.x = minX;
            }

            pos.x = Mathf.Clamp(pos.x, minX, maxX);

            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            questMarker.position = pos;

            questMarker.GetChild(0).GetComponent<TMP_Text>().text = ((int)distanceToPlayer).ToString();
        }


        /*
        if (_questMarker != null)
        {
            _questMarker.LookAt(transform.position + GameMaster.instance.mainCam.transform.forward); //Billboard script example.

            float DistanceToPlayer = Vector3.Distance(GameMaster.instance.transform.position, transform.position);

            _questMarker.GetChild(0).GetComponent<TMP_Text>().text = ((int) DistanceToPlayer).ToString();

            _questMarker.localScale = new Vector3(_questMarker.localScale.x * DistanceToPlayer, _questMarker.localScale.y * DistanceToPlayer, _questMarker.localScale.z * DistanceToPlayer);
        }
        */
    }

    public void CompleteQuestObjective()
    {
        bool inList = false;

        if (_questLog.questList.Count > 0)
        {
            for (int i = 0; i <= _questLog.questList.Count; i++)
            {
                if (_questLog.questList[i].questInfo.ID == _questInfo.ID)
                {
                    _questLog.questList[i].UpdateQuestProgress(_objectiveIndex, _statusUpdate);

                    inList = true;
                }
            }
        }
        
        
        if (!inList)
        {
            _questLog.AddQuest(_questInfo);

            _questLog.questList[_questLog.questList.Count - 1].UpdateQuestProgress(_objectiveIndex, _statusUpdate);
        }
    }

    public void ShowQuestMarker()
    {
        if (questMarker != null) questMarker.gameObject.SetActive(true);
    }

    public void HideQuestMarker()
    {
        if (questMarker != null) questMarker.gameObject.SetActive(false);
    }

    private void AssignQuestMarker()
    {
        _questLog.questList[_questLog.questList.Count - 1].stagesList[_stageIndex].objectivesList[_objectiveIndex].target = this;


    }
}
