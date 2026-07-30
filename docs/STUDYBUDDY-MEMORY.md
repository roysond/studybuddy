# STUDYBUDDY — Project Memory & Architecture Plan
> This file is the context anchor for all future sessions on this project.
> Start every new chat by sharing this file so no context is lost.
> Last updated: 30 July 2026

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

### Mode 1 — Explain ✅ (live end-to-end)
- User pastes a concept, topic, or question from the course
- Claude reads the loaded study material and explains it in plain tutor language
- Explanation appears as text on screen
- ElevenLabs reads the explanation aloud with natural intonation and emphasis *(not built yet)*
- Goal: Make Claude explain things the way a real tutor would, not like documentation

### Mode 2 — Quiz ✅ (live end-to-end)
- User requests a quiz on a topic or section
- Claude generates 3 questions from the loaded study material
- User types their answers
- Claude evaluates each answer and explains what was right or wrong
- ElevenLabs reads the feedback aloud *(not built yet)*
- Goal: Active recall — the fastest way to retain information

### Mode 3 — Summarise ✅ (live end-to-end)
- User pastes a full section of study material
- Claude condenses it into the 5 most important key points
- Summary appears as text on screen
- ElevenLabs reads the summary aloud *(not built yet)*
- Goal: Digest large sections quickly without losing the important details

---

## 3. TECH STACK (DECIDED)

| Layer | Technology | Purpose | Status |
|---|---|---|---|
| Backend | .NET 10 / C# | API, orchestration | ✅ Scaffolded |
| Architecture | Clean Architecture (4 projects) | SOLID layer separation | ✅ In place |
| Frontend | React 19 + TypeScript + Vite | UI | ✅ Built — 3 mode panels + shared study material input, dev port 5180 (AD-020) |
| AI Orchestration | Microsoft.SemanticKernel 1.78 | Plugins, prompts, future Planner | ✅ Wired |
| AI Connector | Microsoft.SemanticKernel.Connectors.OpenAI | OpenAI-compatible client → OpenRouter | ✅ Wired |
| AI Model | Claude Haiku via OpenRouter (`anthropic/claude-haiku-4-5`) | Explanations (and later quizzes/summaries) | ✅ Working path |
| Text-to-Speech | Browser Web Speech API (`window.speechSynthesis`) | Reads Claude's responses aloud, client-side only | ✅ Built — voice picker + speed control. ElevenLabs was built then fully removed (AD-021, AD-024) |
| Monitoring | SK Telemetry + OpenTelemetry Console exporter | Logs every Claude call in dev | ✅ Wired |
| Dev observability | `IFunctionInvocationFilter` → in-memory store → `/dev` dashboard | Live token/latency/cost view, eval and tutoring traffic separated | ✅ Built (AD-026), defects fixed (AD-027) |
| Evaluation | `Microsoft.Extensions.AI.Evaluation` + `.Quality` + `.Reporting` 10.8.0 | LLM-as-judge scoring per mode with per-case reasoning; disk reports + history | ✅ Built and verified (AD-026, AD-029) |
| Eval reporting CLI | `Microsoft.Extensions.AI.Evaluation.Console` (global tool, command is `aieval`) | Interactive HTML drill-down report from `eval-reports/` | ✅ Installed and working (AD-029) |
| Database | PostgreSQL via EF Core (`Npgsql.EntityFrameworkCore.PostgreSQL`) | Study material + session history | ⏳ DbContext scaffolded; not actively used yet |
| Secrets | Env vars + `appsettings.Development.json` (gitignored) | Local keys never committed | ✅ Configured |
| Local dev startup | `start.sh` + `studybuddy` shell alias | One command starts backend + frontend | ✅ Built (AD-023) |
| Containerisation | Docker Compose | Deferred — see AD-023 | ⏳ Not started |

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
| LangSmith (tracing half) | SK Telemetry | Logs every AI call automatically — what happened |
| LangSmith (evaluations half) | `Microsoft.Extensions.AI.Evaluation` | Scores whether the output was actually good — Groundedness, Relevance, Completeness, Fluency, Coherence, Truthfulness. Separate library from SK Telemetry; sibling to the Microsoft.Extensions.AI packages SK already sits on. |
| Tool | SK Function | A capability the AI can call |

**Important distinction:** LangSmith is not one thing — it's tracing (observability: what happened) plus evaluation (quality: was it good). The original mapping in this file equated LangSmith with SK Telemetry, which only covers the tracing half. There is no SK-native equivalent for evals; `Microsoft.Extensions.AI.Evaluation` fills that gap.

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

### What is implemented today (Explain):

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

**Not yet in the path:** SK Planner, ElevenLabs TTS, React UI.

### What is implemented today (Quiz):

```
POST /api/study/quiz/questions  { topic, studyMaterial }
↓
StudyController
↓
IQuizService / QuizService
↓
Kernel.InvokeAsync("QuizPlugin", "GenerateQuestions", ...)
↓
QuizPlugin.GenerateQuestionsAsync → InvokePromptAsync(QuizPromptTemplates.QuestionsTemplate)
↓
OpenRouter → anthropic/claude-haiku-4-5
↓
JSON { questions: "..." }

POST /api/study/quiz/evaluate  { questions, studentAnswers, studyMaterial }
↓
StudyController
↓
IQuizService / QuizService
↓
Kernel.InvokeAsync("QuizPlugin", "EvaluateAnswers", ...)
↓
QuizPlugin.EvaluateAnswersAsync → InvokePromptAsync(QuizPromptTemplates.EvaluationTemplate)
↓
OpenRouter → anthropic/claude-haiku-4-5
↓
JSON { evaluation: "..." }
```

### The three SK Plugins:

**ExplainPlugin** ✅
- Location: `backend/StudyBuddy.Application/Plugins/ExplainPlugin.cs`
- Prompt: `backend/StudyBuddy.Application/Prompts/ExplainPromptTemplate.cs`
- Takes: `userMessage` + `studyMaterial`
- Instruction: plain, conversational tutor language — not documentation style
- Decorated with `[KernelFunction("Explain")]` and `[Description(...)]`
- Returns: explanation text

**QuizPlugin** ✅
- Location: `backend/StudyBuddy.Application/Plugins/QuizPlugin.cs`
- Prompt: `backend/StudyBuddy.Application/Prompts/QuizPromptTemplates.cs` (two templates: `QuestionsTemplate`, `EvaluationTemplate`)
- Two `[KernelFunction]` entries on one plugin: `GenerateQuestions` (topic + studyMaterial → 3 numbered questions) and `EvaluateAnswers` (questions + studentAnswers + studyMaterial → per-question right/wrong feedback)
- Service: `IQuizService` / `QuizService` — mirrors `IExplainService` / `ExplainService` exactly
- Endpoints: `POST /api/study/quiz/questions` and `POST /api/study/quiz/evaluate` on the same `StudyController`
- Registered on the Kernel via `kernelBuilder.Plugins.AddFromType<QuizPlugin>()`; `IQuizService` registered `AddScoped` in `Program.cs`
- Explain mode was left untouched during this build

**SummarisePlugin** ✅
- Location: `backend/StudyBuddy.Application/Plugins/SummarisePlugin.cs`
- Prompt: `backend/StudyBuddy.Application/Prompts/SummarisePromptTemplate.cs`
- Takes: `studyMaterial` only (single-step, same shape as Explain)
- Instruction: condense into exactly the 5 most important points, grounded strictly in the material, no invented facts
- Decorated with `[KernelFunction("Summarise")]` and `[Description(...)]`
- Service: `ISummariseService` / `SummariseService` — mirrors `IExplainService` / `ExplainService`
- Endpoint: `POST /api/study/summarise` on `StudyController`
- Registered on the Kernel via `kernelBuilder.Plugins.AddFromType<SummarisePlugin>()`; `ISummariseService` registered `AddScoped` in `Program.cs`
- Returns: 5 bulleted key points

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
│   ├── Controllers/StudyController.cs    # explain + quiz/questions + quiz/evaluate
│   ├── appsettings.json                  # safe defaults (no secrets)
│   └── appsettings.Development.json      # LOCAL ONLY — gitignored
├── StudyBuddy.Application/               # Use cases / SK surface
│   ├── Plugins/ExplainPlugin.cs, QuizPlugin.cs, SummarisePlugin.cs
│   ├── Prompts/ExplainPromptTemplate.cs, QuizPromptTemplates.cs, SummarisePromptTemplate.cs
│   ├── Interfaces/IExplainService.cs, IQuizService.cs, ISummariseService.cs
│   ├── Services/ExplainService.cs, QuizService.cs, SummariseService.cs
│   └── Models/ExplainRequest.cs, ExplainResponse.cs, QuizQuestionsRequest.cs,
│       QuizQuestionsResponse.cs, QuizEvaluationRequest.cs, QuizEvaluationResponse.cs,
│       SummariseRequest.cs, SummariseResponse.cs
├── StudyBuddy.Infrastructure/            # External I/O
│   ├── DependencyInjection/              # AddInfrastructure()
│   ├── Persistence/StudyBuddyDbContext.cs
│   └── ExternalServices/
│       ├── OpenRouterOptions.cs
│       └── ElevenLabsOptions.cs          # stub for later TTS
└── StudyBuddy.Domain/                    # Enterprise models
    ├── Entities/StudyMaterial.cs
    └── Models/ExplainResult.cs, QuizQuestionsResult.cs, QuizEvaluationResult.cs, SummariseResult.cs
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

### AD-009 — Frontend not started in Phase 1 *(superseded — frontend now built, see AD-020/AD-023)*
**Decision:** No React work until Explain API is proven manually (curl / `.http` file).  
**Test helper:** `backend/StudyBuddy.API/StudyBuddy.API.http`  
**Local URL:** `http://localhost:5017` (from `launchSettings.json`)

### AD-010 — Cowork as the primary architecture and decision hub
**Decision:** Claude Cowork (desktop app) is the primary environment for all architecture decisions, creative thinking, and prompt drafting for Cursor. Cursor handles actual code writing only.  
**Why:** Cowork has full access to the local repo folder (read/write), can run shell commands, and persists files. It is a superset of regular Claude.ai chat — everything a Project chat can do, plus local machine capabilities. All big decisions originate in Cowork, then Cursor acts on them.  
**Workflow:** Decide in Cowork → draft Cursor prompt → Cursor builds → come back to Cowork to verify and discuss next steps.

### AD-011 — Local folder mount replaces need for GitHub MCP connectivity
**Decision:** GitHub MCP connector is not required for AI context awareness. The local `studybuddy` repo folder is mounted in Cowork with read/write access.  
**Why:** The local folder is more up-to-date than GitHub (includes uncommitted changes). Whenever Cursor makes changes, they are immediately readable from the local folder — no commit or push needed. To update context, simply say "scan the repo" and Cowork reads the latest state instantly.  
**Implication:** GitHub Desktop is still used for version control and pushing to remote. GitHub MCP connectivity is a separate concern and not needed for this workflow.

### AD-012 — Memory file update workflow and studybuddy-markdown-update skill
**Decision:** Claude in Cowork owns the responsibility of updating `docs/STUDYBUDDY-MEMORY.md` after every meaningful session. A dedicated skill (`studybuddy-markdown-update`) was created to handle this.  
**Why:** All major decisions originate in Cowork, so it makes more sense for Cowork to update the memory file than Cursor. The skill reads the file, makes surgical updates, saves it, and always outputs a visible ✅ confirmation.  
**Two-copy sync rule:** The local file (`docs/STUDYBUDDY-MEMORY.md`) is the live source of truth — updated by the skill after each session. The cloud project knowledge copy in Claude.ai is the fallback — synced manually by Royson via copy-paste at the end of sessions or at major milestones.

### AD-013 — QuizPlugin: two-endpoint design (generate, then evaluate)
**Decision:** `QuizPlugin` exposes two `[KernelFunction]`s on one plugin — `GenerateQuestions` and `EvaluateAnswers` — invoked via two separate endpoints (`POST /api/study/quiz/questions`, `POST /api/study/quiz/evaluate`) rather than one combined endpoint.
**Why:** Matches the real UX flow (ask → student answers → evaluate) and keeps each SK function single-purpose (SRP), same as ExplainPlugin's one-function-per-responsibility shape.
**Pattern:** `QuizPlugin` / `IQuizService` / `QuizService` mirror `ExplainPlugin` / `IExplainService` / `ExplainService` file-for-file. Built by Cursor from a Cowork-drafted prompt per the AD-010 workflow; build succeeded, Explain mode untouched.

### AD-014 — Secrets access boundary discussed
**Decision:** No architecture change yet, but flagged as an open item: move `OPENROUTER_API_KEY` out of `appsettings.Development.json` and into either a shell-exported env var or `dotnet user-secrets`, so the real key never sits inside a file Cowork's connected folder can read.
**Why:** Cowork sessions run remotely on Anthropic's servers; any file read through the connected folder is processed server-side for that session, not just scanned locally. `appsettings.Development.json` is gitignored (safe from git) but not excluded from Cowork's file access. `dotnet user-secrets` stores keys outside the project directory entirely, which is a real boundary; an instruction to Claude not to open the file is a behavioral backstop only, not a technical one.
**Status:** Not yet implemented — Royson to decide whether to switch to `dotnet user-secrets` or a shell env var.

### AD-015 — Standing reminder: commit & push after significant sessions
**Decision:** After any session with a substantial code or architecture change — a new plugin/service/endpoint, a passing build, or a meaningful memory-file update — Claude Cowork reminds Royson to write a commit summary and push to GitHub. In practice Royson commits via either GitHub Desktop or Cursor's built-in commit-and-sync — both are fine, whichever is faster in the moment. Claude does not commit or push itself (no GitHub MCP write access is configured).
**Why:** Royson wants to keep the commit habit consistent and not lose track of what changed session to session.
**Trigger:** New or changed plugin, service, interface, endpoint, or other backend/frontend code; a `docs/STUDYBUDDY-MEMORY.md` update; anything Claude would already judge as "worth a commit."

### AD-016 — Two-tier verification standard: structural vs functional
**Decision:** After any new mode/plugin is built, Claude Cowork explicitly labels which kind of check it did:
- **Structural verification** (Claude can do this alone, no API key involved): confirms the expected files exist, code matches the established pattern, and DI/Kernel/controller wiring is correct. This is a code-review-level check, not proof the feature works at runtime.
- **Functional verification** (requires the real `OPENROUTER_API_KEY` and a running server): actually hitting the endpoint and confirming Claude returns a correct, good-quality response. Claude does not do this silently or alone — it either happens with Royson running the app and reviewing output together (as done for Explain), or Royson pastes curl output back into Cowork for review.
**Why:** Running the live server requires the real API key from `appsettings.Development.json`, which conflicts with the AD-014 secrets boundary. Structural checks alone are not sufficient to claim a feature "works" — functional verification with Royson is required before a mode is marked ✅ verified (not just ✅ built).
**Status:** QuizPlugin is ✅ built, structurally verified, and **functionally verified** on 27 July 2026 — Royson ran both curl calls from Section 12 with real study material and deliberately wrong answers. `quiz/questions` returned 3 well-grounded, varied-difficulty questions in tutor voice. `quiz/evaluate` correctly confirmed the right answer and correctly caught both wrong answers (one confused, one directly contradicting DI), with accurate corrections grounded in the study material.

### AD-017 — SummarisePlugin built and verified; all three modes now live
**Decision:** `SummarisePlugin` built following the exact ExplainPlugin/QuizPlugin pattern (single `[KernelFunction("Summarise")]`, `ISummariseService`/`SummariseService`, `POST /api/study/summarise`). Build succeeded; Explain and Quiz left untouched.
**Functional verification (27 July 2026):** Royson ran the summarise endpoint against a SOLID-principles passage with more than 5 candidate points. Response returned exactly 5 bullets, correctly identified all 5 SOLID principles as the core content, and folded the "why it matters" sentence into a closing note rather than forcing a 6th bullet — real prioritization, not truncation.
**Status:** Explain, Quiz, and Summarise are now all ✅ built, structurally verified, and functionally verified. SK Planner (to route between them by intent) is the next major SK milestone per Section 10.

### AD-018 — Automated evaluation via Microsoft.Extensions.AI.Evaluation
**Decision:** Adopt `Microsoft.Extensions.AI.Evaluation` (plus `Microsoft.Extensions.AI.Evaluation.Quality` for the Relevance/Groundedness/Completeness/Fluency/Coherence/Truthfulness/Equivalence evaluators) as the eval layer for StudyBuddy, to replace the manual curl-and-eyeball verification process used for Quiz and Summarise with automated, repeatable scoring.
**Why:** This is the .NET-native equivalent of LangSmith's evaluation half (see Section 4). It also directly maps to real "AI evals" job requirements Royson is researching — defining a metric, describing a test set, and catching a specific failure mode are the core eval-engineering skills, and this library lets StudyBuddy demonstrate exactly that, in .NET rather than the more common Python eval ecosystem.
**Scope:** Build a small eval test set per mode (a handful of study-material + expected-quality-bar pairs), wire the Quality evaluators against Explain/Quiz/Summarise outputs, and run it as a repeatable regression check rather than a one-off manual test.
**How results are viewed:** `Microsoft.Extensions.AI.Evaluation.Reporting` caches eval run results to disk. Install the CLI once via `dotnet tool install Microsoft.Extensions.AI.Evaluation.Console`, then generate a report with `dotnet aieval report --path <cache folder> --output report.html --open` — produces an interactive HTML report (drill-down from high-level scores to individual test cases, historical trend tracking across runs) that opens in the browser. This is separate from SK Telemetry, which prints token/latency/call info to the console in real time. Both are developer-facing tools — neither appears in the StudyBuddy React frontend, same as how LangSmith/Datadog dashboards work at companies (internal tooling, not product UI).
**Status:** Not yet started — logged as Phase 5 in Section 11.

### AD-019 — Developer Dashboard: deferred until after the student-facing frontend *(SUPERSEDED — built 29 July, see AD-026)*
**Decision:** A "Developer" tab/view (separate from the student-facing Explain/Quiz/Summarise screens) showing live telemetry and evaluation results was discussed and architected, but deliberately deferred. Sequencing: Phase 1 (student-facing React frontend) ships first, so Royson can see the app work for its primary purpose end-to-end; the Developer Dashboard becomes its own later phase (Phase 6) once that's solid.
**Why:** Building the dashboard now would mean scaffolding the React app shell around developer tooling before the actual product exists. Royson explicitly wants to see the primary-purpose app running first, then layer in developer-facing tooling.
**Architecture, for when this phase starts (do not build yet):**
- **Telemetry (can be near-live):** SK Telemetry currently only prints to console (AD-005) — not stored or queryable. Needs an Infrastructure-layer capture mechanism (in-memory store of recent calls: mode, tokens in/out, latency, timestamp) plus a new API endpoint the frontend polls every few seconds. Polling recommended over Server-Sent Events/WebSockets — simpler, SOLID-clean, upgradeable later without touching Domain.
- **Evals (cannot be live — this is true industry-wide, not a limitation of this project):** Evals score a curated test set using an LLM-as-judge, which is slow and costly — no real eval tool (LangSmith, Datadog, etc.) runs evals synchronously per live user request. Needs `POST /api/dev/evals/run` (triggers the Microsoft.Extensions.AI.Evaluation suite programmatically) and `GET /api/dev/evals/latest` (returns scored results as JSON). The full interactive `dotnet aieval` HTML report (AD-018) stays as a separate, complementary deep-dive tool rather than being rebuilt inside React.
- **Frontend:** a distinct Developer tab/route, clearly separated from student-facing screens.
**Status:** Architecture decided, build deferred. Prerequisite: Phase 1 React frontend must exist first (tab needs a shell to live in). Do not draft a Cursor prompt for this until Royson asks.

### AD-020 — StudyBuddy frontend uses a fixed, dedicated dev port (5180)
**Decision:** `frontend/vite.config.ts` pins the dev server to port 5180 with `strictPort: true`. The CORS policy in `Program.cs` is locked to `http://localhost:5180` accordingly.
**Why:** Royson also runs NOSYOR.M.I locally, which legitimately occupies Vite's default port 5173. Without a fixed port, StudyBuddy's Vite server would silently drift to 5174/5175/etc. whenever 5173 was taken, which quietly breaks CORS (the policy only allows one exact origin) and produced a confusing multi-round debugging session. A dedicated port removes the collision entirely; `strictPort: true` means if 5180 is ever unexpectedly occupied, Vite fails loudly instead of silently drifting again.
**Implication:** StudyBuddy's frontend always runs at `http://localhost:5180`. If this port ever needs to change, both `vite.config.ts` and the CORS origin in `Program.cs` must be updated together.

### AD-021 — TTS switched to browser Web Speech API (ElevenLabs since fully removed — see AD-024)
> **Superseded in part by AD-024:** the ElevenLabs code described below as "kept dormant" was later deleted entirely. The Web Speech API decision still stands.

**Decision:** Audio playback uses the browser's built-in Web Speech API (`window.speechSynthesis`), entirely client-side. The ElevenLabs backend integration (`ISpeechService`, `ElevenLabsSpeechService`, `SpeechController`, `POST /api/speech`) was initially kept in the codebase, fully working but unused.
**Why:** ElevenLabs was built and verified working (AD-007 superseded), but its free tier caps at ~10,000 credits/month ≈ 1 credit per character — roughly 3 full Explain responses. Not viable for daily study use. Two earlier errors during integration were both plan limits, not code faults: `402 paid_plan_required` (free accounts can't use Voice Library voices via API — resolved by switching to a `premade` voice, Alice `Xb7hH8MSUJpSbSDYk0k2`), then `401 quota_exceeded`. Web Speech API is free, unlimited, and needs no API key or backend call; the tradeoff is a more robotic voice.
**Why keep the ElevenLabs code:** It sits behind the `ISpeechService` abstraction with zero coupling to the tutoring modes — it costs nothing to leave in place and can be re-enabled by swapping the `PlayButton` implementation back if the plan is ever upgraded. This is a concrete payoff of the layer isolation in AD-001.
**Implementation notes:** `PlayButton.tsx` chunks text (~200 chars, on sentence boundaries) because Chrome silently truncates long utterances; voices load asynchronously via the `voiceschanged` event; `interrupted`/`canceled` utterance errors are ignored since they fire on deliberate stop.
**Env vars `ELEVENLABS_API_KEY` / `ELEVENLABS_VOICE_ID`** are no longer required to run the app, but remain valid if the ElevenLabs path is re-enabled.

### AD-022 — OPEN ISSUE: Web Speech voice quality unresolved (paused 27 July 2026)
**State:** Voice + speed picker is built and working (`useSpeechVoices` hook + `PlayButton`, choice persisted in `localStorage`, speed 0.5x–1.5x). Playback functions correctly. **The problem is voice quality only** — the available voices sound flat and robotic.
**What was tried:** Royson downloaded macOS voices via System Settings → Accessibility → **Read & Speak** (renamed from "Spoken Content" in his macOS version) → System Voice → Manage Voices. The downloaded voices did **not** appear in the app's dropdown — it only listed basic voices (e.g. "Shelley (English (United Kingdom))").

**ROOT CAUSE IDENTIFIED:** He downloaded **Siri voices**. Apple does not expose Siri voices to the Web Speech API — or even to native third-party apps via `AVSpeechSynthesizer`. They are reserved for system use. No amount of code change will surface them; this is an Apple platform restriction, not a StudyBuddy bug.

**CORRECTION — earlier guidance in this file was wrong:** an earlier version of this entry said to try Safari because it exposes more voices. That is backwards. **Chrome and Edge list all installed macOS voices; Safari is the browser that restricts them.** Stay on Chrome.

**Fix to try when resuming:**
1. Download **Enhanced** or **Premium** variants of the *regular, non-Siri* voices — Ava, Allison, Tom, Samantha, Evan, Joelle, Nathan. These are ordinary system voices that Chrome can see, and the Enhanced versions are substantially better than the basic ones.
2. Fully quit Chrome (Cmd+Q, not just the tab) and relaunch — Chrome caches the system voice list at launch, so voices installed mid-session won't appear.
3. Diagnostic — run in DevTools console to see what the browser can actually access:
   `speechSynthesis.getVoices().filter(v => v.lang.startsWith('en')).forEach(v => console.log(v.name, '|', v.lang, '|', v.localService ? 'local' : 'network'));`
4. If the Enhanced non-Siri voices still aren't good enough: build the local neural TTS option (Kokoro via transformers.js — runs fully in-browser, free and unlimited, high quality, but ~300MB model download and a significantly bigger build).

**No code change is required for steps 1–3** — the voice picker already lists whatever the browser exposes.

**CURRENT WORKAROUND IN USE (good enough for now):** Royson generates the explanation in StudyBuddy, then highlights the text and triggers **macOS Speak selection** (System Settings → Accessibility → Read & Speak → Speak selection, toggled on). Because this is a system-level feature rather than a browser one, it *can* use Siri voices — giving Claude's tutoring content with Siri's delivery quality. The in-app Web Speech playback remains available but is not his preferred path. **This is acceptable and not blocking** — revisit only if he wants in-app audio to match that quality, which would mean the Kokoro/transformers.js local neural TTS build.

### AD-023 — Single start script for local dev; Docker deferred
**Decision:** `start.sh` at the repo root launches both the backend (`dotnet run`, port 5017) and the frontend (`npm run dev`, port 5180) together; one Ctrl+C shuts both down. It pre-checks both ports and fails with a clear message if either is occupied.
**Why not Docker (yet):** Royson asked about containerising like NOSYOR.M.I. Deliberately deferred — for daily development Docker adds friction: hot reload inside containers needs volume mounts and polling config on macOS, container networking breaks the `localhost:5017` API base URL and the pinned CORS origin from AD-020, and the API key needs new plumbing. The script solves the actual annoyance (two terminals) with none of that.
**When to revisit Docker:** when deploying, when PostgreSQL is actually wired up (scaffolded but unused per AD-006), or when someone else needs to run the project without installing .NET and Node.
**Usage:** `chmod +x start.sh` once, then `./start.sh` from the repo root.

### AD-024 — ElevenLabs layer fully removed from the codebase
**Decision:** The entire ElevenLabs TTS layer was deleted rather than kept dormant. This reverses the "keep it in place" part of AD-021.
**Why:** Royson chose a clean codebase over a reversible option he was unlikely to use — the Web Speech API plus his macOS Speak-selection workaround (AD-022) cover his needs, and the ElevenLabs free tier was never viable for daily study.
**Removed (6 files deleted):** `ElevenLabsSpeechService.cs`, `ElevenLabsOptions.cs`, `ISpeechService.cs`, `SpeechRequest.cs`, `SpeechResult.cs`, `SpeechController.cs`.
**Also cleaned:** ElevenLabs wiring stripped from `DependencyInjection.cs` (Configure, named HttpClient, service registration); `Microsoft.Extensions.Http` package reference dropped from `StudyBuddy.Infrastructure.csproj` (nothing else used HttpClient); `ElevenLabs` section removed from `appsettings.json` and `appsettings.Development.json`; `ELEVENLABS_API_KEY` / `ELEVENLABS_VOICE_ID` removed from `backend/.env.example`.
**Verified 27 July 2026:** `dotnet build` succeeded; independent search confirmed zero remaining references to `ElevenLabs`, `ISpeechService`, `SpeechResult`, `SpeechRequest`, or `SpeechController` in source. Frontend (`PlayButton.tsx`, `useSpeechVoices.ts`) and all three tutoring plugins untouched.
**Implication:** `POST /api/speech` no longer exists. Audio playback is entirely client-side. If ElevenLabs is ever wanted again it must be rebuilt from scratch — but AD-021's original prompt structure and the `ISpeechService` abstraction shape are documented here and in git history (commit `ec3ad88`) as a starting point.
**Env vars no longer needed:** `ELEVENLABS_API_KEY`, `ELEVENLABS_VOICE_ID` — safe to remove from the shell profile.

### AD-025 — Certificate revocation check disabled in Development (SSL fix)
**Symptom (29 July 2026):** Every OpenRouter call suddenly failed — the app had been working the day before, no relevant code had changed. Error: `AuthenticationException: The remote certificate is invalid because of errors in the certificate chain: RevocationStatusUnknown`, surfacing as `HttpOperationException: The SSL connection could not be established`.
**Diagnosis:** `curl -I https://openrouter.ai/api/v1` returned 200 OK, proving the network could reach OpenRouter and the certificate itself was fine. The difference: curl does not perform certificate revocation checks by default, .NET does. The network was reaching OpenRouter on 443 but could not reach the OCSP/CRL endpoints that answer "has this certificate been revoked?" — so .NET treated the unknown status as a validation failure. Retrying did not help (not a transient OCSP outage).
**Fix:** In `Program.cs` → `ConfigureSemanticKernel`, when `builder.Environment.IsDevelopment()`, Semantic Kernel is given a custom `HttpClient` built on a `SocketsHttpHandler` with `SslOptions.CertificateRevocationCheckMode = X509RevocationMode.NoCheck`, passed via the `httpClient:` parameter of `AddOpenAIChatCompletion`.
**Scope and security note:** This disables **only** the revocation lookup, **only** for the OpenRouter client, **only** in Development. Certificate chain and hostname validation remain fully enforced — this is *not* the "trust any certificate" switch. Production behaviour is unchanged.
**If this resurfaces:** it is environmental, not a code bug. Root cause is the network blocking OCSP/CRL (common on ISP-filtered, corporate, VPN, or DNS-filtered connections). Testing on a phone hotspot isolates it. If the app is ever deployed, the same wall would appear in production and should be solved at the network level rather than by carrying this workaround forward.
**Related fix, same commit:** `start.sh` originally used `wait -n`, which requires bash 4+. macOS ships bash 3.2, so the script exited immediately after starting both servers and then killed them. Replaced with a `kill -0` polling loop.

### AD-026 — Phases 5 and 6 built together: Automated Evaluation + Developer Dashboard
**Decision:** Both the eval layer (Phase 5 / AD-018) and the Developer Dashboard (Phase 6 / AD-019) were built on 29 July 2026, ahead of the sequencing in AD-019. This supersedes AD-019's "deferred, do not build yet" status — its prerequisite (the Phase 1 student frontend) was already met.
**Built in a separate Cursor session** driven from a different project folder, so Cowork had no context until Royson flagged it. Code was then scanned and reviewed here.

**Evaluation half (Phase 5):**
- Packages: `Microsoft.Extensions.AI.Evaluation` 10.8.0 + `Microsoft.Extensions.AI.Evaluation.Quality` 10.8.0 (Infrastructure)
- `IEvalRunnerService` / `EvalRunnerService` — calls the real Explain/Quiz/Summarise services (does not duplicate their logic), then scores outputs with a `CompositeEvaluator`: `GroundednessEvaluator`, `FluencyEvaluator`, `CoherenceEvaluator`, and the experimental `RelevanceTruthAndCompletenessEvaluator` (needs `#pragma warning disable AIEVAL001`)
- `IEvalTestSetProvider` / `HardcodedEvalTestSetProvider` — curated test cases per mode; `IEvalResultStore` / `InMemoryEvalResultStore` caches the last run
- Endpoints: `POST /api/dev/evals/run`, `GET /api/dev/evals/latest` (`DevEvalsController`)
- **Button-driven only, never polled** — correct, since each run makes many real LLM calls (mode outputs + LLM-as-judge scoring)

**Telemetry half (Phase 6):**
- `ITelemetryStore` / `InMemoryTelemetryStore` — thread-safe bounded ring, max 200 entries
- `TelemetryFunctionInvocationFilter` — an SK `IFunctionInvocationFilter` capturing latency and token usage **without modifying any plugin code**; uses `AsyncLocal` so nested prompt calls attribute usage to the outermost tracked mode
- Endpoints: `GET /api/dev/telemetry/recent`, `GET /api/dev/telemetry/summary` (`DevTelemetryController`)
- Frontend: `DeveloperDashboard.tsx` at route `/dev`, `useTelemetryPolling` (4s interval), `devApi.ts` kept separate from `studyApi.ts`

**Cost clarification (Royson's concern, resolved):** the 4-second polling costs **nothing**. Both telemetry endpoints are pure in-memory reads — no Kernel invocation, no OpenRouter call, no tokens. The displayed cost is arithmetic over token counts already recorded; it only grows when a tutoring mode is used or evals are run. The 4-second refresh merely redraws the same figure.

**Two real defects found on review — both FIXED 29 July 2026 (see AD-027).**

---

### AD-027 — Dashboard defects fixed: cost accuracy + eval/tutoring separation
**Both AD-026 defects are resolved.** Build verified (`dotnet build` + `npm run build`) and independently confirmed by file inspection in Cowork. Tutoring plugins, services, and prompt templates untouched.

**Fix 1 — cost accuracy (was ~100x too high):**
- `EstimatedCostUsdPer1kTokens = 0.25m` deleted entirely
- Pricing moved to configuration: `OpenRouterOptions.InputCostPerMillionUsd` (default `1.00`) and `OutputCostPerMillionUsd` (default `5.00`), mirrored in `appsettings.json` under the `OpenRouter` section — so a pricing change is now a config edit, not a code change
- `InMemoryTelemetryStore` injects `IOptions<OpenRouterOptions>` (safe — it's a Singleton) and computes cost via `CalculateCostUsd(tokensIn, tokensOut)`, charging input and output at their **separate** rates rather than one blended per-1k figure

**Fix 2 — eval traffic no longer counted as tutoring traffic:**
- New `IEvalExecutionContext` (Application) with `bool IsEvalRun` and `IDisposable BeginEvalRun()`; implemented in Infrastructure as `EvalExecutionContext` using an `AsyncLocal<int>` **depth counter** so nested scopes behave correctly
- `EvalRunnerService.RunAsync` opens the scope for its whole body (`using var _ = _evalExecutionContext.BeginEvalRun();`), so every downstream Kernel call inherits the marker via async flow
- `TelemetryFunctionInvocationFilter` injects the context and stamps `TelemetryEntry.Source` as `TelemetrySource.Eval` or `TelemetrySource.Tutoring`
- `GetSummary` partitions today's entries by source: `CallsToday` / `AverageLatencyMs` / `TotalTokens` / `EstimatedCostUsd` are now **tutoring-only**, with `EvalCallsToday` / `EvalTotalTokens` / `EvalEstimatedCostUsd` reported alongside
- Registered `AddSingleton<IEvalExecutionContext, EvalExecutionContext>()`

**Why this design:** the marker is ambient rather than a parameter threaded through the services, so `IExplainService`/`IQuizService`/`ISummariseService` and their plugins stay completely unaware that evaluation exists. That preserves the layer isolation from AD-001 — the eval suite depends on the tutoring services, never the reverse.

**Frontend:** `TelemetryEntry` gained `source`, `TelemetrySummary` gained the three eval fields; dashboard shows a "Source" column, relabelled "Est. cost (tutoring)", and added an "Eval cost (N runs)" card. `useTelemetryPolling` now returns `isStale` — a failed poll that still has last-good data renders as a soft "showing last known data" notice instead of a bare error, fixing the confusing "Failed to fetch" appearing beside valid numbers.

**Learning note for the eval discussion:** this is a good concrete example of *observability hygiene* — an eval suite that pollutes production metrics makes both numbers useless. Separating traffic by source is standard practice in real eval tooling, and being able to explain why is the kind of thing eval-engineering interviews probe.

---

### AD-028 — three eval gaps blocking the Phase 5 learning goal *(RESOLVED 30 July — see AD-029)*
Royson asked on 29 July whether eval runs work as intended and why he sees no report. Verdict: **the run itself works correctly** — button → `POST /api/dev/evals/run` → `RunAsync` (inside the eval scope) → for each test case, call the real tutoring service, score the output with the `CompositeEvaluator`, average per mode → save → render score bars. Test set is 11 cases (4 Explain, 4 Quiz, 3 Summarise), all grounded in SOLID / dependency-injection material, so a full run is roughly 25 LLM calls — hence correctly button-gated.

**But three gaps exist:**

1. **Per-case results are discarded.** `EvaluateModeAsync` sums each test case's scores into running totals and returns only per-mode averages. If Explain scores 3.2 on Groundedness, there is no way to tell which of the four cases dragged it down.
2. **Evaluator reasoning is discarded.** `TryAccumulate` reads only `metric.Value`; the diagnostics explaining *why* a score was assigned are dropped.
3. **No report, and no history.** `Microsoft.Extensions.AI.Evaluation.Reporting` is **not installed** — only `Evaluation` and `Evaluation.Quality`. Reporting is what caches runs to disk and enables `dotnet aieval report --output report.html --open` with drill-down and historical trends. Separately, `InMemoryEvalResultStore` holds a single result in memory, so all eval history dies on backend restart — which defeats regression testing (comparing scores before/after a prompt change).

**Why 1 and 2 are the priority:** they structurally prevent the Phase 5 deliverable "document a specific failure mode the eval caught" — the single most career-relevant artifact for the AI-evals roles Royson is targeting. The code currently cannot say *which* case failed or *why*.

**Correction to AD-018:** that entry described the `dotnet aieval report` HTML report as though it were part of the plan being implemented. It never was — the Reporting package was never added. What exists today is averaged score bars in the dashboard only.

**Agreed order for 30 July:** (1) draft + apply the fix prompt, (2) then walk through the numbers together — what each metric measures, how LLM-as-judge works, how to read the scores.

---

### AD-029 — All three eval gaps closed; reporting live and functionally verified (30 July 2026)
Prompt drafted in a separate chat, applied by Cursor, then structurally verified in Cowork. `dotnet build` and `npm run build` both succeed. Tutoring plugins, services, and prompt templates untouched.

**Gap 1 — per-case results now retained:**
- `EvalTestCase` gained a `Name`; fixtures use readable IDs (`Explain-SRP`, `Explain-OCP`, `Explain-DI`, `Explain-DIP-DI`, `Quiz-SRP`, `Quiz-LSP`, `Quiz-DI`, `Quiz-ISP`, `Summarise-SOLID`, `Summarise-DI`, `Summarise-SOLID-why`)
- New `EvalCaseResult(CaseName, Metrics)`; `ModeEvalScores` now carries both `Scores` (mode averages, drives the bars) and `CaseResults` (per-case detail)

**Gap 2 — evaluator reasoning now captured:**
- New `EvalMetricResult(double Value, string? Reasoning)`
- `TryCapture` reads `metric.Reason` alongside `metric.Value`, nulling blanks. Mode averages derive from the same per-case values — no double scoring.

**Gap 3 — reporting and durable history:**
- Added `Microsoft.Extensions.AI.Evaluation.Reporting` 10.8.0
- New `IEvalReportWriter` / `DiskEvalReportWriter` — evaluation now runs through `DiskBasedReportingConfiguration` + per-scenario `ScenarioRun`s, writing to `eval-reports/` (`cache/` + `results/`). `enableResponseCaching: true`, so repeat runs on unchanged cases reuse cached judge responses instead of paying again.
- `InMemoryEvalResultStore` **deleted**, replaced by `FileEvalResultStore` writing timestamped JSON to `eval-history/` — eval results now survive backend restarts
- `GET /api/dev/evals/history` added; `/run` and `/latest` response shapes unchanged
- Frontend: collapsible per-case breakdown under each mode, showing metric scores and reasoning. History UI deliberately deferred.

**DI lifetimes matter here:** `IEvalReportWriter` is registered **Scoped**, so each "Run evals" click gets a fresh `executionName` timestamp. Registering it Singleton would collapse every run into one execution and break the report's historical trend view.

**Functional verification (30 July, from the dashboard):** an 11-case run completed successfully. Eval cost displayed **$0.0171** — confirming the AD-027 cost fix works and is now realistic. The Source column correctly showed `Eval` for all eval traffic and `Tutoring` for the one real call, confirming the AD-027 separation works. Scores: Explain (Groundedness 4.75, Relevance 5.00, Completeness 4.25, Fluency 4.00, Coherence 4.00, Truthfulness 5.00); Quiz (5.00 / 5.00 / 4.00 / 4.00 / 4.25 / 5.00); Summarise (4.67 / 5.00 / 5.00 / 4.00 / 4.33 / 5.00).

**HTML report generated and confirmed working** — 854KB `report.html` at repo root.

**CLI gotcha, documented so it doesn't recur:** the tool is invoked as **`aieval`**, NOT `dotnet aieval`. The `dotnet <cmd>` form only works for executables named `dotnet-<cmd>`; `dotnet tool list --global` shows the command as plain `aieval`. Correct sequence:
```bash
dotnet tool install --global Microsoft.Extensions.AI.Evaluation.Console
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.zshrc && source ~/.zshrc   # if not found
cd "/Users/roysondsouza/AI Projects/STUDYBUDDY"
aieval report --path eval-reports --output report.html --open
```
The doc comment in `DiskEvalReportWriter.cs` still says `dotnet aieval report` — **minor open fix**, should be corrected to `aieval report`.

**Gitignore:** `eval-reports/` and `eval-history/` added. **`report.html` still needs adding** — it is 854KB of regenerable output sitting untracked at the repo root and would otherwise be committed (same class of mistake as the `bin`/`obj` issue).

**First observation worth investigating (Phase 5 learning goal):** Fluency scored **exactly 4.00 across all three modes**, the lowest and suspiciously uniform metric. Open question: is the judge flagging something real about the tutoring prose, or is 4 simply where that evaluator lands for competent-but-unexceptional writing? The per-case reasoning now captured can answer this from evidence — and answering it *is* the Phase 5 deliverable.

---

## 8. THE TTS LAYER — BROWSER WEB SPEECH API

**Current implementation (client-side only, no backend involvement):**
- Each result panel renders a `PlayButton` with a "Read aloud" control, a voice dropdown, and a 0.5x–1.5x speed slider
- `useSpeechVoices` hook loads the browser's available English voices and persists the chosen one in `localStorage`
- Text is chunked to ~200 characters on sentence boundaries, because Chrome silently truncates long utterances
- Files: `frontend/src/components/PlayButton.tsx`, `frontend/src/hooks/useSpeechVoices.ts`

**Why not ElevenLabs (it was built, then removed):**
- ElevenLabs was fully implemented and verified working (`ISpeechService`, `POST /api/speech`), then deleted — see AD-021 and AD-024
- Free tier caps at ~10,000 credits/month ≈ 1 credit per character, roughly 3 full Explain responses. Not viable for daily study.
- Two errors hit during integration were both plan limits, not code faults: `402 paid_plan_required` (free accounts can't use Voice Library voices via API) then `401 quota_exceeded`

**Known limitation — voice quality (AD-022):**
- Browser voices sound flat. Apple blocks Siri voices from the Web Speech API entirely (and from native third-party apps too), so the best macOS voices are unreachable from Chrome.
- Chrome and Edge expose all installed macOS voices; Safari exposes fewer. Stay on Chrome.
- **Workaround in daily use:** generate the explanation in StudyBuddy, highlight the text, and trigger macOS **Speak selection** — a system-level feature, so it *can* use Siri voices.
- Open upgrade path if that stops being enough: Kokoro via transformers.js, a local neural TTS model running fully in-browser — free, unlimited, high quality, but ~300MB model download and a significantly larger build.

**Still true:** Claude cannot generate audio; Anthropic has no TTS API. Any spoken output comes from a separate service or the OS.

---

## 9. HOW STUDY MATERIAL GETS INTO THE APP

The app is source-agnostic — it never connects to any external platform directly. All content arrives as plain text through one of these methods:

**Method 1 — Copy-paste (in use):** ✅ The frontend has **one shared study material textarea** owned by `App.tsx` and passed down to all three mode panels, so material is pasted once per session and reused across Explain, Quiz, and Summarise. Every API call sends it as `studyMaterial`.
**Method 2 — File upload (Phase 4 enhancement):** `.txt` / `.md` upload. Not built.
**Method 3 — URL fetch (future consideration):** Public URL fetch only. Not built.

**Note on statelessness:** the app holds nothing between page refreshes — material lives in React state only. Royson confirmed on 29 July that this is fine; he prefers pasting fresh each session, so persistence (Phase 4 / AD-006) is deliberately not being pursued.

**What the app will never do:** Log into any platform on the user's behalf, scrape authenticated pages, or try to access content behind a paywall.

---

## 10. WHAT THIS PROJECT TEACHES (LEARNING GOALS)

By building this app, Royson will directly experience:

1. **SK Plugin definition** — ✅ all three plugins built (Explain, Quiz, Summarise), including a two-function plugin (QuizPlugin)
2. **SK Planner routing** — ⏳ next major SK learning milestone — Quiz and Summarise plugins are both done, so this is now the immediate next step
3. **Prompt Templates** — ✅ four templates across three modes, including conditional template selection (with/without a user question or topic)
4. **SK Telemetry** — ✅ console OTel wired for Claude calls
5. **Multi-service orchestration** — ✅ experienced via the ElevenLabs build (external REST API behind an Application-layer abstraction), even though that layer was later removed (AD-024). The lesson landed: clean isolation made both adding *and* deleting it low-risk.
6. **Claude API behaviour** — ✅ via OpenRouter + Haiku 4.5
7. **Eval literacy** — 🟡 Phase 5 built (AD-026): evaluators wired, LLM-as-judge running, scores rendered. **The learning half is still outstanding** — documenting a metric definition, test-set rationale, and a specific failure mode caught. That articulation, not the wiring, is what the job market actually tests for. Original framing: Phase 5 (AD-018): defining metrics, building a test set, automated grading via `Microsoft.Extensions.AI.Evaluation`. Directly relevant to "AI evals engineer" / AI quality roles Royson is researching — the transferable skill is eval literacy itself (metric definition, test-set design, failure-mode tracing), not the specific tool, since most eval tooling in the job market is Python-based and this project is .NET.

---

## 11. BUILD SEQUENCE & CURRENT PROGRESS

**Phase 1 — Core**
- [x] .NET API scaffold (Clean Architecture)
- [x] SK setup with ExplainPlugin only
- [x] Claude via OpenRouter (SK OpenAI connector)
- [x] SK Telemetry logging to console
- [x] Verify path: message in → Claude explanation out *(manual with API key)*
- [x] React frontend (React 19 + TS + Vite) with shared study material input and three mode panels
- [x] End-to-end UI verification — all three modes exercised in the browser

**Phase 2 — All three modes**
- [x] QuizPlugin (`quiz/questions` + `quiz/evaluate`) — built, structurally verified, and functionally verified end-to-end
- [x] SummarisePlugin (`summarise`) — built, structurally verified, and functionally verified end-to-end
- [ ] SK Planner to route by intent
- [ ] Verify planner picks the correct mode

**Phase 3 — TTS** *(complete; ElevenLabs path built then removed)*
- [x] ~~ElevenLabs client + `POST /api/speech`~~ — built and verified, then fully removed (AD-024)
- [x] "Read aloud" playback in all three modes — browser Web Speech API, client-side only (AD-021)
- [x] Voice picker + speed control, choice persisted in `localStorage`
- [ ] Voice *quality* still unsatisfying in-browser — see AD-022 (workaround in use, not blocking)

**Phase 4 — Polish**
- [ ] Persistent study material loading / file upload
- [ ] Session history in PostgreSQL
- [ ] UI polish

**Phase 5 — Automated Evaluation (AD-018, built 29 July — see AD-026)**
- [x] Add `Microsoft.Extensions.AI.Evaluation` + `Microsoft.Extensions.AI.Evaluation.Quality` packages (10.8.0)
- [x] Define a small eval test set per mode — `HardcodedEvalTestSetProvider`
- [x] Wire Groundedness / Fluency / Coherence / RelevanceTruthAndCompleteness evaluators against all three modes
- [x] `POST /api/dev/evals/run` + `GET /api/dev/evals/latest` + `GET /api/dev/evals/history`
- [x] Per-case results retained with case names (AD-029)
- [x] Evaluator reasoning captured, not just scores (AD-029)
- [x] `Microsoft.Extensions.AI.Evaluation.Reporting` + `DiskEvalReportWriter` → `eval-reports/`; HTML report generated and verified via `aieval report` (AD-029)
- [x] Durable history — `FileEvalResultStore` → `eval-history/`, survives restarts (AD-029)
- [ ] Add `report.html` to `.gitignore` (854KB generated artifact, currently untracked at repo root)
- [ ] Fix doc comment in `DiskEvalReportWriter.cs`: `dotnet aieval report` → `aieval report`
- [ ] **Document at least one failure mode the evals catch** — the remaining Phase 5 deliverable and the most career-relevant artifact. Starting thread: why is Fluency exactly 4.00 across all three modes? (AD-029)

**Phase 6 — Developer Dashboard (AD-019 superseded, built 29 July — see AD-026)**
- [x] Telemetry capture layer — `InMemoryTelemetryStore` (bounded ring, 200 entries) + `TelemetryFunctionInvocationFilter`
- [x] `GET /api/dev/telemetry/recent` + `GET /api/dev/telemetry/summary`
- [x] `POST /api/dev/evals/run` + `GET /api/dev/evals/latest` — trigger and retrieve eval results
- [x] Developer route (`/dev`) in the frontend, separate from student-facing screens
- [x] **Cost accuracy fixed** — pricing config-driven, input/output charged separately (AD-027)
- [x] **Eval traffic separated from tutoring traffic** via `IEvalExecutionContext` + `TelemetryEntry.Source` (AD-027)
- [x] Staleness indicator (`isStale`) in the polling hook (AD-027)

---

## 12. HOW TO RUN WHAT EXISTS TODAY

### Normal use — one command (AD-023)

```bash
studybuddy
```

A shell alias in `~/.zshrc` pointing at `./start.sh`, which starts the backend (port 5017) and frontend (port 5180) together. Then open **http://localhost:5180** in Chrome. Ctrl+C once stops both.

If it reports a port is already in use, something is still running — most often a `dotnet run` that Cursor started while verifying a build. Free it and retry:

```bash
lsof -ti:5017 | xargs kill -9
```

Setup was one-time: `chmod +x start.sh`, plus the alias
`alias studybuddy='cd "/Users/roysondsouza/AI Projects/STUDYBUDDY" && ./start.sh'` in `~/.zshrc`.

### Developer Dashboard and evaluation

Open **http://localhost:5180/dev** — live telemetry (4s polling, free in-memory reads) plus on-demand evaluation. "Run evals" makes ~25 real LLM calls across 11 test cases and costs roughly $0.02.

To view the full interactive eval report (per-case scores, evaluator reasoning, historical trends):

```bash
# one-time setup
dotnet tool install --global Microsoft.Extensions.AI.Evaluation.Console
echo 'export PATH="$PATH:$HOME/.dotnet/tools"' >> ~/.zshrc && source ~/.zshrc

# after any "Run evals" click
cd "/Users/roysondsouza/AI Projects/STUDYBUDDY"
aieval report --path eval-reports --output report.html --open
```

⚠️ The command is **`aieval`**, not `dotnet aieval` — see AD-029.

### Backend only (for API testing)

```bash
export OPENROUTER_API_KEY="your-key"   # or rely on appsettings.Development.json
cd backend/StudyBuddy.API
dotnet run
```

```bash
curl -X POST http://localhost:5017/api/study/explain \
  -H "Content-Type: application/json" \
  -d '{"userMessage":"What is dependency injection?","studyMaterial":"Dependency injection means..."}'
```

Expect JSON `{ "explanation": "..." }` and SK telemetry spans in the console.

```bash
curl -X POST http://localhost:5017/api/study/quiz/questions \
  -H "Content-Type: application/json" \
  -d '{"topic":"Dependency injection","studyMaterial":"Dependency injection means..."}'

curl -X POST http://localhost:5017/api/study/quiz/evaluate \
  -H "Content-Type: application/json" \
  -d '{"questions":"1. ...\n2. ...\n3. ...","studentAnswers":"1. ...\n2. ...\n3. ...","studyMaterial":"Dependency injection means..."}'
```

Expect JSON `{ "questions": "..." }` and `{ "evaluation": "..." }` respectively. **Functionally verified 27 July 2026.**

```bash
curl -X POST http://localhost:5017/api/study/summarise \
  -H "Content-Type: application/json" \
  -d '{"studyMaterial":"..."}'
```

Expect JSON `{ "summary": "..." }`. **Functionally verified 27 July 2026.**

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
**Stack:** .NET 10 + React 19 + Semantic Kernel + Claude Haiku via OpenRouter + browser Web Speech API  
**Repo:** Local git repo at `studybuddy` (separate from NOSYOR.M.I)  
**Design:** Keep it simple and functional — learning tool first  
**Standing coding rule:** Follow SOLID principles and clean coding structure throughout

---

## 14. OPEN QUESTIONS (STILL TO DECIDE)

- [x] ~~Anthropic SDK direct vs OpenRouter~~ → **OpenRouter for Phase 1** (AD-002)
- [x] ~~SQLite vs PostgreSQL~~ → **PostgreSQL via EF Core** (AD-006)
- [x] ~~ElevenLabs voice selection — which voice fits a tutor persona?~~ → **Moot.** ElevenLabs removed entirely (AD-024). Free tier capped at ~10k chars/month. Alice (`Xb7hH8MSUJpSbSDYk0k2`, "Clear, Engaging Educator") was the chosen premade voice while it was live.
- [ ] In-app TTS voice quality — browser voices are flat; Siri voices are blocked from browsers by Apple. Workaround in use (macOS Speak selection). Open path: Kokoro/transformers.js local neural TTS (AD-022).
- [ ] PostgreSQL schema beyond `StudyMaterial` — what else to persist (sessions, quiz history)? **Lower priority:** Royson confirmed 29 July he's fine pasting material each session, so persistence is not currently wanted (AD-006 scaffolding stays unused).
- [x] ~~When to introduce SK Planner — after both Quiz + Summarise plugins exist, or earlier?~~ → **After.** Both are built and verified; Planner is now the next SK milestone whenever he chooses to pick it up.
- [x] ~~GitHub connectivity for AI context~~ → **Resolved by local folder mount in Cowork** (AD-011). GitHub Desktop still used for version control separately.
- [ ] GitHub remote / project board setup (for version control and backups — separate from AI context)
- [ ] Whether to switch from Haiku to a larger Claude model for Quiz evaluation quality — now testable since QuizPlugin is live. Note: Haiku's evaluation quality tested well on 27 July (correctly caught both deliberately wrong answers), so this is optional rather than needed.
- [ ] Secrets access boundary — switch `OPENROUTER_API_KEY` from `appsettings.Development.json` to `dotnet user-secrets` or shell env var (AD-014). Still open; the key currently lives in the gitignored Development settings file.
- [ ] Certificate revocation workaround (AD-025) is Development-only — if the app is ever deployed, the network-level OCSP/CRL blocking must be solved properly rather than carrying the workaround into production.

---

## 15. KEY CONTEXT — WHO IS BUILDING THIS

- **Builder:** Royson D'Souza
- **Background:** MBA graduate, no prior coding background, learning through building
- **Existing project:** NOSYOR.M.I — full-stack AI personal finance app (.NET 10 + React 19 + pgvector + Docker)
- **Tools:** Cursor IDE (Agent mode), Claude Code extension in Cursor, GitHub Desktop, Claude Cowork (desktop app)
- **Working style:** Architecture first in Cowork, then Cursor builds the code, then back to Cowork to verify and plan next steps
- **Standing rule:** Every Cursor prompt must include "Follow SOLID principles and clean coding structure throughout"
- **Standing rule:** Claude Cowork reminds Royson to commit and push to GitHub (via GitHub Desktop) after any session with a significant code or docs change (AD-015)
- **AI workflow:** Claude Cowork = architecture, decisions, creativity, memory file updates, repo scanning. Cursor = actual code writing and terminal commands. Both have access to the same local repo folder.

---

## 16. HOW TO START THE NEXT SESSION

**In Cowork (preferred):** The local repo folder is already mounted. Simply open Cowork and say:
*"Read the StudyBuddy memory file and let's continue from the next unchecked item."*
Cowork will read `docs/STUDYBUDDY-MEMORY.md` directly from the local folder — no copy-paste needed.

**In regular Claude.ai chat:** Paste the contents of this file with the message:
*"Let's continue building StudyBuddy. Here is the project memory file. Pick up from the next unchecked item."*

**Remember to sync:** If Cowork updated this local file in the previous session, copy-paste the updated content into the cloud project knowledge to keep both versions in sync.

### Current state as of 29 July 2026

**The app is complete and in daily use for its primary purpose.** All three modes (Explain, Quiz, Summarise) are built and functionally verified end-to-end. The React frontend works, one shared study material field feeds all three modes, question/topic inputs are optional, and read-aloud works in-browser. Start it with `studybuddy`. Nothing blocks use.

**Additionally built 29–30 July (AD-026, AD-027, AD-029):** the Developer Dashboard at `/dev` — live telemetry (4s polling, in-memory, zero cost, eval traffic separated from tutoring traffic) plus a complete on-demand evaluation layer: LLM-as-judge scoring across 11 test cases, per-case results with evaluator reasoning, durable history on disk, and an interactive HTML report via the `aieval` CLI. This covers Phases 5 and 6.

### START HERE NEXT SESSION — the teaching pass (all building is done)

**The eval layer is complete and verified** (AD-026, AD-027, AD-028 → AD-029). Reports generate, per-case reasoning is captured, history persists. Nothing is broken and nothing needs building.

**Two small cleanups first (2 minutes):**
- Add `report.html` to `.gitignore` — 854KB generated artifact currently untracked at repo root
- Fix the doc comment in `DiskEvalReportWriter.cs`: `dotnet aieval report` → `aieval report`

**Then the main event — the teaching pass Royson has been waiting for.**
He deferred this twice to finish building; it is now the actual priority. Concrete starting point: run `aieval report --path eval-reports --output report.html --open`, open it together, and investigate **why Fluency scored exactly 4.00 across all three modes** using the per-case reasoning. That single question naturally covers what a metric is, how LLM-as-judge works, why scores cluster, and how to distinguish a real quality signal from an evaluator artifact — and produces the Phase 5 deliverable as a by-product.

**The dashboard is now correct and complete** (AD-026 built, AD-027 defects fixed). Nothing is broken.

What Royson actually asked for: to **work through traceability and evaluation step by step and understand how they work**, with a fresh start. He is new to these concepts, so the goal is comprehension, not more building. Good starting points:
- Walk through what the `/dev` dashboard is actually showing him, field by field — what a token is, why input and output cost differently, what latency includes, why polling is free
- Explain what each evaluator measures in plain language: Groundedness (did it stick to the source material?), Relevance, Completeness, Fluency, Coherence, Truthfulness
- Explain LLM-as-judge — that scoring is itself an LLM call, which is why eval runs cost real money and are button-gated
- Then run the evals together and interpret the actual scores rather than just reading numbers

**Note:** he has previously said this material goes over his head when explained too densely. Keep it concrete, one idea at a time, and check in rather than delivering long explanations.

### Then, in rough priority order:
1. **Document eval failure modes** (Phase 5 remaining item) — pick a metric, describe the test set, name a specific failure it caught. This is the single most career-relevant deliverable for the AI-evals roles Royson is targeting, and follows naturally from the teaching pass above.
2. **SK Planner** (Phase 2) — route by intent across the three plugins instead of clicking a tab. The remaining Semantic Kernel learning milestone.
3. **Secrets boundary** (AD-014) — move `OPENROUTER_API_KEY` to `dotnet user-secrets` or a shell env var.
4. **In-app TTS quality** (AD-022) — Kokoro/transformers.js local neural TTS, if the macOS Speak-selection workaround stops being good enough.
5. **Docker Compose** (AD-023) — when deploying, or if PostgreSQL is ever wired up.

Claude will read this file and pick up with no context loss.
