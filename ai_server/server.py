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
        "terrainData":{{
            "heightsGeneratorData": {{
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
            "texturesGeneratorDataList": [
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
            "treeGeneratorData": {{
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
            "grassGeneratorData": {{
                "octaves": integer,
                "scale": float,
                "lacunarity": float,
                "persistence": float,
                "offset": float,
                "minLevel": float,
                "maxLevel": float,
                "maxSteepness": float,
                "islandSize": float,
                "density": integer,
                "randomize": boolean,
                "grassTextures": integer
            }},
            "waterGeneratorData": {{
                "waterType": string,
                "waterLevel": float,
                "riverWidthRangeX": float,  
                "riverWidthRangeY": float, 
                "randomize": boolean,
                "autoUpdate": boolean
            }}
        }}
    }}


    Use the following description to generate appropriate values:
    "{description}"
    """

    # Call the Gemini API to generate the content
    response = model.generate_content(prompt)

    # Log the raw API response for debugging
    # print("API Response:", response.text)

    # Clean the response by removing any triple backticks if present
    return response.text.strip().strip('```json').strip('```')


if __name__ == '__main__':
    app.run(port=5000)
