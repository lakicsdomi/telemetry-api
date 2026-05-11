# Telemetry API Project

![Coverage](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/coverage.svg)
![Pipeline Status](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/pipeline.svg)

A modern, containerized .NET 9 Minimal API equipped with a full-fledged observability and monitoring stack. The project demonstrates the "Configuration as Code" (CaC) and "Alerting as Code" approaches using Docker, Prometheus, and Grafana, while maintaining robust testing and automated CI/CD pipelines deploying directly to AWS.

## 🚀 Key Features

* **Telemetry Minimal API:** A high-performance C# .NET 9 REST API that provides real-time system metrics (CPU, Memory, Uptime) via a `/status` endpoint.

* **Infrastructure as Code (Terraform):** Cloud infrastructure provisioning is fully automated. The AWS EC2 instance, VPC, and Security Groups are defined and deployed using Terraform.

* **Automated CI/CD & Configuration Management:** Fully integrated GitLab pipeline for automated testing, multi-stage Docker build optimization, and continuous deployment (CD). Server configuration and application deployment are managed by Ansible.

* **Documentation:** Automatically generated Doxygen documentation hosted on GitLab Pages.

* **Automated Metrics Exposure:** Native integration with `prometheus-net`, automatically collecting and exposing HTTP request metrics at the `/metrics` endpoint.

* **Containerized Stack:** The entire application and monitoring stack can be spun up locally or remotely using a single `docker compose up -d` command.

* **Provisioned Observability (Grafana & Prometheus):**

  * **Zero-Click Setup:** Data sources, contact points, and dashboards are automatically provisioned on startup.
  * **Alerting as Code:** Pre-configured PromQL-based alert rules (e.g., detecting `404 Not Found` error spikes) defined entirely in YAML.
  * **Automated Notifications:** SMTP integration for instant email delivery when alert thresholds are breached.

* **Docker-Native HTTP Request Logging:** Structured HTTP request and response logging using ASP.NET Core `HttpLoggingMiddleware`. Logs are written directly to container stdout/stderr, making them fully compatible with Docker logging drivers and future AWS CloudWatch integration.

* **Security Hardening:**

  * Grafana and Prometheus dashboards are restricted to trusted developer IP addresses.
  * Fail2Ban protection is automatically configured on the EC2 instance to mitigate brute-force attacks.
  * Sensitive Terraform artifacts (`terraform.tfvars`, `*.tfplan`, state crash logs) are excluded from version control.
  * HTTP header values are automatically redacted by ASP.NET Core logging middleware to reduce accidental sensitive data exposure.
  * Prometheus metrics are completely isolated on a dedicated internal port, preventing external unauthorized access.

* **Advanced Deployment Workflow:**

  * DRY GitLab CI/CD pipeline design using reusable deployment templates.
  * Manual dry-run deployments (`ansible-playbook --check`) available for feature branches before production rollout.
  * Automated deployment to AWS on the `main` branch after successful test and build stages.

* **Traffic Simulation:** Includes a custom Python script (`traffic_generator.py`) to simulate user behavior and generate load/errors for testing purposes.

## 🛠️ Tech Stack

**Application & Testing:**

* Backend: .NET 9.0 (C#), ASP.NET Core Web API
* Testing: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing
* Scripting: Python 3 (Requests library for load generation)

**Observability & Monitoring:**

* Prometheus (Metrics scraping & Time-series DB)
* Grafana (Data visualization & Alerting engine)
* PromQL (Alert rule queries)
* ASP.NET Core HttpLogging Middleware

**Infrastructure & DevOps:**

* Cloud: AWS (EC2)
* IaC & Provisioning: Terraform
* Configuration Management: Ansible
* Containerization: Docker & Docker Compose
* CI/CD: GitLab CI/CD
* Security: Fail2Ban, SSH Key Authentication

## ⚙️ How to Run

### 1. Prerequisites

* Docker and Docker Desktop (or Docker Engine) installed.
* Python 3.x (for the traffic generator).
* *(Optional)* Terraform installed for AWS infrastructure provisioning.

### 2. Environment Variables

Create a `.env` file in the root directory to configure the Grafana SMTP settings for email alerts:

```env
SMTP_PASSWORD=your_16_character_google_app_password
```

### 3. Spin up the Stack (Locally)

Build the .NET image and start the containers:

```bash
docker compose down -v
docker compose up -d --build
```

### 4. Inspect Container Logs

The API uses Docker-native stdout/stderr logging, so logs can be inspected directly through Docker:

```bash
docker compose logs -f [server-container-name]
```

This approach follows modern container best practices ("Twelve-Factor App" methodology), where application logs are streamed to stdout instead of written into files inside containers.

### 5. Test the Alerting System

Run the Python traffic generator to simulate requests and 404 errors. This will trigger the Grafana alert rule automatically.

```bash
pip install requests
python scripts/traffic_generator.py
```

### 6. Infrastructure Provisioning (Optional)

Provision the AWS infrastructure using Terraform:

```bash
cd terraform

terraform init

terraform plan \
  -var='developer_ips=["YOUR_IP/32"]'

terraform apply \
  -var='developer_ips=["YOUR_IP/32"]'
```

### 7. CI/CD Deployment Workflow

* Every commit triggers automated testing and dependency scanning through GitLab CI/CD.
* Feature branches support optional dry-run deployments using Ansible’s `--check` mode.
* Successful commits to the `main` branch automatically deploy the application stack to AWS EC2.

### 📍 Endpoints

* **API Status (Public):** `http://localhost:8080/status`

* **Prometheus Metrics (Internal):** `http://server:9091/metrics` *(Isolated on the internal Docker network for security)*

* **Prometheus Query:** `http://localhost:9090`

* **Grafana Dashboard:** `http://localhost:3000` **(Access restricted to developer IPs)**

### 🔐 Security Notes

* SSH access is protected using SSH key authentication and Fail2Ban monitoring.
* Grafana and Prometheus endpoints are restricted via Terraform-managed Security Groups.
* Terraform secrets and local state artifacts are excluded from Git tracking using `.gitignore`.
* HTTP request logging uses ASP.NET Core’s built-in redaction support to prevent sensitive header values from being exposed in logs.
* The application runs inside Docker containers using the non-root default ASP.NET runtime user for improved container security.
