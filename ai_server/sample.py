from flask import Flask, request, jsonify
import google.generativeai as genai
import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

genai.configure(api_key=os.environ["API_KEY"])
app = Flask(__name__)

# Initialize the GenerativeModel
model = genai.GenerativeModel("gemini-1.5-flash")

@app.route('/parse_description', methods=['POST'])
def parse_description():
    description = request.json.get('description')

    # Prepare the prompt to extract objects, attributes, and actions/relations
    prompt = f"""
    Parse the following description and extract the objects, attributes, and actions/relations:
    
    Description: "{description}"

    Format the output as:
    Objects: [list of objects]
    Attributes: [list of attributes]
    Actions/Relations: [list of actions/relations]
    """

    # Generate content using the API
    response = model.generate_content(prompt)
    output = response.text

    # Return both the input description and the generated output
    return jsonify({
        "input": description,
        "output": output
    })

if __name__ == '__main__':
    app.run(port=5000)



from flask import Flask, request, jsonify
import google.generativeai as genai
import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

genai.configure(api_key=os.environ["API_KEY"])
app = Flask(__name__)

# Initialize the GenerativeModel
model = genai.GenerativeModel("gemini-1.5-flash")

@app.route('/parse_description', methods=['POST'])
def parse_description():
    description = request.json.get('description')

    # Prepare the prompt to extract a comprehensive set of elements for Unity
    prompt = f"""
    Parse the following description and extract detailed information needed for generating a 3D environment in Unity.
    
    Description: "{description}"

    Format the output as:
    Objects: [list of objects]
    Attributes: [list of attributes]
    Actions/Relations: [list of actions/relations]
    Terrain Type: [type of terrain]
    Surface Texture: [surface texture details]
    Elevation: [description of elevation]
    Water Bodies: [types of water bodies]
    Vegetation Density: [description of vegetation density]
    Vegetation Type: [types of vegetation]
    Terrain Features: [description of terrain features]
    Terrain Conditions: [conditions like dry, wet, etc.]
    Weather Type: [type of weather]
    Temperature: [description of temperature]
    Wind Conditions: [wind-related details]
    Time of Day: [description of the time of day]
    Sun/Moon Position: [position of the sun or moon]
    Sky Conditions: [description of the sky]
    Atmospheric Effects: [description of atmospheric effects]
    Global Lighting: [description of global lighting]
    Light Sources: [types of light sources]
    Shadows: [description of shadows]
    Special Lighting Effects: [special effects like flickering lights]
    Natural Objects: [list of natural objects]
    Man-made Structures: [list of structures]
    Interactive Objects: [objects that can be interacted with]
    Furniture: [list of furniture items]
    Decorative Objects: [list of decorative objects]
    Tools and Equipment: [list of tools and equipment]
    Vehicles: [list of vehicles]
    Humans: [description of human entities]
    Animals: [description of animals]
    Mythical Creatures: [description of mythical creatures]
    Monsters: [description of monsters]
    Artificial Entities: [description of artificial entities]
    NPCs: [list of non-player characters]
    Character Animations: [list of animations]
    Ambient Sounds: [description of ambient sounds]
    Weather Sounds: [description of weather-related sounds]
    Animal Sounds: [description of animal sounds]
    Environmental Sounds: [description of environmental sounds]
    Music: [description of background music]
    Interaction Sounds: [description of interaction-related sounds]
    Character Sounds: [description of character sounds]
    Object Interaction: [types of interactions possible with objects]
    World Interaction: [types of interactions with the world]
    Physics-Based Interactions: [description of physics interactions]
    Environmental Hazards: [description of hazards]
    Inventory Management: [description of inventory management systems]
    Crafting and Building: [description of crafting and building systems]
    Exploration: [description of exploration mechanics]
    Particle Effects: [list of particle effects]
    Visual Effects: [list of visual effects]
    Lighting Effects: [list of lighting effects]
    Camera Effects: [description of camera effects]
    UI Elements: [list of UI elements]
    Story Elements: [description of story elements]
    Dialogue: [description of dialogue systems]
    Books/Notes/Scrolls: [description of in-game lore elements]
    Cinematic Cutscenes: [description of cutscenes]
    Events and Triggers: [description of events and triggers]
    Narration: [description of narration elements]
    Character Development: [description of character development systems]
    Time Systems: [description of time systems]
    Weather Systems: [description of weather systems]
    Economy Systems: [description of economy systems]
    AI Behavior: [description of AI behavior]
    Game Mechanics: [description of game mechanics]
    VR/AR Specific Elements: [description of VR/AR elements if applicable]
    Custom Parameters: [description of any custom parameters]
    Scene Suggestions: [suggestions for scene settings based on the description]
    """

    response = model.generate_content(prompt)
    output = response.text

    # Return both the input description and the generated output
    return jsonify({
        "input": description,
        "output": output
    })

if __name__ == '__main__':
    app.run(port=5000)



from flask import Flask, request, jsonify
import google.generativeai as genai
import os
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()

genai.configure(api_key=os.environ["API_KEY"])
app = Flask(__name__)

# Initialize the GenerativeModel
model = genai.GenerativeModel("gemini-1.5-flash")

@app.route('/parse_description', methods=['POST'])
def parse_description():
    print("I'm here")
    description = request.json.get('description')

    # Simplified response for the "plain" input
    if "plain" in description.lower():
        output = "Terrain Type: plane"
    else:
        output = "Unknown terrain type"

    return jsonify({
        "input": description,
        "output": output
    })

if __name__ == '__main__':
    app.run(port=5000)


using UnityEngine;
using System.Collections;

public class WorldBuilder : MonoBehaviour
{
    public AICommunicator aiCommunicator;
    public string userDescription;

    void Start()
    {
        StartCoroutine(GenerateWorld(userDescription));
    }

    public IEnumerator GenerateWorld(string description)
    {
        yield return StartCoroutine(aiCommunicator.GetMapping(description, (responseText) =>
        {
            if (responseText != null)
            {
                ProcessAIOutput(responseText);
            }
            else
            {
                Debug.LogError("Failed to receive AI output.");
            }
        }));
    }

    private void ProcessAIOutput(string aiOutput)
    {
        // Simplified parsing for "Terrain Type: plane"
        if (aiOutput.Contains("Terrain Type: plane"))
        {
            CreatePlane();
        }
        else
        {
            Debug.LogWarning("Unknown terrain type in AI output.");
        }
    }

    private void CreatePlane()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(10, 1, 10); // Adjust size as needed
        Debug.Log("Flat plane created.");
    }
}
