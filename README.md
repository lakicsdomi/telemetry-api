# Telemetry API Project
![Coverage](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/coverage.svg)
![Pipeline Status](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/pipeline.svg)

A modern, containerized .NET 9 Minimal API equipped with a full-fledged observability and monitoring stack. The project demonstrates the "Configuration as Code" (CaC) and "Alerting as Code" approaches using Docker, Prometheus, and Grafana.

## 🚀 Key Features

-   **Telemetry Minimal API:** A lightweight C# .NET 9 REST API that exposes system and application status via a `/status` endpoint.
    
-   **Automated Metrics Exposure:** Native integration with `prometheus-net`, automatically collecting and exposing HTTP request metrics at the `/metrics` endpoint.
    
-   **Traffic Simulation:** Includes a custom Python script (`traffic_generator.py`) to simulate user behavior and generate load/errors for testing purposes.
    
-   **Infrastructure as Code (Docker):** The entire application, database (coming soon), and monitoring stack can be spun up using a single `docker compose up -d` command.
    
-   **Provisioned Observability (Grafana & Prometheus):**
    
    -   **Zero-Click Setup:** Data sources, contact points, and dashboards are automatically provisioned on startup.
        
    -   **Alerting as Code:** Pre-configured PromQL-based alert rules (e.g., detecting `404 Not Found` error spikes) defined entirely in YAML.
        
    -   **Automated Notifications:** SMTP integration for instant email delivery when alert thresholds are breached.
        

## 🛠️ Tech Stack
**Application & Scripting:**

-   .NET 9 (C# Minimal API)
    
-   Python 3 (Requests library for load generation)
    
**Observability & Monitoring:**

-   Prometheus (Metrics scraping & Time-series DB)
    
-   Grafana (Data visualization & Alerting engine)
    
-   PromQL (Alert rule queries)
    
**Infrastructure & DevOps:**

-   Docker & Docker Compose
    
-   YAML (Provisioning & Configuration) 

## ⚙️ How to Run

### 1. Prerequisites

-   Docker and Docker Desktop (or Docker Engine) installed.
    
-   Python 3.x (for the traffic generator).
    

### 2. Environment Variables

Create a `.env` file in the root directory to configure the Grafana SMTP settings for email alerts:


```
SMTP_PASSWORD=your_16_character_google_app_password

```

### 3. Spin up the Stack

Build the .NET image and start the containers:


```bash
docker compose down -v
docker compose up -d --build

```

### 4. Test the Alerting System

Run the Python traffic generator to simulate requests and 404 errors. This will trigger the Grafana alert rule automatically.


```bash
pip install requests
python scripts/traffic_generator.py

```

### 📍 Endpoints

-   **API Status:** `http://localhost:8080/status`
    
-   **Prometheus Metrics:** `http://localhost:8080/metrics`

-   **Prometheus Query:** `http://localhost:9090`
    
-   **Grafana Dashboard:** `http://localhost:3000` (Default login is bypassed via anonymous Admin access)