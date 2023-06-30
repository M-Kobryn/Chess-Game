using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Transactions;
using System.Xml.Schema;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System.Threading;
using TMPro;
using static Unity.Burst.Intrinsics.X86.Avx;

[Serializable]
public class GameLogic : MonoBehaviour
{
    public int playerColor = 0;
    public GameObject gameBoard;
    public ChessGameLogic gameState;
    public GameObject controller;
    public Action<string> MoveMade;
    public Action OnStateJump;


    public int gameMode;
    public bool AutoPlayEnabled;
    bool autoPlayRunning = false;
    public bool OppoenentAI = false;
    public bool gameStarted = false;

    private async void Update()
    {
        if (AutoPlayEnabled && !autoPlayRunning)
        {
            autoPlayRunning = true;
            await controller.GetComponent<ChessEngineController>().NewMove(gameState.gameHistory.Last());
        }
        if (!AutoPlayEnabled)
        {
            autoPlayRunning = false;
        }
    }


    public void toggleAutoPlay()
    {
        if (gameMode == 0) gameMode = 1;
        else if (gameMode == 1) gameMode = 2;
        else if (gameMode == 2) gameMode = 0;

        if (gameMode == 0)
        {
            AutoPlayEnabled = false;
            OppoenentAI = false;
        }
        else if (gameMode == 1)
        {
            AutoPlayEnabled = false;
            OppoenentAI = true;
        }
        else if (gameMode == 2)
        {
            AutoPlayEnabled = true;
            OppoenentAI = false;
        }
    }

    void Start()
    {
        gameState = new ChessGameLogic();
        controller.GetComponent<ChessEngineController>().OnNewBestMove += DoAutoMove;
        controller.GetComponent<ChessEngineController>().OnNewBestMove += DoAIMove;
    }
    public async void DoAutoMove(string e)
    {
        if (OppoenentAI && playerColor != gameState.currentPlayer)
        {
            string move = e;
            Vector2Int from = ChessGameLogic.stringPositionToBoard(move.Substring(0, 2));
            Vector2Int to = ChessGameLogic.stringPositionToBoard(move.Substring(2, 2));

            MovePice(from, to);
            await controller.GetComponent<ChessEngineController>().NewMove(gameState.gameHistory.Last());
        }
    }

    public async void DoAIMove(string e)
    {
        if (!gameState.gameEnded && AutoPlayEnabled)
        {
            string move = e;
            Vector2Int from = ChessGameLogic.stringPositionToBoard(move.Substring(0, 2));
            Vector2Int to = ChessGameLogic.stringPositionToBoard(move.Substring(2, 2));
            MovePice(from, to);
            await controller.GetComponent<ChessEngineController>().NewMove(gameState.gameHistory.Last());
        }
    }
    public bool MovePice(Vector2Int from, Vector2Int destination, int promotionPiece = Pices.blank) 
    {
        if (gameState.MovePice(from, destination, promotionPiece))
        {
            MoveMade?.Invoke(ChessGameLogic.boardPositionToString(from) + ChessGameLogic.boardPositionToString(destination));
            return true;
        }
        else 
        {
            return false;
        }
 
    }

    public void BackInTime(int index)
    {
        if (gameState.gameHistory.Count < index)
        {
            return;
        }
        List<string> newGameHistory = new List<string>();
        List<string> newMovesHistory = new List<string>();
        int x = 0;
        foreach (string fen in gameState.gameHistory)
        {
            if (x <= index)
            {
                newGameHistory.Add(fen);
            }
            x++;
        }
        x = 0;
        foreach (string move in gameState.movesHistory)
        {
            if (x <= index - 1)
            {
                newMovesHistory.Add(move);
            }
            x++;
        }
        gameState.gameHistory = newGameHistory;
        gameState.movesHistory = newMovesHistory;
        gameState.boardStateFromFEN(gameState.gameHistory.Last());
        OnStateJump?.Invoke();
    }

    public void NewGame()
    {
        gameState = new ChessGameLogic();
    }
}
