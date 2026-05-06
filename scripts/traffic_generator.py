'''
This script simulates API traffic by sending a mix of valid and invalid requests to the API server.
It randomly chooses between valid endpoints ("/status", "/metrics") and 
invalid endpoints ("/invalid", "/notfound", "/dogsandcats") to test the API's response handling.
The script runs indefinitely until interrupted by the user (CTRL+C).
Make sure to have the API server running (e.g., via Docker) before executing this script.
'''

import requests
import time
import random

API_BASE_URL = "http://localhost:8080"
VALID_ENDPOINTS = ["/status", "/metrics"]
INVALID_ENDPOINTS = ["/invalid", "/notfound", "/dogsandcats"]

def send_request():
    # 30% chance for invalid request, 70% for valid request
    if random.random() < 0.3:
        endpoint = random.choice(INVALID_ENDPOINTS)
    else:
        endpoint = random.choice(VALID_ENDPOINTS)
    
    url = f"{API_BASE_URL}{endpoint}"

    try:
        start = time.time()
        response = requests.get(url, timeout=2)
        duration = time.time() - start
        print(f"[{time.strftime('%X')}] GET {url} - Status: {response.status_code} ({duration:.2f}s)")
    except requests.exceptions.ConnectionError:
        print(f"[{time.strftime('%X')}] ERROR: Can't connect to API. Is Docker running?")
    except requests.exceptions.Timeout:
        print(f"[{time.strftime('%X')}] ERROR: Request timed out")

if __name__ == "__main__":
    print("API Traffic Generator is starting...")
    print("Press CTRL+C to stop.\n")

    try:
        while True:
            send_request()
            time.sleep(random.uniform(0.1, 1.0))  # Random delay between requests
    except KeyboardInterrupt:
        print("\nAPI Traffic Generator stopped.")