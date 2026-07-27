# STUDYBUDDY — Project Memory & Architecture Plan
> This file is the context anchor for all future sessions on this project.
> Start every new chat by sharing this file so no context is lost.
> Last updated: July 2026

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

### Mode 1 — Explain
- User pastes a concept, topic, or question from the course
- Claude reads the loaded study material and explains it in plain tutor language
- Explanation appears as text on screen
- ElevenLabs reads the explanation aloud with natural intonation and emphasis
- Goal: Make Claude explain things the way a real tutor would, not like documentation

### Mode 2 — Quiz
- User requests a quiz on a topic or section
- Claude generates questions from the loaded study material
- User types their answers
- Claude evaluates each answer and explains what was right or wrong
- ElevenLabs reads the feedback aloud
- Goal: Active recall — the fastest way to retain information

### Mode 3 — Summarise
- User pastes a full section of study material
- Claude condenses it into key points and takeaways
- Summary appears as text on screen
- ElevenLabs reads the summary aloud
- Goal: Digest large sections quickly without losing the important details

---

## 3. TECH STACK

| Layer | Technology | Purpose |
|---|---|---|
| Backend | .NET 10 / C# | API, orchestration |
| Frontend | React 19 + TypeScript + Vite | UI (familiar from NOSYOR.M.I) |
| AI Orchestration | Semantic Kernel (SK) | Replaces LangChain + LangGraph |
| AI Model | Claude via Anthropic SDK or OpenRouter | All explanations, quizzes, summaries |
| Text-to-Speech | ElevenLabs API (free tier to start) | Reads Claude's responses aloud |
| Monitoring | SK Telemetry | Replaces LangSmith — logs every AI call |
| Database | SQLite (simple, no Docker needed) | Store loaded study material and session history |
| Containerisation | Docker Compose | Keep it simple — no Minikube for this project |

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

### What happens when the user sends a message:

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
Calls Claude via Anthropic SDK
↓
SK Telemetry logs the full call automatically
(exact prompt sent, exact response, tokens, cost)
↓
Claude's text response returns to .NET API
↓
.NET API sends text to ElevenLabs TTS API
↓
ElevenLabs returns audio
↓
React displays text + plays audio simultaneously
```

### The three SK Plugins:

**ExplainPlugin**
- Takes: user question + loaded study material
- Prompt instruction: "You are a tutor explaining this concept in plain, conversational language with emphasis and clarity. Not documentation — a real explanation."
- Returns: explanation text

**QuizPlugin**
- Takes: topic or section + loaded study material
- Prompt instruction: "Generate 3 questions on this topic. After the user answers, evaluate each answer and explain what was right or wrong."
- Returns: questions first, then evaluation after answers

**SummarisePlugin**
- Takes: pasted section of study material
- Prompt instruction: "Summarise this into the 5 most important points a student needs to remember. Be concise but complete."
- Returns: bulleted key points

---

## 6. THE TTS LAYER — ELEVENLABS

- ElevenLabs has a free tier — sufficient for personal study use
- Every text response from Claude is passed to ElevenLabs after generation
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

---

## 7. HOW STUDY MATERIAL GETS INTO THE APP

The app is source-agnostic — it never connects to any external platform directly. All content arrives as plain text through one of these methods:

**Method 1 — Copy-paste (start here):** User copies any text from any source — a course, an article, a transcript, their own notes — and pastes it into the app's study material input area. Simplest approach, works for everything.

**Method 2 — File upload (Phase 4 enhancement):** User saves content as a .txt or .md file and uploads it. App reads and loads the content automatically.

**Method 3 — URL fetch (future consideration):** User pastes a public URL and the app fetches the readable text from that page. Only works for public pages with no login required.

**Start with Method 1.** Methods 2 and 3 are enhancements — don't build them until Method 1 is working cleanly.

**What the app will never do:** Log into any platform on the user's behalf, scrape authenticated pages, or try to access content behind a paywall. The user is always the bridge between the source and the app.

---

## 8. WHAT THIS PROJECT TEACHES (LEARNING GOALS)

By building this app, Royson will directly experience:

1. **SK Plugin definition** — writing a named, reusable AI task in C#
2. **SK Planner routing** — watching the planner read intent and pick the correct plugin — the same concept as LangGraph nodes
3. **Prompt Templates** — defining reusable prompts with variable slots instead of manually building strings
4. **SK Telemetry** — seeing every Claude call logged automatically — the monitoring layer that was missing in NOSYOR.M.I
5. **Multi-service orchestration** — .NET API coordinating Claude + ElevenLabs — the same orchestrator pattern from NOSYOR.M.I
6. **Claude API behaviour** — directly relevant to the certification — seeing how Claude responds to different prompt structures in real time

---

## 9. BUILD SEQUENCE (WHAT TO BUILD IN WHAT ORDER)

**Phase 1 — Core (build this first, get it working)**
- .NET API scaffold
- React frontend with simple text input
- SK setup with one plugin — ExplainPlugin only
- Claude connected via Anthropic SDK or OpenRouter
- SK Telemetry connected and logging
- Verify: type a question, Claude explains it, see the log in telemetry

**Phase 2 — All three modes**
- Add QuizPlugin
- Add SummarisePlugin
- Add SK Planner to route between all three based on user intent
- Verify: the planner correctly identifies which mode the user wants

**Phase 3 — TTS**
- Connect ElevenLabs API
- Every Claude response is passed to ElevenLabs after generation
- React plays audio while text is visible
- Verify: full tutor experience — text on screen, voice reading it aloud

**Phase 4 — Polish**
- Study material loading (file upload or persistent paste area)
- Session history (SQLite — remember what you've studied)
- UI polish — keep it clean and simple

---

## 10. PROJECT IDENTITY

**Name:** StudyBuddy
**Purpose:** Personal AI tutor for Claude certification study
**Stack:** .NET 10 + React 19 + Semantic Kernel + Claude + ElevenLabs
**Repo:** To be created — separate from NOSYOR.M.I entirely
**Design:** Keep it simple and functional — this is a learning tool, not a portfolio showpiece

---

## 11. OPEN QUESTIONS (TO DECIDE IN NEXT SESSION)

- [ ] Anthropic SDK direct vs OpenRouter for calling Claude — which to use?
- [ ] ElevenLabs voice selection — which voice fits a tutor persona?
- [ ] SQLite schema — what needs to be persisted?
- [ ] GitHub repo name
- [ ] Project board setup on GitHub

---

## 12. KEY CONTEXT — WHO IS BUILDING THIS

- **Builder:** Royson D'Souza
- **Background:** MBA graduate, no prior coding background, learning through building
- **Existing project:** NOSYOR.M.I — full-stack AI personal finance app (.NET 10 + React 19 + pgvector + Docker)
- **Tools:** Cursor IDE (Agent mode), Claude Code extension in Cursor, GitHub Desktop
- **Working style:** Architecture first, then Cursor builds the code
- **Standing rule:** Every Cursor prompt must include "Follow SOLID principles and clean coding structure throughout"
- **AI in chat vs AI in Cursor:** claude.ai handles architecture and learning decisions. Claude Code in Cursor handles file edits and terminal commands.

---

## 13. HOW TO START THE NEXT SESSION

Paste this file into a new chat with the message:

*"Let's continue building StudyBuddy. Here is the project memory file with everything we decided."*

Claude will read it and pick up exactly where we left off with no context loss.
