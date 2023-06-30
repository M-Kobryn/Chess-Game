using UnityEngine;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Linq;
using System;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using UnityEditor.PackageManager;

public class ChessEngineController : MonoBehaviour
{

    public Action<string> OnNewBestMove ;
    private Process chessEngineProcess;
    private Thread engineThread;
    private bool isEngineRunning;
    private int len = 0;
    private string path;

    private int depth = 20;
    private int engineElo = 2850;

    public GameLogic gameLogic;

    public List<string> bestMove = new List<string>();



    private void Update()
    {
        if (len < bestMove.Count)
        {
            OnNewBestMove?.Invoke(bestMove.Last() );
            len = bestMove.Count;
        }
    }


    public void SetELOCommand(string elo) 
    {
        engineElo = Int32.Parse(elo);
        const int MAX_ELO = 2850;
        const int MIN_ELO = 1350;
        float skillFactor =(float) (engineElo - MIN_ELO) / ( MAX_ELO - MIN_ELO);
        const int MAX_STOCKFISH_SKILL = 20;
        //SendEngineCommandAsync("setoption name UCI_Elo value " + ((int)(skillFactor * MAX_STOCKFISH_SKILL)).ToString());
        int skilllevel = Mathf.RoundToInt(MAX_STOCKFISH_SKILL * skillFactor);
        SendEngineCommandAsync("setoption name skill level value " + skilllevel.ToString());
    }
    public void SetDepth(string elo)
    {
        depth = Int32.Parse(elo);
    }


    public async Task NewMove(string notationFEN) 
    {
        SendEngineCommandAsync("position fen " + notationFEN);
        SendEngineCommandAsync("go depth " + depth.ToString() );
    }
    private async void Start()
    {

        path = Application.dataPath;
        // Start the chess engine process in a separate thread
        engineThread = new Thread(RunChessEngine);
        engineThread.Start();
        SetELOCommand(engineElo.ToString());
        SendEngineCommandAsync("setoption name debug value on");

        // Wait for the engine to start

        // Send a command to the engine asynchronously
        //await SendEngineCommandAsync("uci");
        //await SendEngineCommandAsync("position startpos");
        //SendEngineCommandAsync("go depth 20");
    }

    public void SendEngineCommandAsync(string command)
    {
        if (isEngineRunning)
        {
            // Write the command to the engine's standard input asynchronously
            chessEngineProcess.StandardInput.WriteLine(command);
            chessEngineProcess.StandardInput.Flush();
        }
    }

    private async void RunChessEngine()
    {
        // Create the process start info for the chess engine
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = path+ "/engine/engine.exe";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = true;

        // Start the chess engine process
        chessEngineProcess = new Process();
        chessEngineProcess.StartInfo = startInfo;
        chessEngineProcess.EnableRaisingEvents = true;
        chessEngineProcess.OutputDataReceived += HandleEngineOutput;
        chessEngineProcess.Exited += HandleEngineExit;
        chessEngineProcess.Start();

        // Begin asynchronous reading of the engine's standard output
        chessEngineProcess.BeginOutputReadLine();

        isEngineRunning = true;
        SendEngineCommandAsync("uci");
        // Wait for the engine process to exit
        await Task.Run(() => chessEngineProcess.WaitForExit());
    }

    private void HandleEngineOutput(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            // Process the output received from the engine asynchronously
            Task.Run(() => ProcessEngineOutput(e.Data));
        }
    }

    private void ProcessEngineOutput(string output)
    {
        // Process the engine's output and update the game state accordingly
        // This can include parsing move suggestions, analysis results, etc.
        string[] outputSplit = output.Split(" ");
        if (outputSplit.Length > 2) 
        {
            if (outputSplit[0] == "bestmove") 
            {
                bestMove.Add(outputSplit[1]);
            }
        }
        UnityEngine.Debug.Log(output);
    }

    private void HandleEngineExit(object sender, System.EventArgs e)
    {
        // Clean up and handle engine process exit
        chessEngineProcess.Close();
        chessEngineProcess.Dispose();
        isEngineRunning = false;

        // Perform any additional cleanup or actions
    }

    private void OnDestroy()
    {
        // Stop the engine thread and clean up resources when the game object is destroyed
        engineThread?.Abort();
        chessEngineProcess?.Close();
        chessEngineProcess?.Dispose();
    }
}