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
    If the description for the terrain does not need tree or grass like dessert(height 162, octaves 8, ) or snow unles the user specicly said, do not add( make the values zero's).
    Please use the following guidelines for each module:

    HeightsGenerator:
    Width: Determines the width of the terrain in units.
    Height: Determines the height (or length) of the terrain in units.
    Depth: Controls the maximum height variation of the terrain (vertical scaling).
    Octaves: Specifies the number of noise layers used in terrain generation (more octaves add detail).
    Scale: Adjusts the scale of the noise map, affecting the size of terrain features.
    Lacunarity: Controls the increase in frequency for each octave, adding finer details.
    Persistance: Determines how the amplitude decreases with each octave, affecting how details diminish.
    HeightCurve: An animation curve that adjusts the terrain heights based on evaluated noise values.
    Offset: A value to randomize or shift the noise pattern used in terrain generation.
    FalloffDirection: Defines the direction of the falloff, influencing terrain shape towards edges.
    FalloffRange: Defines how far the falloff effect reaches, influencing terrain smoothness.
    UseFalloffMap: Toggles the use of a falloff map to control terrain generation near edges.
    Randomize: Enables randomization of the noise offset to generate different terrains each time.
    AutoUpdate: Automatically updates the terrain when changes are made in the inspector.
    ShallowDepth: Controls the shallow depth for terrain, affecting low-lying areas.

    - width and height should be around 1024 (minimum 512, maximum 1024).
    - depth represents the terrain height and should be between 5 and 200.
    - octaves represent the levels of detail and should be between 1 and 15.
    - scale determines the level of detail and should range between 60 and 500.
    - lacunarity affects how much detail is added at each octave, typically between 1 and 5.
    - persistence controls how each octave contributes to the overall shape, typically between 0 and 1.
    - heightCurve: Choose from ["linear", "constant", "easeIn", "easeOut", "sine", "bezier"].  Rate of change of heights curve.
    - heightCurveOffset is the vertical offset of the height curve, usually between 1000 and 12000.
    - falloffDirection affects the direction of terrain slopes, usually between 1 and 7.
    - falloffRange affects the slope of the terrain, usually between 1 and 5.
    - useFalloffMap should be true or false.
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
    Octaves: Controls the number of noise layers used in the Perlin noise generation (more octaves add detail).
    Scale: Defines the scale of the Perlin noise; a higher value stretches the noise pattern, making features larger.
    Lacunarity: Determines how much the frequency of each octave increases; higher values result in more noise details.
    Persistance: Controls how the amplitude decreases with each octave; higher values retain more detail in higher octaves.
    Offset: A shift applied to the Perlin noise generation, used to randomize the noise pattern.
    MinLevel: Minimum terrain height where grass or trees can be placed.
    MaxLevel: Maximum terrain height where grass or trees can be placed.
    MaxSteepness: The steepest slope on which grass or trees can be placed (higher values allow steeper placement).
    IslandsSize: Defines the size of areas where grass or trees are not placed; used to control the density of islands.
    Density: Controls how densely the grass or trees are placed within the allowed areas.

    - octaves should be between 0 and 8.
    - scale should be between 0 and 50.
    - lacunarity should be between 0 and 3.
    - persistence should be between 0 and 1.
    - offset should be between 0 and 1.
    - minLevel should be between 0 and 1.
    - maxLevel should be between 0 and 1.
    - maxSteepness should be between 0 and 90.
    - islandSize should be between -1 and 1.
    - density should be between 0 and 100.
    - randomize and autoUpdate should be true or false.
    - Grass Texture should be an integer representing the number of Grass Texture, typically between 1 and 10.

    TreeGenerator:
    Octaves: Determines the number of noise layers used for tree distribution (more octaves add finer details to the noise).
    Scale: Adjusts the scale of the noise map, affecting how spread out or compact the tree distribution will be.
    Lacunarity: Controls the increase in frequency for each noise octave, affecting the overall detail of tree distribution.
    Persistance: Defines how the amplitude of each noise octave diminishes, influencing the terrain's fine details for tree placement.
    Offset: A value used to shift the noise map, introducing randomness to the tree distribution.
    MinLevel: The minimum height at which trees can be placed.
    MaxLevel: The maximum height at which trees can be placed.
    MaxSteepness: The steepest slope on which trees can grow; trees won't appear on slopes steeper than this value.
    IslandsSize: Controls the areas where trees are placed based on noise values; lower values restrict trees to specific areas, while higher values spread trees across the terrain.
    Density: Determines how densely trees are placed on the terrain; higher values result in more trees.
    Randomize: If enabled, randomizes the noise offset to produce different tree layouts each time the map is generated.
    AutoUpdate: Automatically regenerates the trees whenever any parameter is changed in the inspector.
    for forest uslay its around Density 1, IslandsSize 1, MaxLevel 100.

    - octaves should be between 1 and 10.
    - scale should be between 0 and 100.
    - lacunarity should be between 0 and 3.
    - persistence should be between 0 and 1.0.
    - offset should be between 2000 and 10000.
    - minLevel should be between 0 and 100.
    - maxLevel should be between 0 and 100.
    - maxSteepness should be between 0 and 90.
    - islandSize should be between -1 and 1.
    - density should be between 0 and .3.
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
    print("API Response:", response.text)

    # Clean the response by removing any triple backticks if present
    return response.text.strip().strip('```json').strip('```')



if __name__ == '__main__':
    app.run(port=5000)
