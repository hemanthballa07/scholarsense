# CLAUDE.md — ScholarSense project context

This file is read automatically by Claude Code at the start of every session in this repo. It tells Claude what this project is, the locked decisions, the conventions, and what *not* to suggest.

## What this project is

ScholarSense is an eligibility-aware scholarship search and recommendation system. The core hard problem is hybrid retrieval (vector + BM25 + structured filters) composed with LLM eligibility verification and explainable ranking. Built independently on public/synthetic data. Portfolio project targeting Microsoft new-grad SWE roles.

Authoritative docs in this repo:
- `DESIGN.md` — architecture, retrieval pipeline, data model, API surface, failure handling.
- `EXECUTION.md` — week-by-week build plan, build order, cut rules, weekly rituals.
- `DATA_PROVENANCE.md` — per-record source tracking.
- `PROGRESS.md` — live ledger of decisions and scope changes; Snapshot (overwritten) + append-only Log, updated *before* each change lands.

Always consult `DESIGN.md` before proposing architecture changes. Always consult `EXECUTION.md` before suggesting new work.

## Locked decisions (do not re-litigate)

- Backend: .NET 9 minimal API in C#
- Frontend: React + TypeScript
- Search: Azure AI Search (Basic tier), hybrid mode with RRF + semantic re-ranking
- AI: Azure OpenAI — `text-embedding-3-small` for embeddings, `gpt-4o-mini` for verification. No `gpt-4o`.
- Database: Cosmos DB (NoSQL API, serverless)
- Storage: Azure Blob
- Deployment: Azure Container Apps (backend), Azure Static Web Apps (frontend)
- Observability: Application Insights via OpenTelemetry exporter
- V1 verifier candidates: top 5, parallel fan-out, JSON mode, 3s timeout
- No auth in V1. Demo-data banner displayed in the V1 UI (search page is the only V1 page; profile-creation UI is out of V1 — hardcoded `profileId` per Week 4).
- Essay generation is out. Permanently.
- No labeled eval metrics (recall@k, MRR, nDCG) in V1.

If a suggestion would change any of the above, surface it as a tradeoff question first; do not silently introduce it in code.

## Stack conventions

- C# 12 / .NET 9. Minimal API style, not controllers, unless an endpoint genuinely benefits from a controller.
- `FluentValidation` for input validation.
- `Polly` for retries and timeouts. No circuit breakers in V1 — see `DESIGN.md` §Failure handling for the reasoning; breaker is in *Out of V1* and reconsidered for V2 if traffic warrants it.
- Structured logging via `ILogger`; correlation IDs via `Activity.Current`.
- Async all the way. `Task.WhenAll` for parallel fan-out. `SemaphoreSlim` for bounded concurrency.
- Configuration via `IOptions<T>` pattern; no magic strings for keys.
- DTOs are records, not classes.

## Folder structure

```
scholarsense/
├── src/
│   ├── ScholarSense.Api/           # minimal API host
│   ├── ScholarSense.Core/          # domain models, interfaces
│   ├── ScholarSense.Search/        # AI Search client + hybrid query builder
│   ├── ScholarSense.Verifier/      # LLM verifier with JSON validation
│   └── ScholarSense.Ingestion/     # console app: synthetic + real ingestion
├── tests/
│   ├── ScholarSense.Api.Tests/     # WebApplicationFactory integration tests
│   └── ScholarSense.Verifier.Tests/
├── frontend/                       # React + TS (Vite)
├── infra/                          # bicep or terraform (decide Week 5)
├── data/
│   ├── synthetic/                  # generated fixtures, JSONL
│   └── curated/                    # hand-curated public records
├── docs/                           # diagrams, design notes
├── DESIGN.md
├── EXECUTION.md
├── DATA_PROVENANCE.md
├── PROGRESS.md
└── CLAUDE.md                       # this file
```

## Coding rules for Claude Code

- Read `DESIGN.md` before making changes that touch architecture or data flow.
- Read `EXECUTION.md` before adding features. If the feature isn't in the current week's deliverables, surface that and ask whether to defer.
- Never add a new external dependency without flagging the tradeoff.
- Never add `gpt-4o` calls; verifier uses `gpt-4o-mini` only.
- Never expand verification beyond top 5 in V1.
- Never commit secrets. Use User Secrets locally (`dotnet user-secrets`) and Key Vault in deployed environments.
- Tests for any code touching retrieval scoring or the verifier are non-negotiable.
- All telemetry events follow the shape `{ eventName, profileId?, sessionId?, properties{}, measurements{} }`.

## What's out of scope (do not suggest)

Essay generation. User auth. Saved searches. Admin UI. Multi-region. Fine-tuned embeddings. recall@k / MRR / nDCG eval pipelines. A second App Insights workbook. AI Search outage fallback. Real scraping with respect-robots.txt orchestration (we're synthetic-first).

## Sub-agents available

- `design-critic` — replays the GPT-style stress-test against the current plan. Use when scope is drifting.
- `code-reviewer` — Microsoft engineering-bar review of a diff or file.
- `interview-coach` — quizzes on shipped design decisions.

Slash commands available:
- `/critique-plan` — runs `design-critic` on the current state of `DESIGN.md` + `EXECUTION.md` + `PROGRESS.md`.
- `/week-checkpoint` — compares Git activity to the current week's deliverables in `EXECUTION.md`.
- `/interview-quiz` — five questions from `interview-coach` on this week's shipped work.

## Tone for Claude

Direct. Opinionated. Push back when I'm scope-creeping. Don't apologize for refusing to add things that violate locked decisions. Surface tradeoffs explicitly.
