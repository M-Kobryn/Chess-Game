using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using System.Linq;

public class UI : MonoBehaviour
{
    public GameObject engine;

    public GameObject historyPanel;
    public GameObject historyRecordPrefab;

    public Color textColor = Color.yellow;
    public GameObject bestMoveBYEngine;
    public GameObject rightPanel;
    public GameObject bestMoveTextField;
    // Start is called before the first frame update

    List<GameObject> recordsList = new List<GameObject>();

    void Start()
    {
        //"sdasdasda".Split('|');
        recordsList.Add( historyPanel.transform.GetChild(1).gameObject );
        engine.GetComponent<ChessEngineController>().OnNewBestMove +=  ChangeBestMove;
        foreach (Transform child in rightPanel.transform) 
        {
            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                child.GetComponent<TextMeshProUGUI>().color = textColor;
            }
        }
    }

    private void ChangeBestMove(object sender, ChessEngineController.StringEventArgs e) 
    {
        bestMoveTextField.GetComponent<TextMeshProUGUI>().text = e.text.Substring(0,2) + " " + e.text.Substring(2,2);
    }

    private void createHistoryRecord() 
    {
        GameObject child = Instantiate(historyRecordPrefab, historyPanel.transform);
        int lastIndex = recordsList.Count - 1;
        child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (Int32.Parse(recordsList[lastIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.Split(".")[0]) + 1).ToString() ;
        recordsList.Add(child);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
