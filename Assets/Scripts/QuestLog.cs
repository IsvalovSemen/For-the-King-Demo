using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class QuestLog : MonoBehaviour
{
    public List<Quest> questList;
    [SerializeField] private GameObject _questButtonPrefab;
    [SerializeField] private GameObject _questList;
    [SerializeField] private Button _backButton;
    [SerializeField] private GameObject _selectedQuestPage;
    [SerializeField] private GameObject _questPagePrefab;
    public event Action QuestAdded;

    public Quest CheckQuestList(string questID)
    {
        Quest quest = null;

        for (int i = 0; i <= questList.Count; i++)
        {
            if (questList.Count > 0) //If quest list isn't empty.
            {
                if (questList[i].questInfo.ID == questID) //Checks if there's a quest in the quest list with corresponding ID.
                {
                    quest = questList[i];

                }
                else //If not, adds this quest to the list and increases stage to establish initial objectives.
                {

                    quest = null;
                }
            }
            else //If the list of quests are empty, adds new quest to it and sets the current stage to 1.
            {

                quest = null;
            }
        }

        return quest;
    }

    public void AddQuest(QuestTemplate questInfo)
    {
        Debug.Log("quest with ID " + questInfo.ID + " was added");

        var questTopic = Instantiate(_questButtonPrefab, _questList.transform.GetChild(0).transform);

        var questPage = Instantiate(_questPagePrefab, this.transform);

        questPage.GetComponent<Quest>().questInfo = questInfo;

        questPage.GetComponent<Quest>().stagesList = questInfo.stagesList;

        questList.Add(questPage.GetComponent<Quest>());

        questPage.GetComponent<Quest>().titleTextfield.text = questInfo.title;

        _selectedQuestPage = questPage;

        questTopic.GetComponent<Button>().onClick.AddListener(delegate { ToggleQuestPage(questPage); });

        questTopic.GetComponentInChildren<TMP_Text>().text = questInfo.title;

        QuestAdded?.Invoke();

        questPage.SetActive(false);
    }

    public void ToggleQuestPage(GameObject questPage)
    {
        if (!questPage.activeSelf)
        {
            questPage.SetActive(true);

            _backButton.gameObject.SetActive(true);
        }
        else
        {
            questPage.SetActive(false);

            _backButton.gameObject.SetActive(false);
        }

        ToggleQuestListPage();
    }

    public void ToggleQuestListPage()
    {
        if (!_questList.activeSelf) _questList.SetActive(true);
        else _questList.SetActive(false);
    }

    public void BackToQuestLogMenu()
    {
        ToggleQuestPage(_selectedQuestPage);
    }
}
