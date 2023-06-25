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
    public class StringEventArgs : EventArgs
    {
        public string text;
    }

    public event EventHandler<StringEventArgs> OnNewBestMove;
    private Process chessEngineProcess;
    private Thread engineThread;
    private bool isEngineRunning;
    private string path;
    private int len;
    public List<string> bestMove = new List<string>();

    private void Update()
    {
        if (len < bestMove.Count) 
        {
            OnNewBestMove?.Invoke(this, new StringEventArgs { text = bestMove.Last() });
            len = bestMove.Count;
        }
    }
    bool newMove = false;

    public GameObject tmp;
    public GameLogic gameLogic;

    public async Task NewMove(string notationFEN) 
    {
        SendEngineCommandAsync("position fen " + notationFEN);
        SendEngineCommandAsync("go depth 20");
    }
    private async void Start()
    {

        path = Application.dataPath; ;
        // Start the chess engine process in a separate thread
        engineThread = new Thread(RunChessEngine);
        engineThread.Start();

        // Wait for the engine to start
        await Task.Delay(3000);

        // Send a command to the engine asynchronously
        //await SendEngineCommandAsync("uci");
        //await SendEngineCommandAsync("position startpos");
        SendEngineCommandAsync("go depth 20");
    }

    private void SendEngineCommandAsync(string command)
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
        //UnityEngine.Debug.Log(output);
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