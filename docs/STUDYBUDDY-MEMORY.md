# STUDYBUDDY — Project Memory & Architecture Plan
> This file is the context anchor for all future sessions on this project.
> Start every new chat by sharing this file so no context is lost.
> Last updated: 26 July 2026

---

## 1. WHAT THIS PROJECT IS

**StudyBuddy** — An AI-powered personal tutor application that turns any text content into an interactive study session with Claude as the tutor and ElevenLabs as the voice.

**The core idea:** Paste or load any text — from any source, any platform, any topic — and interact with it in three distinct modes: Explain, Quiz, and Summarise. Claude generates the responses. ElevenLabs reads them back with natural intonation and emphasis.

**The application is source-agnostic. It accepts text from anywhere:**
- Online course platforms (Skilljar, Coursera, Udemy, LinkedIn Learning)
- Websites and articles (Medium, documentation pages, blog posts)
- YouTube video transcripts (copy-paste the transcript)
- Downloaded PDFs (copy the text out)
- Your own typed or handwritten notes
- Complex technical concepts from anywhere on the internet
- Any other text a user wants to learn from

**The app does not care where the text came from. It only works with what you paste or upload into it.**

**Why this project exists (dual purpose):**
1. To make studying any topic faster — explain, quiz, and summarise any content on demand
2. To learn and experience Semantic Kernel, SK Plugins, SK Planner, and SK Telemetry in a familiar stack — the .NET equivalent of LangChain, LangGraph, and LangSmith

---

## 2. THE THREE MODES

### Mode 1 — Explain ✅ (backend live)
- User pastes a concept, topic, or question from the course
- Claude reads the loaded study material and explains it in plain tutor language
- Explanation appears as text on screen
- ElevenLabs reads the explanation aloud with natural intonation and emphasis *(not built yet)*
- Goal: Make Claude explain things the way a real tutor would, not like documentation

### Mode 2 — Quiz (not started)
- User requests a quiz on a topic or section
- Claude generates questions from the loaded study material
- User types their answers
- Claude evaluates each answer and explains what was right or wrong
- ElevenLabs reads the feedback aloud
- Goal: Active recall — the fastest way to retain information

### Mode 3 — Summarise (not started)
- User pastes a full section of study material
- Claude condenses it into key points and takeaways
- Summary appears as text on screen
- ElevenLabs reads the summary aloud
- Goal: Digest large sections quickly without losing the important details

---

## 3. TECH STACK (DECIDED)

| Layer | Technology | Purpose | Status |
|---|---|---|---|
| Backend | .NET 10 / C# | API, orchestration | ✅ Scaffolded |
| Architecture | Clean Architecture (4 projects) | SOLID layer separation | ✅ In place |
| Frontend | React 19 + TypeScript + Vite | UI | ⏳ Not started |
| AI Orchestration | Microsoft.SemanticKernel 1.78 | Plugins, prompts, future Planner | ✅ Wired |
| AI Connector | Microsoft.SemanticKernel.Connectors.OpenAI | OpenAI-compatible client → OpenRouter | ✅ Wired |
| AI Model | Claude Haiku via OpenRouter (`anthropic/claude-haiku-4-5`) | Explanations (and later quizzes/summaries) | ✅ Working path |
| Text-to-Speech | ElevenLabs API (free tier) | Reads Claude's responses aloud | ⏳ Stub options + HttpClient only |
| Monitoring | SK Telemetry + OpenTelemetry Console exporter | Logs every Claude call in dev | ✅ Wired |
| Database | PostgreSQL via EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL`) | Study material + session history | ⏳ DbContext scaffolded; not actively used yet |
| Secrets | Env vars + `appsettings.Development.json` (gitignored) | Local keys never committed | ✅ Configured |
| Containerisation | Docker Compose | Later | ⏳ Not started |

**Important stack corrections vs early draft:**
- **Claude access = OpenRouter**, not Anthropic SDK direct (for now). SK uses the OpenAI connector pointed at OpenRouter’s OpenAI-compatible endpoint.
- **Database = PostgreSQL**, not SQLite (decision made when scaffolding Infrastructure).
- Solution file format under .NET 10 is **`StudyBuddy.slnx`** (not classic `.sln`).

---

## 4. HOW SEMANTIC KERNEL MAPS TO THE LANG FAMILY

This is the conceptual bridge — same ideas, different names:

| Lang Family (Python) | Semantic Kernel (.NET) | What it does |
|---|---|---|
| LangChain Chain | SK Plugin | A named, reusable AI task |
| LangGraph | SK Planner | Routes between tasks based on intent |
| Prompt Template | Prompt Template (same name) | Reusable prompt with variable slots |
| LangSmith | SK Telemetry | Logs every AI call automatically |
| Tool | SK Function | A capability the AI can call |

**Key insight:** The concepts are identical. Only the syntax and language differ. Understanding SK deeply means you can speak to LangChain concepts in interviews — the architectural thinking transfers directly.

---

## 5. THE SK ARCHITECTURE FOR THIS APP

### Target flow (full product vision):

```
User types a message (explain / quiz / summarise)
↓
React sends it to .NET API
↓
.NET API hands it to SK Planner
↓
SK Planner reads the intent of the message
↓
Routes to the correct SK Plugin:
  → "explain this" → ExplainPlugin
  → "quiz me"      → QuizPlugin
  → "summarise"    → SummarisePlugin
↓
Plugin builds the prompt template
Injects the study material + user message
Calls Claude via OpenRouter (SK OpenAI connector)
↓
SK Telemetry logs the full call automatically
(exact prompt sent, exact response, tokens, etc.)
↓
Claude's text response returns to .NET API
↓
.NET API sends text to ElevenLabs TTS API
↓
ElevenLabs returns audio
↓
React displays text + plays audio simultaneously
```

### What is implemented today (Phase 1 backend):

```
POST /api/study/explain  { userMessage, studyMaterial }
↓
StudyController
↓
IExplainService / ExplainService
↓
Kernel.InvokeAsync("ExplainPlugin", "Explain", ...)
↓
ExplainPlugin.ExplainAsync → InvokePromptAsync(ExplainPromptTemplate)
↓
OpenRouter → anthropic/claude-haiku-4-5
↓
SK OpenTelemetry traces/metrics printed to console
↓
JSON { explanation: "..." }
```

**Not yet in the path:** SK Planner, QuizPlugin, SummarisePlugin, ElevenLabs TTS, React UI.

### The three SK Plugins:

**ExplainPlugin** ✅
- Location: `backend/StudyBuddy.Application/Plugins/ExplainPlugin.cs`
- Prompt: `backend/StudyBuddy.Application/Prompts/ExplainPromptTemplate.cs`
- Takes: `userMessage` + `studyMaterial`
- Instruction: plain, conversational tutor language — not documentation style
- Decorated with `[KernelFunction("Explain")]` and `[Description(...)]`
- Returns: explanation text

**QuizPlugin** ⏳
- Takes: topic or section + loaded study material
- Prompt instruction: "Generate 3 questions on this topic. After the user answers, evaluate each answer and explain what was right or wrong."
- Returns: questions first, then evaluation after answers

**SummarisePlugin** ⏳
- Takes: pasted section of study material
- Prompt instruction: "Summarise this into the 5 most important points a student needs to remember. Be concise but complete."
- Returns: bulleted key points

---

## 6. CLEAN ARCHITECTURE — HOW THE BACKEND IS ORGANISED

**Repo root:** `/Users/roysondsouza/AI Projects/studybuddy`  
**Backend solution:** `backend/StudyBuddy.slnx`

```
backend/
├── StudyBuddy.slnx
├── .env.example                          # committed template (empty values)
├── StudyBuddy.API/                       # Entry point
│   ├── Program.cs                        # SK + OpenRouter + Telemetry DI
│   ├── Controllers/StudyController.cs    # POST /api/study/explain
│   ├── appsettings.json                  # safe defaults (no secrets)
│   └── appsettings.Development.json      # LOCAL ONLY — gitignored
├── StudyBuddy.Application/               # Use cases / SK surface
│   ├── Plugins/ExplainPlugin.cs
│   ├── Prompts/ExplainPromptTemplate.cs
│   ├── Interfaces/IExplainService.cs
│   ├── Services/ExplainService.cs
│   └── Models/ExplainRequest.cs, ExplainResponse.cs
├── StudyBuddy.Infrastructure/            # External I/O
│   ├── DependencyInjection/              # AddInfrastructure()
│   ├── Persistence/StudyBuddyDbContext.cs
│   └── ExternalServices/
│       ├── OpenRouterOptions.cs
│       └── ElevenLabsOptions.cs          # stub for later TTS
└── StudyBuddy.Domain/                    # Enterprise models
    ├── Entities/StudyMaterial.cs
    └── Models/ExplainResult.cs
```

### Layer rules (do not violate in future work)

| Project | May depend on | Owns |
|---|---|---|
| **Domain** | Nothing | Entities, pure models |
| **Application** | Domain | SK plugins, prompt templates, service interfaces + implementations that orchestrate Kernel |
| **Infrastructure** | Application, Domain | EF Core/PostgreSQL, HttpClient registrations, options for external APIs |
| **API** | Application, Infrastructure, Domain | Controllers, `Program.cs` composition root, host config |

**SOLID notes already applied:**
- **S** — `ExplainPlugin` only explains; `ExplainService` only invokes Kernel; controller only handles HTTP
- **O** — new modes = new plugins/services, not bloating Explain
- **L/I** — `IExplainService` is the narrow contract the API depends on
- **D** — API depends on `IExplainService`, not concrete Kernel details in the controller

---

## 7. ARCHITECTURAL DECISIONS LOG (REFER BACK HERE)

### AD-001 — Clean Architecture from day one
**Decision:** Four projects under `backend/` instead of a single Web API.  
**Why:** Matches NOSYOR.M.I learning goals, keeps SK plugins testable, and prevents controllers from owning AI logic.  
**Implication:** New features land in Application (plugins/services) or Infrastructure (clients/DB), not in controllers.

### AD-002 — Claude via OpenRouter + SK OpenAI connector
**Decision:** Use OpenRouter’s OpenAI-compatible API through `AddOpenAIChatCompletion` with a custom endpoint.  
**Config:**
- Base URL: `https://openrouter.ai/api/v1`
- Model: `anthropic/claude-haiku-4-5`
- API key: env var `OPENROUTER_API_KEY` (preferred), fallback `OpenRouter:ApiKey` in Development settings  
**Why:** One connector pattern; easy model switching; no Anthropic-specific SDK required for Phase 1.  
**Note:** Custom endpoints are experimental in SK → `#pragma warning disable SKEXP0010` in `Program.cs`.  
**Status:** Open question “Anthropic SDK vs OpenRouter” is **resolved for Phase 1 = OpenRouter**. Revisit only if we need Anthropic-specific features.

### AD-003 — ExplainPlugin first; Planner later
**Decision:** Ship Explain end-to-end before Quiz/Summarise/Planner.  
**Why:** Prove prompt → Claude → telemetry → API response before adding routing complexity.  
**Current API:** `POST /api/study/explain` body `{ "userMessage": "...", "studyMaterial": "..." }` → `{ "explanation": "..." }`

### AD-004 — Prompt templates live in Application
**Decision:** `ExplainPromptTemplate` is a static template string with `{{$userMessage}}` / `{{$studyMaterial}}`, invoked via `kernel.InvokePromptAsync`.  
**Why:** Keeps prompt wording versioned with the plugin, separate from HTTP and infrastructure.

### AD-005 — SK Telemetry to console in Development
**Decision:** OpenTelemetry tracing + metrics for `Microsoft.SemanticKernel*`, console exporters, and:
```csharp
AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
```
**Why:** LangSmith-equivalent visibility while learning — every Claude call should be inspectable in the terminal.  
**Packages:** `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Exporter.Console`

### AD-006 — PostgreSQL via EF Core (scaffold now, use later)
**Decision:** `StudyBuddyDbContext` + `Npgsql.EntityFrameworkCore.PostgreSQL`; register DbContext only when `ConnectionStrings:DefaultConnection` is non-empty.  
**Why:** Aligns with production-shaped persistence early; Phase 1 Explain path does **not** require DB yet (material is passed in the request body).  
**Earlier draft said SQLite** — that was superseded by this decision.

### AD-007 — ElevenLabs deferred; Infrastructure stubs only
**Decision:** `ElevenLabsOptions` + named `HttpClient("ElevenLabs")` registered; no TTS client/service/endpoint yet.  
**Why:** Phase 1 focus = text in → Claude out → telemetry visible.

### AD-008 — Secrets never committed
**Decision:**
- `appsettings.Development.json` is gitignored (`**/appsettings.Development.json`)
- `backend/.env.example` is committed with empty values as a reference
- Real keys go in env vars or the local Development settings file  
**Required env vars (see `.env.example`):**
- `OPENROUTER_API_KEY`
- `ELEVENLABS_API_KEY` (future)
- `ELEVENLABS_VOICE_ID` (future)
- `ConnectionStrings__DefaultConnection` (when DB is used)

### AD-009 — Frontend not started in Phase 1
**Decision:** No React work until Explain API is proven manually (curl / `.http` file).  
**Test helper:** `backend/StudyBuddy.API/StudyBuddy.API.http`  
**Local URL:** `http://localhost:5017` (from `launchSettings.json`)

---

## 8. THE TTS LAYER — ELEVENLABS (PLANNED)

- ElevenLabs has a free tier — sufficient for personal study use
- Every text response from Claude will be passed to ElevenLabs after generation
- ElevenLabs returns an audio file or stream
- React plays the audio while the text is visible on screen simultaneously
- User reads along while hearing it — mirrors real tutor experience

**Why ElevenLabs specifically:**
- Best-in-class intonation and natural emphasis
- Simple REST API — easy to integrate into .NET
- Free tier available — no cost to start

**Important to know:**
- Claude cannot generate audio — text only
- Anthropic does not have a TTS API
- ElevenLabs is a separate service called after Claude responds

**Current code state:** options + HttpClient stub only in Infrastructure. No audio endpoint yet.

---

## 9. HOW STUDY MATERIAL GETS INTO THE APP

The app is source-agnostic — it never connects to any external platform directly. All content arrives as plain text through one of these methods:

**Method 1 — Copy-paste (start here):** User copies any text from any source and pastes it into the study material input. **Today the API accepts it as `studyMaterial` on each explain request.**
**Method 2 — File upload (Phase 4 enhancement):** `.txt` / `.md` upload.
**Method 3 — URL fetch (future consideration):** Public URL fetch only.

**What the app will never do:** Log into any platform on the user's behalf, scrape authenticated pages, or try to access content behind a paywall.

---

## 10. WHAT THIS PROJECT TEACHES (LEARNING GOALS)

By building this app, Royson will directly experience:

1. **SK Plugin definition** — ✅ started with ExplainPlugin
2. **SK Planner routing** — ⏳ next major SK learning milestone after Quiz/Summarise plugins
3. **Prompt Templates** — ✅ ExplainPromptTemplate in place
4. **SK Telemetry** — ✅ console OTel wired for Claude calls
5. **Multi-service orchestration** — ⏳ Claude done; ElevenLabs next
6. **Claude API behaviour** — ✅ via OpenRouter + Haiku 4.5

---

## 11. BUILD SEQUENCE & CURRENT PROGRESS

**Phase 1 — Core**
- [x] .NET API scaffold (Clean Architecture)
- [x] SK setup with ExplainPlugin only
- [x] Claude via OpenRouter (SK OpenAI connector)
- [x] SK Telemetry logging to console
- [x] Verify path: message in → Claude explanation out *(manual with API key)*
- [ ] React frontend with simple text input
- [ ] End-to-end UI verification

**Phase 2 — All three modes**
- [ ] QuizPlugin
- [ ] SummarisePlugin
- [ ] SK Planner to route by intent
- [ ] Verify planner picks the correct mode

**Phase 3 — TTS**
- [ ] Real ElevenLabs client (replace stub)
- [ ] Pass every Claude response to TTS
- [ ] React plays audio with text

**Phase 4 — Polish**
- [ ] Persistent study material loading / file upload
- [ ] Session history in PostgreSQL
- [ ] UI polish

---

## 12. HOW TO RUN WHAT EXISTS TODAY

```bash
export OPENROUTER_API_KEY="your-key"
cd backend/StudyBuddy.API
dotnet run
```

```bash
curl -X POST http://localhost:5017/api/study/explain \
  -H "Content-Type: application/json" \
  -d '{"userMessage":"What is dependency injection?","studyMaterial":"Dependency injection means..."}'
```

Expect JSON `{ "explanation": "..." }` and SK telemetry spans in the console.

**NuGet packages in play:**
- `Microsoft.SemanticKernel`
- `Microsoft.SemanticKernel.Connectors.OpenAI`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.Extensions.Http` (Infrastructure — for future ElevenLabs)
- `OpenTelemetry.Extensions.Hosting` + `OpenTelemetry.Exporter.Console`

---

## 13. PROJECT IDENTITY

**Name:** StudyBuddy  
**Purpose:** Personal AI tutor for Claude certification study  
**Stack:** .NET 10 + (planned) React 19 + Semantic Kernel + Claude via OpenRouter + ElevenLabs  
**Repo:** Local git repo at `studybuddy` (separate from NOSYOR.M.I)  
**Design:** Keep it simple and functional — learning tool first  
**Standing coding rule:** Follow SOLID principles and clean coding structure throughout

---

## 14. OPEN QUESTIONS (STILL TO DECIDE)

- [x] ~~Anthropic SDK direct vs OpenRouter~~ → **OpenRouter for Phase 1** (AD-002)
- [x] ~~SQLite vs PostgreSQL~~ → **PostgreSQL via EF Core** (AD-006)
- [ ] ElevenLabs voice selection — which voice fits a tutor persona?
- [ ] PostgreSQL schema beyond `StudyMaterial` — what else to persist (sessions, quiz history)?
- [ ] When to introduce SK Planner — after both Quiz + Summarise plugins exist, or earlier?
- [ ] GitHub remote / project board setup
- [ ] Whether to switch from Haiku to a larger Claude model for Quiz evaluation quality

---

## 15. KEY CONTEXT — WHO IS BUILDING THIS

- **Builder:** Royson D'Souza
- **Background:** MBA graduate, no prior coding background, learning through building
- **Existing project:** NOSYOR.M.I — full-stack AI personal finance app (.NET 10 + React 19 + pgvector + Docker)
- **Tools:** Cursor IDE (Agent mode), Claude Code extension in Cursor, GitHub Desktop
- **Working style:** Architecture first, then Cursor builds the code
- **Standing rule:** Every Cursor prompt must include "Follow SOLID principles and clean coding structure throughout"
- **AI in chat vs AI in Cursor:** claude.ai handles architecture and learning decisions. Claude Code in Cursor handles file edits and terminal commands.

---

## 16. HOW TO START THE NEXT SESSION

Paste this file into a new chat with the message:

*"Let's continue building StudyBuddy. Here is the project memory file with everything we decided. Phase 1 backend ExplainPlugin is done — pick up from the next unchecked item."*

### Sensible next steps (pick one):
1. Manually verify Explain with a real `OPENROUTER_API_KEY` and confirm telemetry in console
2. Scaffold React frontend that calls `POST /api/study/explain`
3. Add QuizPlugin (same Application pattern as Explain)
4. Implement ElevenLabs TTS after text responses

Claude will read this file and pick up with no context loss.
