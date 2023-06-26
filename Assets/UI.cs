using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
//using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public GameObject engine;
    public GameObject gameLogic;
    private Color color = new Color(0.2039216f, 0.2862f, 0.3686275f,1);
    public GameObject historyPanel;
    public GameObject historyRecordPrefab;

    public Color textColor = Color.yellow;
    public GameObject bestMoveBYEngine;
    public GameObject rightPanel;
    public GameObject bestMoveTextField;
    public List<GameObject> buttons = new List<GameObject>();
    // Start is called before the first frame update

    List<GameObject> recordsList = new List<GameObject>();

    public Action<string> WantAPosition;


    void Start()
    {
        recordsList.Add( historyPanel.transform.GetChild(1).gameObject );
        buttons.Add(recordsList.Last().transform.GetChild(1).gameObject);
        buttons.Add(recordsList.Last().transform.GetChild(2).gameObject);
        engine.GetComponent<ChessEngineController>().OnNewBestMove +=  ChangeBestMove;
        gameLogic.GetComponent<GameLogic>().MoveMade += AddToHistory;
        foreach (Transform child in rightPanel.transform) 
        {
            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                child.GetComponent<TextMeshProUGUI>().color = textColor;
            }
        }
    }

    private void ChangeBestMove(string e) 
    {
        bestMoveTextField.GetComponent<TextMeshProUGUI>().text = e.Substring(0,2) + " " + e.Substring(2,2);
    }

    private void AddToHistory(int player, string move) 
    {
        int index = 1;
        foreach (GameObject button in buttons) 
        {
            if (button.GetComponentInChildren<TextMeshProUGUI>().text == "") 
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = move;
                button.GetComponent<Button>().onClick.AddListener(delegate { gameLogic.GetComponent<GameLogic>().BackInTime(index); });
                return;
            }
            index ++;
        }
        createHistoryRecord();
        buttons[buttons.Count - 2].GetComponentInChildren<TextMeshProUGUI>().text = move;
        buttons[buttons.Count - 2].GetComponent<Button>().onClick.AddListener(delegate { gameLogic.GetComponent<GameLogic>().BackInTime(index); });
    }


    private void createHistoryRecord() 
    {
        GameObject child = Instantiate(historyRecordPrefab, historyPanel.transform);
        if (recordsList.Count % 2 == 0) 
        {
            child.GetComponent<Image>().color = color;
        }
        buttons.Add(child.transform.GetChild(1).gameObject);
        buttons.Add(child.transform.GetChild(2).gameObject);
        int lastIndex = recordsList.Count - 1;
        child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (Int32.Parse(recordsList[lastIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Split(".")[0]) + 1).ToString() +".";
        child.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = "";
        child.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = "";
        recordsList.Add(child);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
