using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AICommunicator : MonoBehaviour
{
    private string serverURL = "http://localhost:5000/parse_description";

    public IEnumerator GetMapping(string description, System.Action<TerrainDataFromAI> callback)
    {
        string jsonData = "{\"description\":\"" + description + "\"}";

        UnityWebRequest request = new UnityWebRequest(serverURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            TerrainDataFromAI parsedResponse = JsonUtility.FromJson<TerrainDataFromAI>(responseText);
            callback(parsedResponse);
        }
        else
        {
            Debug.LogError("Error sending request: " + request.error);
            callback(null);
        }
    }
}
