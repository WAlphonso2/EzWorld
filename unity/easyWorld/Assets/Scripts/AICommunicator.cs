using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AICommunicator : MonoBehaviour
{
    [Header("Server")]
    public string SERVER_TEXT_URL = "http://localhost:5000/parse_description";
    public string SERVER_IMAGE_URL = "http://localhost:5000/process_image";
    public int SERVER_TIMEOUT_SECONDS = 10;

    public ServerHandler serverHandler;

    [Header("Text")]
    public bool resetTextOnButtonClick = false;
    public TMP_InputField inputField;
    public TMP_InputField imageNameInput;
    private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,

    };

    [Header("Generation")]
    public WorldGenerator worldGenerator;

    public void OnGenerateTerrainButton()
    {
        string imageName = imageNameInput.text;
        string description = inputField.text;

        if (!string.IsNullOrEmpty(imageName))
        {
            // handle case of image
            StartCoroutine(ProcessImage(imageName));
        }
        else if (!string.IsNullOrEmpty(description))
        {
            // handles case of text
            StartCoroutine(GetAITextOutput(description));
        }
        else
        {
            Debug.Log("Invalid image / description");
            return;
        }

        StartCoroutine(worldGenerator.ClearCurrentWorld());
    }

    public IEnumerator GetAITextOutput(string description)
    {
        if (!serverHandler.IsServerActive)
        {
            Debug.Log("Server is not active, request will fail unless running server externally");
        }

        string jsonData = $"{{\"description\": \"{description}\"}}";

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = new UnityWebRequest(SERVER_TEXT_URL, "POST")
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

    private IEnumerator ProcessImage(string imageName)
    {
        if (!serverHandler.IsServerActive)
        {
            Debug.Log("Server is not active, request will fail unless running server externally");
        }

        string jsonData = $"{{\"image_name\": \"{imageName}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = new UnityWebRequest(SERVER_IMAGE_URL, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = SERVER_TIMEOUT_SECONDS
        };

        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending request to process image:");

        yield return request.SendWebRequest();

        Debug.Log("Got back a web request for image");

        HandleWebRequestResult(request);
    }

    void HandleWebRequestResult(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            WorldInfo worldInfo = JsonConvert.DeserializeObject<WorldInfo>(request.downloadHandler.text, serializerSettings);

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