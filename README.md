# AI Job Helper

An AI-powered job application assistant that helps you analyse your resume, score it against job descriptions, and generate tailored cover letters — all in one web application.

Built with **.NET 10**, **Google Gemini 2.5 Flash**, and **MySQL 8**.

---

## Quick Start

Assumes MySQL is running and credentials are set in `appsettings.json` (see [Project Setup](#project-setup-step-by-step) for first-time config).

```bash
# 1. Build
dotnet build AIJobHelper.slnx

# 2. Apply database migrations
dotnet ef database update \
  --project src/AIJobHelper.Infrastructure \
  --startup-project src/AIJobHelper.API

# 3. Run
dotnet run --project src/AIJobHelper.API
```

App is available at **http://localhost:5098** — Swagger UI at **http://localhost:5098/swagger**.

---

## Features

### Phase 1 — Resume Analysis
- Upload a resume in **PDF or DOCX** format (up to 10 MB)
- AI extracts and returns a professional **summary**, **strengths**, **weaknesses**, and **suggested job titles**
- Manage your resume library — view parsed text, re-run analysis, delete resumes

### Phase 2 — ATS Scorer
- Paste a job description as text, provide a URL, or pick one already saved in the database
- Select or upload a resume and run an ATS comparison
- Returns a **score (0–100)**, matched skills, missing skills, specific resume edits to clear the ATS, and an AI summary
- Saves every result so you can compare over time

### Phase 3 — Cover Letter Generator
- Save and reuse **named configurations** (AI instructions, header template, footer template)
- Generate a tailored cover letter body using Gemini AI
- Preview the letter in the browser — **all three sections are editable** before downloading
- Download as a professionally formatted **PDF**

### Job Description Manager
- View all saved job descriptions (from ATS Scorer or Cover Letter generator)
- Delete job descriptions you no longer need (associated ATS results are removed automatically)

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 — ASP.NET Core |
| UI | Razor Pages (server-rendered, Bootstrap 5) |
| AI | Google Gemini 2.5 Flash (REST via HttpClient) |
| Database | MySQL 8+ via Pomelo EF Core 9 |
| PDF generation | QuestPDF (Community licence) |
| Resume parsing | PdfPig (PDF) · DocumentFormat.OpenXml (DOCX) |
| URL scraping | HtmlAgilityPack |
| Logging | Serilog — Console + rolling daily file |
| API docs | Swagger UI (Swashbuckle) |
| Containerisation | Docker + docker-compose |

---

## Prerequisites (fresh system)

Install the following before running the project:

### 1. .NET 10 SDK
Download from https://dotnet.microsoft.com/download/dotnet/10.0

Verify after installing:
```bash
dotnet --version
# should print 10.x.x
```

### 2. MySQL 8+

**macOS (Homebrew)**
```bash
brew install mysql
brew services start mysql
mysql_secure_installation   # set a root password
```

**Windows**
Download the MySQL Installer from https://dev.mysql.com/downloads/installer/ and run it.
Choose "Developer Default" or at minimum the MySQL Server component.

Verify after installing:
```bash
mysql --version
# should print  8.x.x
```

### 3. Google Gemini API Key
Go to https://aistudio.google.com/app/apikey, sign in with a Google account, and create a new API key.
Keep it handy — you will store it as a user secret (never in source code).

### 4. EF Core CLI tool (for running migrations manually)
```bash
dotnet tool install --global dotnet-ef
```

---

## Project Setup (step by step)

### Step 1 — Configure the database connection

Open `src/AIJobHelper.API/appsettings.json` and update the connection string with your MySQL credentials:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Port=3306;Database=ai_job_helper;User=root;Password=YOUR_PASSWORD;"
}
```

Create the database in MySQL if it does not already exist:
```bash
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS ai_job_helper;"
```

### Step 2 — Store your Gemini API key (user secrets — never committed to git)

```bash
cd src/AIJobHelper.API
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
```

Verify it was saved:
```bash
dotnet user-secrets list
# Gemini:ApiKey = AIza...
```

### Step 3 — Restore packages and build

Run from the repository root (`AI-Job-Helper/`):
```bash
dotnet restore
dotnet build
```

A successful build prints:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Database Migrations

Migrations are applied **automatically on startup** when the environment is `Development`.

To apply them manually (or on a new machine before first run):
```bash
dotnet ef database update \
  --project src/AIJobHelper.Infrastructure \
  --startup-project src/AIJobHelper.API
```

To list all migrations:
```bash
dotnet ef migrations list \
  --project src/AIJobHelper.Infrastructure \
  --startup-project src/AIJobHelper.API
```

Migrations in this project:
| Migration | Contents |
|---|---|
| `InitialCreate` | Resumes, ResumeAnalyses |
| `Phase2_Ats` | JobDescriptions, AtsResults |
| `Phase3_CoverLetter` | CoverLetterConfigs, CoverLetters |
| `AddJobDescriptionTitle` | Title column on JobDescriptions |

---

## Running the Application

### Standard run (recommended)

From the repository root:
```bash
dotnet run --project src/AIJobHelper.API
```

The launch profile sets `ASPNETCORE_ENVIRONMENT=Development` automatically, which enables user secrets and auto-migration.

### Run in the background (terminal stays free)

```bash
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="http://localhost:5098" \
  dotnet run --project src/AIJobHelper.API --no-launch-profile &
```

### Verify the server started

```bash
curl http://localhost:5098/health
# Healthy
```

---

## Stopping the Application

If running in the foreground: press `Ctrl + C`.

If running in the background:
```bash
# Find the process
lsof -ti :5098

# Kill it
lsof -ti :5098 | xargs kill -9
```

---

## Application URLs

Once the server is running, open these in your browser:

| URL | Description |
|---|---|
| `http://localhost:5098` | Dashboard — overview and quick links |
| `http://localhost:5098/Resumes` | Resume library — upload, view, delete |
| `http://localhost:5098/Resumes/Detail?id={id}` | Resume detail — view parsed text and run AI analysis |
| `http://localhost:5098/Ats` | ATS Scorer — score a resume against a job description |
| `http://localhost:5098/JobDescriptions` | Job Description manager — view and delete saved JDs |
| `http://localhost:5098/CoverLetters/Generate` | Cover Letter Generator — configure, generate, edit, download PDF |
| `http://localhost:5098/swagger` | Swagger UI — interactive API documentation |
| `http://localhost:5098/health` | Liveness health check |
| `http://localhost:5098/health/ready` | Readiness health check (includes DB connectivity) |

---

## API Endpoints

All endpoints are documented and testable via Swagger UI at `/swagger`.

### Resumes
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/resumes/upload` | Upload PDF or DOCX resume |
| GET | `/api/resumes` | List all uploaded resumes |
| GET | `/api/resumes/{id}` | Get resume by ID |
| POST | `/api/resumes/{id}/analyze` | Run AI analysis (triggers Gemini) |
| GET | `/api/resumes/{id}/analysis` | Get stored AI analysis |
| DELETE | `/api/resumes/{id}` | Delete resume and its analysis |

### Job Descriptions
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/job-descriptions` | Save a JD (plain text or URL) |
| GET | `/api/job-descriptions` | List all saved job descriptions |
| GET | `/api/job-descriptions/{id}` | Get job description by ID |
| DELETE | `/api/job-descriptions/{id}` | Delete JD and its ATS results |

### ATS Scorer
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/ats/score` | Score a resume against a job description |
| GET | `/api/ats/{id}` | Get a stored ATS result |
| GET | `/api/ats` | List results (filter by `resumeId` or `jobDescriptionId`) |

### Cover Letter Configs
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/cover-letter-configs` | Create a new config |
| GET | `/api/cover-letter-configs` | List all configs |
| GET | `/api/cover-letter-configs/{id}` | Get config by ID |
| PUT | `/api/cover-letter-configs/{id}` | Update a config |
| DELETE | `/api/cover-letter-configs/{id}` | Delete a config |

### Cover Letters
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/cover-letters/generate` | Generate a cover letter (triggers Gemini) |
| GET | `/api/cover-letters` | List cover letters (filter by `resumeId`) |
| GET | `/api/cover-letters/{id}` | Get a cover letter by ID |
| GET | `/api/cover-letters/{id}/pdf` | Download the original generated PDF |
| POST | `/api/cover-letters/render-pdf` | Render a PDF from custom header/body/footer text |

---

## Supported File Formats

| Format | Extension | Notes |
|---|---|---|
| PDF | `.pdf` | Parsed with PdfPig |
| Word | `.docx` | Parsed with DocumentFormat.OpenXml |

Maximum file size: **10 MB**.
`.doc` (old Word format) is not supported — convert to `.docx` first.

---

## Logging

Logs are written to two places simultaneously:

**Console** — structured output visible while the server runs.

**File** — rolling daily log file at:
```
src/AIJobHelper.API/bin/Debug/net10.0/logs/app-YYYYMMDD.log
```

Log levels configured in `appsettings.json`:
```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

To see verbose EF Core SQL queries, change `Microsoft.EntityFrameworkCore` to `Debug`.

---

## Environment Variables

These override `appsettings.json` values and are required for Docker or cloud deployment:

| Variable | Description | Example |
|---|---|---|
| `ConnectionStrings__Default` | Full MySQL connection string | `Server=db;Port=3306;Database=ai_job_helper;User=root;Password=secret;` |
| `Gemini__ApiKey` | Google Gemini API key | `AIzaSy...` |
| `Gemini__Model` | Gemini model name | `gemini-2.5-flash` |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` | `Development` loads user secrets and auto-migrates |
| `ASPNETCORE_URLS` | Override listen address | `http://localhost:5098` |

---

## Docker Setup

### Run with Docker Compose (API + MySQL together)

```bash
# 1. Copy the example env file and fill in your values
cp .env.example .env
# Edit .env:
#   MYSQL_ROOT_PASSWORD=your_password
#   GEMINI_API_KEY=your_gemini_key

# 2. Build and start
docker compose up --build

# 3. Open the app
# http://localhost:8080
# http://localhost:8080/swagger
```

Stop:
```bash
docker compose down
```

Stop and remove data volumes (full reset):
```bash
docker compose down -v
```

---

## Project Structure

```
AI-Job-Helper/
├── src/
│   ├── AIJobHelper.API/
│   │   ├── Controllers/          # REST API controllers
│   │   ├── Pages/                # Razor Pages UI
│   │   │   ├── Resumes/
│   │   │   ├── Ats/
│   │   │   ├── JobDescriptions/
│   │   │   ├── CoverLetters/
│   │   │   └── Shared/_Layout.cshtml
│   │   ├── wwwroot/css/          # Custom CSS
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── AIJobHelper.Application/
│   │   ├── Interfaces/           # Service and infrastructure contracts
│   │   ├── Services/             # Business logic
│   │   └── DTOs/                 # Request and response models
│   ├── AIJobHelper.Domain/
│   │   └── Entities/             # Database entity classes
│   └── AIJobHelper.Infrastructure/
│       ├── Persistence/          # EF Core DbContext and migrations
│       ├── AI/                   # GeminiClient (HttpClient wrapper)
│       ├── Documents/            # PDF and DOCX parsers
│       ├── Pdf/                  # QuestPDF cover letter generator
│       ├── Web/                  # URL content fetcher
│       └── DependencyInjection.cs
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

---

## Common Issues

**Port already in use**
```bash
lsof -ti :5098 | xargs kill -9
```

**Gemini returns 403 Forbidden**
The server must run with `ASPNETCORE_ENVIRONMENT=Development` so user secrets are loaded.
If using `--no-launch-profile`, set the variable explicitly:
```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/AIJobHelper.API --no-launch-profile
```

**EF migrations fail on startup**
Run migrations manually (see [Database Migrations](#database-migrations)) and confirm MySQL is running and the connection string is correct.

**PDF or DOCX parsing returns empty text**
Ensure the file is not password-protected or image-only. Scanned PDFs (no embedded text) cannot be parsed.