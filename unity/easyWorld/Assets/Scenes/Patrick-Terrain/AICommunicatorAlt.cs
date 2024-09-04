using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AICommunicatorAlt : MonoBehaviour
{
    [Header("Server")]
    public string SERVER_URL = "http://localhost:5000/parse_description";

    public ServerHandler serverHandler;

    [Header("Text")]
    public bool resetTextOnButtonClick = false;
    public TMP_InputField inputField;

    [Header("Generation")]
    public TerrainGenerator terrainGenerator;

    public void OnGenerateTerrainButton()
    {
        string description = inputField.text;

        if (!string.IsNullOrEmpty(description))
        {
            StartCoroutine(GetAIOutput(description));
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
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Waiting for AI Response");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            terrainGenerator.CreateNewTerrainObject();
        }
        else
        {
            Debug.Log("Failed to get AI Output");
        }
    }
}
