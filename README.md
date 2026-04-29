# Greenhouse Automation System
Cloud-based simulation of an automated greenhouse where robots manage irrigation, harvesting, and charging through parallel task scheduling.

## Tech Stack
- .NET API (C#)
- PostgreSQL
- Docker
- AWS (EC2, RDS)
- xUnit

## Overview
This project models a greenhouse environment where crops and robots evolve over time.  
As system conditions change, tasks are generated and processed concurrently, with robots selecting the highest-priority work available.

## Key Features
- Parallel task generation (irrigation, harvesting, charging)
- Priority-based task selection by autonomous robots
- Real-time system state via local dashboard
- PostgreSQL-backed persistence
- Dockerized deployment on AWS
- Unit + integration testing with xUnit

## System Logic (Simplified)
- Crops: moisture decreases, growth increases over time  
- Robots: battery decreases over time  
- Thresholds trigger task creation  
- Robots continuously select highest-priority tasks  

## Status
**In development**
- Expanding end-to-end testing  
- Adding concurrency/stress testing  

## Run Locally
```bash
git clone https://github.com/your-username/greenhouse-system.git
cd greenhouse-system
docker-compose up --build
