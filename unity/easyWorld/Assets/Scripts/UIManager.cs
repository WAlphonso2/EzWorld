using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public InputField descriptionInput;
    public Button submitButton;
    public WorldBuilder worldBuilder;

    void Start()
    {
        // Add a listener to the submit button
        submitButton.onClick.AddListener(OnSubmit);
    }

    void OnSubmit()
    {
        // Get the description from the InputField
        string description = descriptionInput.text;

        if (!string.IsNullOrEmpty(description))
        {
            // Pass the description to the WorldBuilder script
            worldBuilder.userDescription = description;
            worldBuilder.StartCoroutine(worldBuilder.GenerateWorld(description));
        }
        else
        {
            Debug.LogWarning("Description is empty!");
        }
    }
}
