# AI Job Helper

An AI-powered job application assistant — resume analysis, ATS scoring, and cover letter generation.

Built with **.NET 10**, **Google Gemini 2.5 Flash**, and **MySQL 8**.

---

## Features

| Phase | Feature |
|---|---|
| 1 | **Resume Analysis** — upload PDF/DOCX, get AI summary, strengths, weaknesses, and job title suggestions |
| 2 | **ATS Scorer** — compare resume to a job description (text, URL, or saved), get a score + skills gap |
| 3 | **Cover Letter Generator** — generate, preview, edit, and download a tailored PDF cover letter |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 — ASP.NET Core |
| UI | Razor Pages (Bootstrap 5) |
| AI | Google Gemini 2.5 Flash |
| Database | MySQL 8+ via Pomelo EF Core 9 |
| PDF generation | QuestPDF |
| Resume parsing | PdfPig (PDF) · DocumentFormat.OpenXml (DOCX) |
| Logging | Serilog — Console + rolling daily file |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- MySQL 8+ — macOS: `brew install mysql && brew services start mysql` · Windows: [MySQL Installer](https://dev.mysql.com/downloads/installer/)
- [Google Gemini API key](https://aistudio.google.com/app/apikey)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## ⚡ Setup & Commands

> Run all commands from the repository root `AI-Job-Helper/` unless noted.

### 1 — Set the database connection string

Edit `src/AIJobHelper.API/appsettings.json`:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Port=3306;Database=ai_job_helper;User=root;Password=YOUR_PASSWORD;"
}
```

Create the database if it doesn't exist yet:

```bash
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS ai_job_helper;"
```

---

### 2 — Store your Gemini API key

```bash
cd src/AIJobHelper.API
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
```

Verify:

```bash
dotnet user-secrets list
# Gemini:ApiKey = AIza...
```

To remove the key:

```bash
dotnet user-secrets remove "Gemini:ApiKey"
```

---

### 3 — Build

```bash
dotnet build
```

---

### 4 — Run the server

```bash
dotnet run --project src/AIJobHelper.API
```

App: **http://localhost:5098** · Swagger: **http://localhost:5098/swagger**

---

### 5 — Stop the server

Foreground (terminal where server is running):
```
Ctrl + C
```

Background process:
```bash
pkill -f "AIJobHelper.API"
```

---

### 6 — Run migrations manually

Migrations apply automatically on startup in Development. To run manually:

```bash
dotnet ef database update \
  --project src/AIJobHelper.Infrastructure \
  --startup-project src/AIJobHelper.API
```

---

## Application URLs

| URL | Page |
|---|---|
| `http://localhost:5098` | Dashboard |
| `http://localhost:5098/Resumes` | Resume library |
| `http://localhost:5098/Ats` | ATS Scorer |
| `http://localhost:5098/JobDescriptions` | Job Descriptions |
| `http://localhost:5098/CoverLetters/Generate` | Cover Letter Generator |
| `http://localhost:5098/swagger` | Swagger UI |
| `http://localhost:5098/health` | Health check |

---

## API Endpoints

### Resumes
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/resumes/upload` | Upload PDF or DOCX |
| GET | `/api/resumes` | List all resumes |
| GET | `/api/resumes/{id}` | Get resume by ID |
| POST | `/api/resumes/{id}/analyze` | Run AI analysis |
| GET | `/api/resumes/{id}/analysis` | Get stored analysis |
| DELETE | `/api/resumes/{id}` | Delete resume + analysis |

### Job Descriptions
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/job-descriptions` | Save a JD (text or URL) |
| GET | `/api/job-descriptions` | List all JDs |
| DELETE | `/api/job-descriptions/{id}` | Delete JD + ATS results |

### ATS Scorer
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/ats/score` | Score resume vs JD |
| GET | `/api/ats/{id}` | Get ATS result |
| GET | `/api/ats` | List results |

### Cover Letters
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/cover-letter-configs` | Create config |
| GET | `/api/cover-letter-configs` | List configs |
| PUT | `/api/cover-letter-configs/{id}` | Update config |
| DELETE | `/api/cover-letter-configs/{id}` | Delete config |
| POST | `/api/cover-letters/generate` | Generate cover letter |
| GET | `/api/cover-letters/{id}` | Get cover letter |
| POST | `/api/cover-letters/render-pdf` | Render edited preview as PDF |

---

## Docker Setup

```bash
cp .env.example .env
# Edit .env — set MYSQL_ROOT_PASSWORD and GEMINI_API_KEY

docker compose up --build
# App: http://localhost:8080

docker compose down        # stop
docker compose down -v     # stop + wipe data
```

---

## Project Structure

```
AI-Job-Helper/
├── src/
│   ├── AIJobHelper.API/           # Controllers, Razor Pages, Program.cs
│   ├── AIJobHelper.Application/   # Services, interfaces, DTOs
│   ├── AIJobHelper.Domain/        # Entities
│   └── AIJobHelper.Infrastructure/# EF Core, Gemini client, parsers, PDF
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

---

## Environment Variables (Docker / cloud)

| Variable | Description |
|---|---|
| `ConnectionStrings__Default` | MySQL connection string |
| `Gemini__ApiKey` | Gemini API key |
| `Gemini__Model` | Model name (default: `gemini-2.5-flash`) |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
