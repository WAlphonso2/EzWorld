using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ServerHandler : MonoBehaviour
{
    public bool allowServerStart = true;
    public string EXECUTABLE = "python";
    public string SERVER_DIRECTORY = "ai_server";
    public string SERVER_NAME = "server.py";

    private Process pythonAIServer;

    public string BaseDirectory { get; private set; }
    public bool IsServerActive => pythonAIServer != null && !pythonAIServer.HasExited;

    void Start()
    {
        TryStartServer();
    }

    void TryStartServer()
    {
        if (!allowServerStart)
        {
            UnityEngine.Debug.Log("Server is disabled, not starting server");
            return;
        }

        BaseDirectory = Path.Combine(Application.dataPath, "..\\..\\..");
        string serverPath = Path.Combine(BaseDirectory, SERVER_DIRECTORY, SERVER_NAME);

        if (!File.Exists(serverPath))
        {
            UnityEngine.Debug.Log("Server path doesn't exist, aborting server start");
            return;
        }

        // define python server process
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = EXECUTABLE,
            Arguments = serverPath,
        };

        pythonAIServer = new Process()
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        pythonAIServer.Exited += OnProcessExit;

        try
        {
            pythonAIServer.Start();
            UnityEngine.Debug.Log("Server successfully started");
        }
        catch
        {
            UnityEngine.Debug.Log("Failed to start server process");
            CleanupProcess();
        }
    }

    void OnProcessExit(object sender, EventArgs e)
    {
        if (pythonAIServer != null)
        {
            UnityEngine.Debug.Log("Server ended with exit code " + pythonAIServer.ExitCode);
            CleanupProcess();
        }
    }

    void CleanupProcess()
    {
        if (pythonAIServer == null) return;

        if (!pythonAIServer.HasExited) pythonAIServer.Kill();

        pythonAIServer.Dispose();

        pythonAIServer = null;
    }

    void OnApplicationQuit()
    {
        // force quit server if its still running on application close
        if (pythonAIServer != null && !pythonAIServer.HasExited)
        {
            UnityEngine.Debug.Log("Python server force killed");
            CleanupProcess();
        }
    }
}
