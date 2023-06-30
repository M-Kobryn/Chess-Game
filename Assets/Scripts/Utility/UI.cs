using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.IO;
//using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    public GameObject engine;
    public GameObject gameLogic;
    public GameObject gameBoard;
    private Color color = new Color(0.2039216f, 0.2862f, 0.3686275f,1);
    public GameObject historyPanel;
    public GameObject historyRecordPrefab;

    public Color textColor = Color.yellow;
    public GameObject bestMoveBYEngine;
    public GameObject rightPanel;
    public GameObject bestMoveTextField;
    public List<GameObject> buttons = new List<GameObject>();
    private int showButtonMode =  0;
    string saveFileName;
    string loadFileName;

    public Action RequestNewGame;

    public GameObject savedGamesDropdown;
    // Start is called before the first frame update

    List<GameObject> recordsList = new List<GameObject>();

    public Action<string> WantAPosition;

    public void ResetHistoryList() 
    {
        foreach (GameObject item in buttons) 
        {
            Destroy(item);
        }
        buttons.Clear();
        foreach (GameObject item in recordsList) 
        {
            Destroy(item);
        }
        recordsList.Clear();
        createHistoryRecord();
        foreach (string move in gameLogic.GetComponent<GameLogic>().gameState.movesHistory) 
        {
            AddToHistory(move);
        }
    }




    public void ToggleShowBEstMove() 
    {
        if (showButtonMode == 0) showButtonMode = 1;
        else if (showButtonMode == 1) showButtonMode = 2;
        else if (showButtonMode == 2) showButtonMode = 0;

        if (showButtonMode == 0)
        {
            bestMoveTextField.GetComponent<ButtonBestMoveBehavior>().enabled = false;
            bestMoveTextField.transform.GetChild(0).gameObject.SetActive(true);
        }
        else if (showButtonMode == 1)
        {
            bestMoveTextField.GetComponent<ButtonBestMoveBehavior>().enabled = true;
            //bestMoveTextField.GetComponentInChildren<TextMeshProUGUI>().gameObject.SetActive(false);
        }
        else if(showButtonMode == 2)
        {
            bestMoveTextField.GetComponent<ButtonBestMoveBehavior>().enabled = false;
            bestMoveTextField.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    void Start()
    {
        recordsList.Add( historyPanel.transform.GetChild(1).gameObject );
        buttons.Add(recordsList.Last().transform.GetChild(1).gameObject);
        buttons.Add(recordsList.Last().transform.GetChild(2).gameObject);
        engine.GetComponent<ChessEngineController>().OnNewBestMove +=  ChangeBestMove;
        gameLogic.GetComponent<GameLogic>().MoveMade += AddToHistory;
       // RequestNewGame += gameLogic.GetComponent<GameLogic>().NewGame;
      //  RequestNewGame += ResetHistoryList;
       // RequestNewGame += gameBoard.GetComponent<GameBoard>().SimpleUpdate;
        GetFilesName();
        FileHandler.OnGameLoad += ResetHistoryList;

    }
    private void RemovesButtons(int index)
    {

        for (int x = recordsList.Count -1; x > ((int)((index +1) / 2) -1); x--) 
        {
            Destroy(recordsList[x]);
            recordsList.RemoveAt(x);
        }
        
        if (index % 2 != 0) 
        {
            recordsList.Last().transform.GetChild(2).gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
        }
        buttons.RemoveAll(s => s == null);

    }
    private void ChangeBestMove(string e) 
    {
        bestMoveTextField.GetComponentInChildren<TextMeshProUGUI>().text = e.Substring(0,2) + " " + e.Substring(2,2);
    }

    private void AddToHistory(string move) 
    {
        int index = 1;
        foreach (GameObject button in buttons) 
        {
            if (button == null) 
            {
                index--;
                continue;
            }
            if (button.GetComponentInChildren<TextMeshProUGUI>().text == "") 
            {
                button.GetComponentInChildren<TextMeshProUGUI>().text = move;
                button.GetComponent<Button>().onClick.AddListener(delegate { gameLogic.GetComponent<GameLogic>().BackInTime(index); });
                button.GetComponent<Button>().onClick.AddListener(delegate { RemovesButtons(index); });
                return;
            }
            index ++;
        }
        createHistoryRecord();
        buttons[buttons.Count - 2].GetComponentInChildren<TextMeshProUGUI>().text = move;
        buttons[buttons.Count - 2].GetComponent<Button>().onClick.AddListener(delegate { gameLogic.GetComponent<GameLogic>().BackInTime(index); });
        buttons[buttons.Count - 2].GetComponent<Button>().onClick.AddListener(delegate { RemovesButtons(index); });
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
        int lastIndex = recordsList.Count;
        child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (lastIndex+1).ToString() + ".";
        child.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = "";
        child.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = "";
        recordsList.Add(child);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveMethod() 
    {
        FileHandler.Save(saveFileName, gameLogic.GetComponent<GameLogic>());
        GetFilesName();
    }
    public void LoadMethod() 
    {
        FileHandler.Load(loadFileName, gameLogic.GetComponent<GameLogic>());
    }
    public void SetSaveFileName(string name)
    {
         saveFileName = ( name + ".json");
    }
    public void SetLoadFileName()
    {
         loadFileName = (savedGamesDropdown.GetComponent<TMP_Dropdown>().options[savedGamesDropdown.GetComponent<TMP_Dropdown>().value].text + ".json");
    }
    private void GetFilesName()
    {
        string path = Application.persistentDataPath;
        string[] fileNames = Directory.GetFiles(path);
        for (int x = 0; x < fileNames.Length;x++) 
        {
            fileNames[x] = Path.GetFileName(fileNames[x]).Split(".")[0];
        }

        //List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
        //foreach (string name in fileNames)
        //{
        //    list.Add(new TMP_Dropdown.OptionData(Path.GetFileName(name).Split(".")[0]));
        //}

        savedGamesDropdown.GetComponent<TMP_Dropdown>().ClearOptions();
        savedGamesDropdown.GetComponent<TMP_Dropdown>().AddOptions(fileNames.ToList());
    }
    public void DeleteSaveMethod() 
    {
        string path = Application.persistentDataPath;
        File.Delete(Path.Combine(path, (loadFileName) ));
        GetFilesName();
    }
    public void NewGame() 
    {
        gameLogic.GetComponent<GameLogic>().NewGame();
        gameBoard.GetComponent<GameBoard>().SimpleUpdate();
        ResetHistoryList();
    }
}
