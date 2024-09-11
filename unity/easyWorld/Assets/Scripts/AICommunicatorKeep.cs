using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AICommunicatorKeep : MonoBehaviour
{
    [Header("Server")]
    public string SERVER_URL = "http://localhost:5000/parse_description";
    public int SERVER_TIMEOUT_SECONDS = 10;

    public ServerHandler serverHandler;

    [Header("Text")]
    public bool resetTextOnButtonClick = false;
    public TMP_InputField inputField;

    [Header("Generation")]
    public WorldGenerator worldGenerator;

    public void OnGenerateTerrainButton()
    {
        string description = inputField.text;

        if (!string.IsNullOrEmpty(description))
        {
            StartCoroutine(GetAIOutput(description));

            StartCoroutine(worldGenerator.ClearCurrentWorld());
        }
        else
        {
            Debug.Log("Invalid description");
        }
    }

    public IEnumerator GetAIOutput(string description)
    {
        if (!serverHandler.IsServerActive)
        {
            Debug.Log("Server is not active, request will fail unless running server externally");
        }

        string jsonData = $"{{\"description\": \"{description}\"}}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = new UnityWebRequest(SERVER_URL, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = SERVER_TIMEOUT_SECONDS
        };

        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Waiting for AI Response");

        yield return request.SendWebRequest();

        HandleWebRequestResult(request);
    }

    void HandleWebRequestResult(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {

            WorldInfo worldInfo = JsonConvert.DeserializeObject<WorldInfo>(request.downloadHandler.text);

            if (worldInfo == null)
            {
                Debug.Log("AI output could not be deserialized, aborting world generation");
                return;
            }

            worldGenerator.GenerateNewWorld(worldInfo);
        }
        else
        {
            Debug.Log("Failed to get AI Output");
        }
    }
}