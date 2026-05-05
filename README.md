# Telemetry API Project
![Coverage](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/coverage.svg)
![Pipeline Status](https://gitlab.com/lakicsdomi-portfolio/telemetry-api/badges/main/pipeline.svg)

## Project Goal
This project is a high-performance .NET 9.0 based Telemetry API designed to demonstrate modern DevOps practices. 
The main goal is to provide real-time system metrics (CPU, Memory, Uptime) for monitoring tools like **Prometheus** and **Grafana**, while maintaining a fully automated CI/CD pipeline.

## Key Features
*   **Automated CI/CD**: Fully integrated GitLab pipeline for testing and building.
*   **Containerized**: Docker-ready architecture with multi-stage build optimization.
*   **Observability**: Built-in Prometheus metrics export.
*   **Documentation**: Automatically generated Doxygen documentation hosted on GitLab Pages.

## Tech Stack
*   **Backend**: .NET 9.0 (C#), ASP.NET Core Web API
*   **Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing
*   **DevOps**: Docker, GitLab CI/CD, Prometheus, Grafana