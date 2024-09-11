from flask import Flask, request, jsonify
import google.generativeai as genai
import os
from dotenv import load_dotenv
import json

# Load environment variables
load_dotenv()

# Configure Google Generative AI with the API key
genai.configure(api_key=os.environ.get("API_KEY"))

app = Flask(__name__)

# Initialize the Gemini model
model = genai.GenerativeModel("gemini-1.5-flash")

@app.route('/parse_description', methods=['POST'])
def parse_description():

    description = request.json.get('description')

    if not description:
        return jsonify({"error": "Description is required."}), 400

    # Create the prompt for the AI
    prompt = f"""
    You are a terrain generation AI for a game. Based on the user's description, 
    return a JSON object with parameters required for generating terrain. 
    Make sure that the values for each parameter fall within reasonable ranges to avoid any out-of-bounds issues. 
    Please use the following guidelines for each module:

    HeightsGenerator:
    - width and height should be around 1024 (minimum 512, maximum 2048).
    - depth represents the terrain height and should be between 5 and 150.
    - octaves represent the levels of detail and should be between 1 and 15.
    - scale determines the level of detail and should range between 60 and 500.
    - lacunarity affects how much detail is added at each octave, typically between 1 and 3.0.
    - persistence controls how each octave contributes to the overall shape, typically between 0.3 and 0.8.
    - heightCurve: Choose from ["linear", "constant", "easeIn", "easeOut", "sine", "bezier"].  Rate of change of heights curve.
    - heightCurveOffset is the vertical offset of the height curve, usually between 1000 and 12000.
    - falloffDirection affects the direction of terrain slopes, usually between 1 and 7.
    - falloffRange affects the slope of the terrain, usually between 1 and 5.
    - useFalloffMap should be true or false, with values typically around 4 or 5.
    - randomize and autoUpdate should be true or false.

    TexturesGenerator:
    - texture should be a list of textures (comma-separated) that can include any of the following it should be only for the terrain:
    - Water is not part of terrain TexturesGenerator
    ["grass", "desert", "snow", "mud", "rock", "sand", "forestFloor", "mountainRock", "dirt", "deadGrass"].
    - The user may ask for multiple textures, so the model should include more than one texture if necessary. 
    For example, "desert, deadGrass" for a desert with patches of dead grass, or "snow, rock" for snowy mountains.
    - Each texture should have its own properties: 
        - `heightCurve`: Choose from ["linear", "constant", "easeIn", "easeOut", "sine", "bezier"]. These represent the curve type for how the texture is applied based on terrain height.
        - `tileSizeX`: float, between 5 and 50 (determines how the texture is tiled on the X axis).
        - `tileSizeY`: float, between 5 and 50 (determines how the texture is tiled on the Y axis).
        Ensure each texture has unique values for these properties. 


    GrassGenerator:
    - octaves should be between 1 and 8.
    - scale should be between 0.5 and 50.
    - lacunarity should be between 1.5 and 3.
    - persistence should be between 0.3 and 0.7.
    - offset should be between 0 and 1.
    - minLevel should be between 0.05 and 1.
    - maxLevel should be between 0.05 and 1.
    - maxSteepness should be between 10 and 70.
    - islandSize should be between -1 and 1.
    - density should be between 1 and 100.
    - randomize and autoUpdate should be true or false.
    - Grass Texture should be an integer representing the number of Grass Texture, typically between 1 and 10.

    TreeGenerator:
    - octaves should be between 1 and 8.
    - scale should be between 0.5 and 50.
    - lacunarity should be between 1.5 and 3.
    - persistence should be between 0.3 and 0.7.
    - offset should be between 0 and 1.
    - minLevel should be between 0.05 and 1.
    - maxLevel should be between 0.05 and 1.
    - maxSteepness should be between 10 and 70.
    - islandSize should be between -1 and 1.
    - density should be between 0.1 and 10.
    - randomize and autoUpdate should be true or false.
    - treePrototypes should be an integer representing the number of tree prefabs, typically between 1 and 10.

    WaterGenerator:
    - waterType should be "river", "lake", "ocean", or "none".
    - waterLevel represents the level of water height for lakes or oceans.
    - river width range x and y, x should be between 500 and 2000, y should be between 500 and 2000 
    - randomize and autoUpdate should be true or false.

    Make sure you return the result in JSON format like this:   
    {{
        "heightsGenerator": {{
            "width": integer,
            "height": integer,
            "depth": integer,
            "octaves": integer,
            "scale": float,
            "lacunarity": float,
            "persistence": float,
            "heightCurve": string,
            "heightCurveOffset": float,
            "falloffDirection": float,
            "falloffRange": float,
            "useFalloffMap": boolean,
            "randomize": boolean,
            "autoUpdate": boolean
        }},
        "texturesGenerator": [
            {{
                "texture": string,
                "heightCurve": string,
                "tileSizeX": float,
                "tileSizeY": float
            }},
            {{
                "texture": string,
                "heightCurve": string,
                "tileSizeX": float,
                "tileSizeY": float
            }},
            ...
        ],
        "treeGenerator": {{
            "octaves": integer,
            "scale": float,
            "lacunarity": float,
            "persistence": float,
            "offset": float,
            "minLevel": float,
            "maxLevel": float,
            "maxSteepness": float,
            "islandSize": float,
            "density": float,
            "randomize": boolean,
            "treePrototypes": integer
        }},
        "grassGenerator": {{
            "octaves": integer,
            "scale": float,
            "lacunarity": float,
            "persistence": float,
            "offset": float,
            "minLevel": float,
            "maxLevel": float,
            "maxSteepness": float,
            "islandSize": float,
            "density": int,
            "randomize": boolean,
            "grassTextures": integer
        }},
        "waterGenerator": {{
            "waterType": string,
            "waterLevel": float,
            "riverWidthRangeX": float,  
            "riverWidthRangeY": float, 
            "randomize": boolean,
            "autoUpdate": boolean
        }}
    }}


    Use the following description to generate appropriate values:
    "{description}"
    """


    try:
        # Call the Gemini API to generate the content
        response = model.generate_content(prompt)

        # Log the raw API response for debugging
        print("API Response:", response.text)

        # Clean the response by removing any triple backticks if present
        clean_response = response.text.strip().strip('```json').strip('```')

        # Convert the cleaned response to a JSON structure
        terrain_data = json.loads(clean_response)

        # Handle multiple textures in texturesGenerator
        textures_data = []
        for texture in terrain_data.get("texturesGenerator", []):
            textures_data.append({
                "texture": texture.get("texture", "None"),
                "heightCurve": texture.get("heightCurve", "smooth"),
                "tileSizeX": texture.get("tileSizeX", 10.0),
                "tileSizeY": texture.get("tileSizeY", 10.0)
            })

        # Construct the final response
        structured_response = {
            "heightsGenerator": {
                "width": terrain_data.get("heightsGenerator", {}).get("width", 1024),
                "height": terrain_data.get("heightsGenerator", {}).get("height", 1024),
                "depth": terrain_data.get("heightsGenerator", {}).get("depth", 100),
                "octaves": terrain_data.get("heightsGenerator", {}).get("octaves", 4),
                "scale": terrain_data.get("heightsGenerator", {}).get("scale", 100.0),
                "lacunarity": terrain_data.get("heightsGenerator", {}).get("lacunarity", 2.0),
                "persistence": terrain_data.get("heightsGenerator", {}).get("persistence", 0.5),
                "heightCurve": terrain_data.get("heightsGenerator", {}).get("heightCurve", "linear"), 
                "heightCurveOffset": terrain_data.get("heightsGenerator", {}).get("heightCurveOffset", 0.3),
                "falloffDirection": terrain_data.get("heightsGenerator", {}).get("falloffDirection", 3),
                "falloffRange": terrain_data.get("heightsGenerator", {}).get("falloffRange", 3),
                "useFalloffMap": terrain_data.get("heightsGenerator", {}).get("useFalloffMap", True),
                "randomize": terrain_data.get("heightsGenerator", {}).get("randomize", False),
                "autoUpdate": terrain_data.get("heightsGenerator", {}).get("autoUpdate", True)
            },
            "texturesGenerator": textures_data,  # List of textures
            "treeGenerator": {
                "octaves": terrain_data.get("treeGenerator", {}).get("octaves", 3),
                "scale": terrain_data.get("treeGenerator", {}).get("scale", 1.0),
                "lacunarity": terrain_data.get("treeGenerator", {}).get("lacunarity", 2.0),
                "persistence": terrain_data.get("treeGenerator", {}).get("persistence", 0.5),
                "offset": terrain_data.get("treeGenerator", {}).get("offset", 0.2),
                "minLevel": terrain_data.get("treeGenerator", {}).get("minLevel", 0.1),
                "maxLevel": terrain_data.get("treeGenerator", {}).get("maxLevel", 0.9),
                "maxSteepness": terrain_data.get("treeGenerator", {}).get("maxSteepness", 45.0),
                "islandSize": terrain_data.get("treeGenerator", {}).get("islandSize", 1.0),
                "density": terrain_data.get("treeGenerator", {}).get("density", 5.0),
                "randomize": terrain_data.get("treeGenerator", {}).get("randomize", False),
                "treePrototypes": terrain_data.get("treeGenerator", {}).get("treePrototypes", 3)
            },
            "grassGenerator": {
                "octaves": terrain_data.get("grassGenerator", {}).get("octaves", 3),
                "scale": terrain_data.get("grassGenerator", {}).get("scale", 0.8),
                "lacunarity": terrain_data.get("grassGenerator", {}).get("lacunarity", 2.0),
                "persistence": terrain_data.get("grassGenerator", {}).get("persistence", 0.5),
                "offset": terrain_data.get("grassGenerator", {}).get("offset", 0.3),
                "minLevel": terrain_data.get("grassGenerator", {}).get("minLevel", 0.1),
                "maxLevel": terrain_data.get("grassGenerator", {}).get("maxLevel", 1.0),
                "maxSteepness": terrain_data.get("grassGenerator", {}).get("maxSteepness", 45.0),
                "islandSize": terrain_data.get("grassGenerator", {}).get("islandSize", 1.0),
                "density": terrain_data.get("grassGenerator", {}).get("density", 20),
                "randomize": terrain_data.get("grassGenerator", {}).get("randomize", False),
                "grassTextures": terrain_data.get("grassGenerator", {}).get("grassTextures", 2)
            },"waterGenerator": {
                "waterType": terrain_data.get("waterGenerator", {}).get("waterType", "none"),
                "waterLevel": terrain_data.get("waterGenerator", {}).get("waterLevel", 20),  
                "randomize": terrain_data.get("waterGenerator", {}).get("randomize", True),
                "riverWidthRange": terrain_data.get("waterGenerator", {}).get("riverWidthRange", (1024, 1024)),
                "autoUpdate": terrain_data.get("waterGenerator", {}).get("autoUpdate", True)
    }
        }

    except json.JSONDecodeError as e:
        # Log error details
        print(f"Error parsing JSON: {e}")
        return jsonify({"error": "Failed to generate valid terrain data."}), 500

    # Return the generated terrain data as JSON
    return jsonify(structured_response)


if __name__ == '__main__':
    app.run(port=5000)
