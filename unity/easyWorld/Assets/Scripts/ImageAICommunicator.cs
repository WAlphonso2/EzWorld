using Newtonsoft.Json;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ImageAICommunicator : MonoBehaviour
{
    [Header("Server")]
    public string SERVER_URL = "http://localhost:5000/process_image";
    public int SERVER_TIMEOUT_SECONDS = 10;

    public ServerHandler serverHandler;

    [Header("Text")]
    public bool resetTextOnButtonClick = false;
    public TMP_InputField imageNameInput;
    private JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
    {
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,

    };

    [Header("Generation")]
    public WorldGenerator worldGenerator;

    public void OnProcessImageButton()
    {
        Debug.Log("Button Pressed for Image");
        string imageName = imageNameInput.text;
        //string imageName = "DesertPictureTest.jpg"; // Temporarily hardcode an image name here


        if (!string.IsNullOrEmpty(imageName))
        {
            StartCoroutine(ProcessImage(imageName));
            StartCoroutine(worldGenerator.ClearCurrentWorld());
        }
        else
        {
            Debug.Log("Invalid image name");
        }
    }

    private IEnumerator ProcessImage(string imageName)
    {
        if (!serverHandler.IsServerActive)
        {
            Debug.Log("Server is not active, request will fail unless running server externally");
        }

        string jsonData = $"{{\"image_name\": \"{imageName}\"}}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using UnityWebRequest request = new UnityWebRequest(SERVER_URL, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = SERVER_TIMEOUT_SECONDS
        };

        request.SetRequestHeader("Content-Type", "application/json");

        Debug.Log("Sending request to process image:");


        yield return request.SendWebRequest();

        HandleWebRequestResult(request);
    }

    void HandleWebRequestResult(UnityWebRequest request)
    {
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Server response: " + request.downloadHandler.text);
            WorldInfo worldInfo = JsonConvert.DeserializeObject<WorldInfo>(request.downloadHandler.text, serializerSettings);

            if (worldInfo == null)
            {
                Debug.Log("AI image output could not be deserialized, aborting world generation");
                return;
            }

            worldGenerator.GenerateNewWorld(worldInfo);
        }
        else
        {
            Debug.Log($"Image Failed to get AI Output. Status Code: {request.responseCode}");
            Debug.Log("Error: " + request.downloadHandler.text);
        }
    }
}
