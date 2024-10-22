import zipfile
from flask import Flask, flash, redirect, render_template, request, jsonify, send_file, abort
import google.generativeai as genai
import os
from dotenv import load_dotenv
import json
from flask_cors import CORS

# Load environment variables
load_dotenv()

# Configure Google Generative AI with the API key
genai.configure(api_key=os.environ.get("API_KEY"))
BASE_DIR = os.path.abspath(os.path.dirname(__file__))
APP_FOLDER = os.path.join(BASE_DIR, 'static', 'APP')
ZIP_PATH = os.path.join(BASE_DIR, 'static', 'APP.zip')

app = Flask(__name__)
CORS(app)

# Initialize the Gemini model
model = genai.GenerativeModel("gemini-1.5-flash")

# Serve the HTML page with the WebGL game and the input form
@app.route('/')
def index():
    return render_template('index.html')

# Play Route
@app.route('/play')
def play():
    return render_template('play.html')

# Guide Route
@app.route('/guide')
def guide():
    return render_template('guide.html')

# Contact Us Route
@app.route('/contact')
def contact():
    if request.method == 'POST':
        name = request.form['name']
        phone = request.form['phone']
        email = request.form['email']
        message = request.form['message']

        # Store the data or send an email as needed
        flash('Your message has been sent successfully!', 'success')
        return redirect('/contact')

    return render_template('contact.html')

# Download Route
@app.route('/download')
def download():
    return render_template('download.html')

@app.route('/download_app')
def download_app():
    # Create the ZIP file if it doesn't exist
    if not os.path.exists(ZIP_PATH):
        try:
            with zipfile.ZipFile(ZIP_PATH, 'w') as zipf:
                for root, _, files in os.walk(APP_FOLDER):
                    for file in files:
                        file_path = os.path.join(root, file)
                        # Add files to the ZIP with relative paths
                        zipf.write(file_path, os.path.relpath(file_path, APP_FOLDER))
        except Exception as e:
            print(f"Error creating ZIP: {e}")
            abort(500, description="Internal Server Error: Could not create ZIP archive.")

    # Serve the ZIP file for download
    if os.path.exists(ZIP_PATH):
        return send_file(ZIP_PATH, as_attachment=True)
    else:
        abort(404, description="File not found.")


@app.route('/parse_description', methods=['POST'])
def parse_description():

    description = request.json.get('description')

    if not description:
        return jsonify({"error": "Description is required."}), 400
    
    object_set = {"Brick House", "Ferris Wheel", "Small House"}

    # Create the prompt for the AI
    prompt = f"""
    You are a terrain generation AI for a game. Based on the user's description, 
    return a JSON object with parameters required for generating terrain. 
    Make sure that the values for each parameter fall within reasonable ranges to avoid any out-of-bounds issues. 
    If the description for the terrain does not need tree or grass like dessert(height 162, octaves 8, ) or snow unles the user specicly said, do not add( make the values zero's).
    Make sure if the user say dont add <object> make sure to set the value to zero, for exampl if the user say dont add tree or grass or water(etc) make it zero.
    Please use the following guidelines for each module:
    

    ### Rules:
    1. **City vs Terrain:**
    - If the user mentions a city or related attributes (e.g., traffic system, downtown, satellite city), include **both** the **"cityData"** and **"terrainsData"** fields. Ensure that **TreeGenerator** parameters (like trees) are all set to **zero** for city generation.
    - If the user only describes terrain features (e.g., mountains, rivers, deserts and more) without mentioning a city, **only include the "terrainsData"** field. **Remove the "cityData"** field if terrain is the primary focus.
    - If the user requests multiple cities, set `"withSatelliteCity": true` to avoid duplicating the **"cityData"** field.
    - Make sure to *remove* "cityData" from JSON if the user wants "terrainsData".
    - Make sure to *Keep* "terrainsData" in JSON if the user wants "cityData" but make sure to set all the values to zeros.

    2. **Logical Terrain Placement:**
    - **Deserts:** No trees or grass should appear in the desert. Set `TreeGenerator` and `GrassGenerator` parameters to **zero**.
    - **Snow:** Trees and grass should only appear if the player explicitly mentions it. Default these parameters to **zero** unless specified otherwise.
    - **Mountains:** Place sparse vegetation if required, but only at lower elevations. Use logical constraints (e.g., lower density of grass and trees on slopes).
    - **Grass Fields / Forests:** Allow denser vegetation, but ensure it aligns logically (e.g., rivers and lakes can co-exist).
    - For terrain textures (e.g., "snow", "sand", "forestFloor"), **the most important texture should appear prominently** and be the focus.
    - Use **"easeIn"** or **"bezier"** or **"sine"** or **"easeout"** curves to emphasize primary textures.
    - Use **"linear"** or **"easeout"** for flat surfaces.
    - For less important or secondary textures, apply **"constant"**, or **"linear"** curves to minimize their presence.
    - Ensure **no two textures** use the **same curve type** for more variety.

    3. **Handling Absence of Objects:**
    - If the user says **"don't add"**(eg **"no trees"**, **"no grass"**, or **"no water"**) a specific object (e.g., water, trees, grass), set all corresponding generator parameters to **zero**.
    - Ensure that the generated parameters logically align with the environment (e.g., snow with minimal vegetation, deserts with no water or vegetation).


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
    UseFalloffMap: Toggles the use of a falloff map to control terrain generation near edges(Always False).
    Randomize: Enables randomization of the noise offset to generate different terrains each time.
    AutoUpdate: Automatically updates the terrain when changes are made in the inspector.

    - width and height should be around 1024 (minimum 512, maximum 1024).
    - depth represents the terrain height and should be between 65 and 200.
    - octaves represent the levels of detail and should be between 1 and 15.
    - scale determines the level of detail and should range between 70 and 500.
    - lacunarity affects how much detail is added at each octave, typically between 1 and 5.
    - persistence controls how each octave contributes to the overall shape, typically between 0 and 0.2.
    - heightCurve: Choose from ["linear", "constant", "easeIn", "easeOut", "sine", "bezier"].  Rate of change of heights curve.
    - heightCurveOffset is the vertical offset of the height curve, usually between 5000 and 12000.
    - falloffDirection affects the direction of terrain slopes, usually between 1 and 4.
    - falloffRange affects the slope of the terrain, usually between 1 and 4.
    - useFalloffMap should be true or false. mostly true
    - randomize and autoUpdate should be true or false.

    TexturesGenerator:
    - texture should be a list of textures (comma-separated) that can include any of the following it should be only for the terrain:
    - Water is not part of terrain TexturesGenerator
    ["grass", "desert", "snow", "mud", "rock", "sand", "forestFloor", "mountainRock", "dirt", "deadGrass"].
    - The user may ask for multiple textures, so the model should include more than one texture if necessary. 
    For example, "desert, deadGrass" for a desert with patches of dead grass, or "snow, rock" for snowy mountains.
    - Each texture should have its own properties: 
        -  make sire
        - `heightCurve`: Choose from ["linear", "constant", "easeIn", "easeOut", "sine", "bezier"]. These represent the curve type for how the texture is applied based on terrain height.
        - `tileSizeX`: float, between 0 and 50 (determines how the texture is tiled on the X axis).
        - `tileSizeY`: float, between 0 and 50 (determines how the texture is tiled on the Y axis).
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

    - octaves should be between 0 and 4.
    - scale should be between 0 and 25.
    - lacunarity should be between 0 and 3.
    - persistence should be between 0 and 1.
    - offset should be between 1000 and 10000.
    - minLevel should be between -200 and -90.
    - maxLevel should be between 90 and 200.
    - maxSteepness should be between 50 and 90.
    - islandSize should be between 0.5 and 1.
    - density should be between 700 and 1000.
    - randomize and autoUpdate should be true or false.
    - Grass Texture should be an integer representing the number of Grass Texture, typically between 4 and 10.

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
    Density: Determines how densely trees are placed on the terrain; higher values result in more trees 0.0-1.0.
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
    - waterLevel represents the level of water height for lakes or oceans, should be between 50 and 0.
    - river width range x and y, x should be between 100 and 100, y should be between 100 and 1000 
    - randomize and autoUpdate should be true or false.
    
    CityGenerator: if the user wantd city always generate building
    - If the user mentions generating a city, include the following parameters in the output:
    - citySize: Choose from [Small, Medium, Large, Very Large]
    - withSatelliteCity: Boolean, whether the city should have a satellite connected by a highway
    - borderFlat: Boolean, whether the city should have flat surroundings(Mostly False)
    - withDowntownArea: Boolean, whether the city should have a downtown metropolitan area
    - downtownSize: Float 0-200, represents the size of the downtown area
    - addTrafficSystem: Boolean, whether a traffic system should be added(Mostly True)
    - trafficHand: Choose between [RightHand, LeftHand]
    
    ObjectData:
    Multiplle copies of objects are allowed to be generated at a time.
    - name: The name of the object. Must be in {object_set}
    - x: The x position of the center of the object, 0<x<1024
    - y: The y position of the center of the object, 0<y<1024
    - Rx: The rotation of the object around the x axis, 0<Rx<360
    - Ry: The rotation of the object around the y axis, 0<Ry<360
    - Rz: The rotation of the object around the z axis, 0<Rz<360
    - scale: The scale of the model size, as a multiple of the model, 0 < scale < 4, typically 1

    AtmosphereGenerator:
    - timeOfDay is a floating point value representing the time of day. Its value should be between 0 and 24 inclusive with 0 and 24 representing 12:00am, 12 representing 12:00pm and so on. 
    - sunSize is a floating point value representing the size of the sun. Its value should range from 0 to 1 inclusive and the standard sun size is .05.
    - skyTint is a color defined by RGB values each ranging from 0 to 1. Default sky color should be r=.5, g=.5, b=.5
    - atmosphericThickness is a float ranging from 0-5 inclusive. The standard value is 1.
    - exposure is a float ranging from 0-8 inclusive. The standard value is 1.3 and this controls the overall light intensity coming from the sun in the skybox
    - fogIntensity is a float ranging from 0-.5 inclusive. 0 implies no fog and .5 implies extremely foggy. Unless specified, 0 fog intensity should be the default.
    - fogColor is a color defined by RGB values each ranging from 0 to 1. Default fog color should be r=.5, g=.5, b=.5

    Make sure you return the result in JSON format like this:   
    {{
        "terrainsData": [
            {{
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
                    }}
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
                    "density": float,
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
            }},
            ...
        ],
        "atmosphereGeneratorData": {{
            "timeOfDay": float,
            "sunSize": float,
            "skyTint":{{
                "r": float,
                "g": float,
                "b": float
            }}
            "atmosphericThickness": float,
            "exposure": float,
            "fogIntensity": float,
            "fogColor": {{
                "r": float,
                "g": float,
                "b": float
            }}
        }},
        "objectList": [
        {{
            "name": string,
            "x": float,
            "y": float,
            "Rx": float,
            "Ry": float,
            "Rz": float,
            "scale": float
        }},
        ...
        ]
        "cityData": {{
            "citySize": string,
            "withSatelliteCity": boolean,
            "borderFlat": boolean,
            "withDowntownArea": boolean,
            "downtownSize": float,
            "addTrafficSystem": boolean,
            "trafficHand": string
        }},

    }}

    The structure should be well-formed, and all boolean values should be correctly set.
    Use the following description to generate appropriate values:
    "{description}"
    """
    
    # Call the Gemini API to generate the content
    response = model.generate_content(prompt)

    # Log the raw API response for debugging
    print("API Response:", response.text)

    # Clean the response by removing any triple backticks if present
    cleaned_response = response.text.strip().strip('```json').strip('```')

    # Load the cleaned response into a Python dictionary
    try:
        generated_data = json.loads(cleaned_response)
    except json.JSONDecodeError as e:
        print(f"Error parsing JSON: {e}")
        return jsonify({"error": "Invalid JSON response from AI."}), 500

    # # Enforce the rule: either cityData or terrainsData, not both
    # if "cityData" in generated_data and "terrainsData" in generated_data:
    #     print("Both cityData and terrainsData detected. Prioritizing cityData.")
    #     del generated_data["terrainsData"]  # Remove terrainsData to prioritize cityData

    return jsonify(generated_data)



if __name__ == '__main__':
    app.run(port=5000)
