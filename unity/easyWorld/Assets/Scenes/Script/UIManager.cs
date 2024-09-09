using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public InputField descriptionInput;
    public Button submitButton;
    public WorldBuilder worldBuilder;

    void Start()
    {
        if (submitButton == null)
        {
            Debug.LogError("Submit button is not assigned!");
            return;
        }

        if (descriptionInput == null)
        {
            Debug.LogError("Description input field is not assigned!");
            return;
        }

        if (worldBuilder == null)
        {
            Debug.LogError("WorldBuilder is not assigned in the inspector!");
            return;
        }

        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        if (worldBuilder == null)
        {
            Debug.LogError("WorldBuilder reference is null when trying to submit.");
            return;
        }

        string description = descriptionInput.text;
        if (!string.IsNullOrEmpty(description))
        {
            worldBuilder.userDescription = description;
            worldBuilder.StartCoroutine(worldBuilder.GenerateWorld(description));
        }
        else
        {
            Debug.LogWarning("Description is empty!");
        }
    }
}
