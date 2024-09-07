from flask import Flask, request, jsonify
import google.generativeai as genai
import os
from dotenv import load_dotenv


load_dotenv()
genai.configure(api_key=os.environ["API_KEY"])
app = Flask(__name__)

model = genai.GenerativeModel("gemini-1.5-flash")

@app.route('/parse_description', methods=['POST'])
def parse_description():

    description = request.json.get('description')

    # class content should be a copy paste of the WorldInfo.cs file
    classContents = """
/*
 * Define parameters to the world generation function.
 * Each parameter should have a description stating how the ai
 * model should handle assigning it a value
 */
[System.Serializable]
public struct WorldInfo
{
    /*
     * Terrain Material must match a value from the TerrainMaterial enum reprsented as an integer
     */
    public TerrainMaterial TerrainMaterial;

    /*
     * Cloud Density must be a value in the range [0,1]
     */
    public float CloudDensity;

    public override readonly string ToString()
    {
        return $"\nTerrainMaterial: {TerrainMaterial}" +
            $"\nCloudDensity: {CloudDensity}";
    }
}

public enum TerrainMaterial : int
{
    None = 0,
    Grass = 1,
    Dirt = 2,
    Snow = 3,
    Sand = 4,
}"""

    prompt = f"""
    Parse the following description and extract detailed information needed for generating terrain in Unity.
   
    Description: "{description}"

    Format the output as a json object to match the following C# serializable class: {classContents}

    There should be no other text in the response other than the json object
    """

    response = model.generate_content(prompt)
    output = response.text

    return jsonify({
        "input": description,
        "output": output
    })


if __name__ == '__main__':
    app.run(port=5000)
