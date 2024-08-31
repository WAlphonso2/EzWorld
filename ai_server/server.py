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

    prompt = f"""
    Parse the following description and extract detailed information needed for generating terrain in Unity.
   
    Description: "{description}"

    Format the output as:
    Terrain Type: [type of terrain, e.g., mountain, desert, rock, mud, flat, shallow, etc.]
    """

    response = model.generate_content(prompt)
    output = response.text

    return jsonify({
        "input": description,
        "output": output
    })


if __name__ == '__main__':
    app.run(port=5000)
