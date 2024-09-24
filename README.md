# EzWorld

OSU AI Capstone creating a unity world from a text prompt 
This project allows users to generate game terrains via a Flask-based web interface, with the backend powered by Google Generative AI. The Unity engine renders the terrains based on the JSON data from the server.

## Setup Instructions

### Prerequisites

- Python 3.x
- Unity 2020.x or higher
- [Google Generative AI](https://ai.google) API key

### Server Setup (Flask)

    1. Clone the repository and navigate to the `ai_server/` directory:
    ```bash
    cd project_root/ai_server

### Install Python dependencies

    pip install -r requirements.txt

### Create a .env file in the ai_server/ directory and add your API key

    API_KEY=your_google_generative_ai_key

### Run the Flask server

 python server.py
