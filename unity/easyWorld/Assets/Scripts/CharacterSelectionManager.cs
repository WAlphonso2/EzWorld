using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;  // Reference to the TerrainGenerator script
    public GameObject scrollView;              // Reference to the Scroll View for character selection
    public GameObject[] characterImages;       // Images representing each character in the scroll view
    public Camera initialCamera;
    public Camera thirdPersonCamera;           // Third-person camera for gameplay

    // Show the character selection UI after terrain generation
    public void ShowCharacterSelection()
    {
        scrollView.SetActive(true);  // Enable the scroll view for character selection
        Debug.Log("Character selection UI is now visible.");
    }

    // Method called by the buttons when a character is selected
    public void SelectCharacter(int characterIndex)
    {
        // Inform the TerrainGenerator which character was selected
        terrainGenerator.SetSelectedCharacter(characterIndex);

        // Hide the character selection UI after a character is selected
        HideCharacterSelection();

        // Switch from the initial camera to third-person camera after selection
        SwitchToGameplayCamera();

        Debug.Log("Character selected: " + characterIndex);
    }

    // Hide the character selection UI
    public void HideCharacterSelection()
    {
        scrollView.SetActive(false);  // Disable the scroll view
    }

    // Switch to third-person camera after character selection
    private void SwitchToGameplayCamera()
    {
        initialCamera.enabled = false;
        thirdPersonCamera.enabled = true;
        thirdPersonCamera.gameObject.SetActive(true);

        Debug.Log("Switched to gameplay camera.");
    }
}
