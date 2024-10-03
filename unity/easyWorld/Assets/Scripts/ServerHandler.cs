using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ServerHandler : MonoBehaviour
{
    public bool allowUnityStartServer = true;
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
        if (!allowUnityStartServer)
        {
            UnityEngine.Debug.Log("Unity won't start server, ensure that you are running locally");
            return;
        }

        BaseDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName).FullName;
        string serverPath = Path.Combine(BaseDirectory, SERVER_DIRECTORY, SERVER_NAME);

        if (!File.Exists(serverPath))
        {
            UnityEngine.Debug.Log("Server path doesn't exist, cannot start server");
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
            UnityEngine.Debug.Log("Python server force killed because application was quit");
            CleanupProcess();
        }
    }
}
