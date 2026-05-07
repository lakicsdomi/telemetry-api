# Telemetry API Project
![Coverage](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/coverage.svg)
![Pipeline Status](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/pipeline.svg)

A modern, containerized .NET 9 Minimal API equipped with a full-fledged observability and monitoring stack. The project demonstrates the "Configuration as Code" (CaC) and "Alerting as Code" approaches using Docker, Prometheus, and Grafana, while maintaining robust testing and automated CI/CD pipelines deploying directly to AWS.

## 🚀 Key Features

-   **Telemetry Minimal API:** A high-performance C# .NET 9 REST API that provides real-time system metrics (CPU, Memory, Uptime) via a `/status` endpoint.
    
-   **Automated CI/CD & AWS Deployment:** Fully integrated GitLab pipeline for automated testing, multi-stage Docker build optimization, and continuous deployment (CD) to an AWS EC2 instance using Ansible.
    
-   **Documentation:** Automatically generated Doxygen documentation hosted on GitLab Pages.
    
-   **Automated Metrics Exposure:** Native integration with `prometheus-net`, automatically collecting and exposing HTTP request metrics at the `/metrics` endpoint.
    
-   **Infrastructure as Code (Docker & Ansible):** The entire application and monitoring stack can be spun up using Docker Compose, while server provisioning and deployment are fully automated via Ansible playbooks.
    
-   **Provisioned Observability (Grafana & Prometheus):**
    -   **Zero-Click Setup:** Data sources, contact points, and dashboards are automatically provisioned on startup.
    -   **Alerting as Code:** Pre-configured PromQL-based alert rules (e.g., detecting `404 Not Found` error spikes) defined entirely in YAML.
    -   **Automated Notifications:** SMTP integration for instant email delivery when alert thresholds are breached.
        
-   **Traffic Simulation:** Includes a custom Python script (`traffic_generator.py`) to simulate user behavior and generate load/errors for testing purposes.

## 🛠️ Tech Stack

**Application & Testing:**
-   Backend: .NET 9.0 (C#), ASP.NET Core Web API
-   Testing: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing
-   Scripting: Python 3 (Requests library for load generation)
    
**Observability & Monitoring:**
-   Prometheus (Metrics scraping & Time-series DB)
-   Grafana (Data visualization & Alerting engine)
-   PromQL (Alert rule queries)
    
**Infrastructure & DevOps:**
-   AWS (EC2)
-   Ansible
-   Docker & Docker Compose
-   GitLab CI/CD
-   YAML (Provisioning & Configuration)
    
-   **Provisioned Observability (Grafana & Prometheus):**
    -   **Zero-Click Setup:** Data sources, contact points, and dashboards are automatically provisioned on startup.
    -   **Alerting as Code:** Pre-configured PromQL-based alert rules (e.g., detecting `404 Not Found` error spikes) defined entirely in YAML.
    -   **Automated Notifications:** SMTP integration for instant email delivery when alert thresholds are breached.
        
-   **Traffic Simulation:** Includes a custom Python script (`traffic_generator.py`) to simulate user behavior and generate load/errors for testing purposes.

## 🛠️ Tech Stack

**Application & Testing:**
-   Backend: .NET 9.0 (C#), ASP.NET Core Web API
-   Testing: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing
-   Scripting: Python 3 (Requests library for load generation)
    
**Observability & Monitoring:**
-   Prometheus (Metrics scraping & Time-series DB)
-   Grafana (Data visualization & Alerting engine)
-   PromQL (Alert rule queries)
    
**Infrastructure & DevOps:**
-   Docker & Docker Compose
-   GitLab CI/CD
-   YAML (Provisioning & Configuration) 

## ⚙️ How to Run

### 1. Prerequisites

-   Docker and Docker Desktop (or Docker Engine) installed.
-   Python 3.x (for the traffic generator).

### 2. Environment Variables

Create a `.env` file in the root directory to configure the Grafana SMTP settings for email alerts:

```env
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