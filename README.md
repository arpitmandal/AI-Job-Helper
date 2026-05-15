# AI Job Helper

AI-powered job application assistant — resume analysis, ATS scoring, and cover letter generation.

Built with **.NET 10**, **Google Gemini AI**, and **MySQL 8**.

---

## Features

| Phase | Feature | Status |
|---|---|---|
| 1 | Comprehensive Resume Analysis | ✅ Complete |
| 2 | ATS Resume vs Job Description Scorer | Planned |
| 3 | Cover Letter Generator | Planned |

---

## Tech Stack

- **Runtime:** .NET 10 (ASP.NET Core Web API)
- **AI:** Google Gemini 2.5 Flash
- **Database:** MySQL 8+ via Pomelo EF Core
- **Docs:** Swagger UI at `/swagger`
- **Logging:** Serilog (Console + rolling file)
- **Platform:** macOS & Windows (cross-platform)
- **Cloud-ready:** Docker + 12-factor config for future AWS ECS/App Runner deployment

---

## Project Structure

```
AI-Job-Helper/
├── src/
│   ├── AIJobHelper.API/          # Controllers, Program.cs, Swagger, health checks
│   ├── AIJobHelper.Application/  # Services, interfaces, DTOs
│   ├── AIJobHelper.Domain/       # Entities
│   └── AIJobHelper.Infrastructure/  # EF Core, Gemini client, PDF/DOCX parsers
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- MySQL 8+ installed locally
  - macOS: `brew install mysql && brew services start mysql`
  - Windows: [MySQL Installer](https://dev.mysql.com/downloads/installer/)
- Google Gemini API key — [Get one here](https://aistudio.google.com/app/apikey)

---

## Local Setup (without Docker)

**1. Clone and configure secrets**
```bash
cd src/AIJobHelper.API
dotnet user-secrets set "Gemini:ApiKey" "your-key-here"
```

**2. Update the connection string**

Edit `src/AIJobHelper.API/appsettings.json`:
```json
"ConnectionStrings": {
  "Default": "Server=localhost;Port=3306;Database=ai_job_helper;User=root;Password=your-password;"
}
```

**3. Run the API**
```bash
dotnet run --project src/AIJobHelper.API
```

cd "/Users/arpitmandal/Desktop/Work Projects/AI-Job-Helper"
dotnet build
dotnet run --project src/AIJobHelper.API

cd "/Users/arpitmandal/Desktop/Work Projects/AI-Job-Helper" && dotnet run --project src/AIJobHelper.API 2>&1 &
sleep 8 && curl -s http://localhost:5098/health

pkill -f "AIJobHelper.API"


The app auto-applies EF Core migrations on startup in Development mode.

**4. Open Swagger UI**
```
http://localhost:5000/swagger
```

---

## Local Setup (with Docker)

```bash
cp .env.example .env
# Edit .env with your MYSQL_ROOT_PASSWORD and GEMINI_API_KEY

docker compose up --build
```

API available at `http://localhost:8080/swagger`

---

## Running Migrations Manually

```bash
dotnet tool install --global dotnet-ef

dotnet ef database update \
  --project src/AIJobHelper.Infrastructure \
  --startup-project src/AIJobHelper.API
```

---

## Phase 1 — Resume Analysis API

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/resumes/upload` | Upload PDF or DOCX resume |
| POST | `/api/resumes/{id}/analyze` | Run AI analysis |
| GET | `/api/resumes/{id}/analysis` | Get stored analysis |
| GET | `/api/resumes` | List all resumes |
| DELETE | `/api/resumes/{id}` | Delete resume + analysis |

### Analysis response includes:
- **Summary** — 2-3 sentence professional overview
- **Strengths** — key qualifications and experience highlights
- **Weaknesses** — areas for improvement
- **Suggestions** — recommended job titles

---

## Health Checks

- `GET /health` — liveness check
- `GET /health/ready` — readiness check (includes DB connectivity)

---

## Environment Variables (cloud deployment)

| Variable | Description |
|---|---|
| `ConnectionStrings__Default` | MySQL connection string |
| `Gemini__ApiKey` | Google Gemini API key |
| `Gemini__Model` | Model name (default: `gemini-2.5-flash`) |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |

---

## AWS Deployment (future)

The app is cloud-native ready:
- Stateless (no session state)
- Config from env vars → maps to AWS SSM / Secrets Manager
- MySQL → AWS RDS (MySQL 8 compatible)
- File storage abstracted via `IFileStore` → swap to S3 implementation
- Docker image → push to ECR → deploy to ECS Fargate or App Runner
- Serilog → add `Serilog.Sinks.AwsCloudWatch` for CloudWatch Logs
