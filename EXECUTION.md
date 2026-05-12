# ScholarSense — Execution Plan

60 hours over 6 weeks at ~10 hr/week. **4 weeks for working end-to-end demo, 2 weeks for polish/buffer.**

## Governing rule

> If hybrid search + LLM eligibility verification + explainable results are not working by end of Week 4, **stop all polish work**. No deployment, no Key Vault, no workbook, no demo video until the core works locally.

The core is non-negotiable. The release polish is sacrificeable.

## Build order

1. Local .NET backend with hardcoded config
2. Azure AI Search index + ingestion (the only cloud dependency that's hard to fake locally)
3. Top-5 LLM verifier with JSON validation + timeout (no circuit breaker yet)
4. React UI talking to local backend
5. App Insights instrumentation (early; the workbook view is late)
6. Deploy to Container Apps + Static Web Apps
7. Managed identity, Key Vault, Polly circuit breaker
8. README, architecture diagram, demo video

## Phase 0 — .NET warm-up (before Week 1, ~2 hours)

Microsoft Learn: "Build a web API with ASP.NET Core minimal APIs." Build `/health` locally. No Cosmos, no AI Search. Just confirm the language and tooling click.

## Week 1 — Backend skeleton

**Goal.** Local .NET API runs. You can `curl localhost`.

**Deliverables.**
- .NET 9 minimal API project
- `/api/v1/health` returns 200
- One sample endpoint returning a hardcoded scholarship JSON
- Domain models: `Profile`, `Scholarship`, `EligibilityVerdict`
- `ILogger` structured logging wired
- Repo structure: `src/`, `tests/`, `infra/`, `data/synthetic/`, `docs/`
- Azure subscription confirmed; one empty resource group created

**Cut rule.** If you can't get the API running locally by end of week, postpone Cosmos/AI Search setup to Week 3 and use in-memory storage in Week 2.

**Definition of done.** Push to GitHub. `curl localhost:5000/api/v1/scholarships/sample` returns valid JSON.

## Week 2 — Data + search foundation

**Goal.** ~250 scholarships are indexed and searchable.

**Deliverables.**
- Synthetic scholarship generator (C# console app) → ~200 records to `data/synthetic/`
- ~50 hand-curated real scholarships from public sources, with `DATA_PROVENANCE.md`
- Cosmos DB provisioned; ingestion writes records there
- Azure AI Search index provisioned via code (not the portal)
- Embeddings generated via `text-embedding-3-small`, written to index
- Basic search endpoint returning hybrid (BM25 + vector) ranked results — no profile-aware filtering yet

**Cut rule.** Skip CareerOneStop if access requires more than 30 minutes of setup. Skip the hand-curated 50 records if synthetic-only gets you to Week 3 on time.

**Definition of done.** `curl localhost:5000/api/v1/search?q=computer+science` returns ranked results.

## Week 3 — Profile + structured filtering

**Goal.** Profile-aware ranked search works.

**Deliverables.**
- `POST /api/v1/profiles`, `GET /api/v1/profiles/{id}` working against Cosmos
- Structured filters: GPA, country, state, degree level, major, deadline
- `POST /api/v1/search` accepts `profileId` and applies filters as part of the AI Search query
- Integration tests for the search endpoint using `WebApplicationFactory`

**Cut rule.** If filter logic balloons, drop major/demographic_tags from V1 filters; keep deadline + country + state + GPA.

**Definition of done.** Create a profile, search with profileId, see filtered ranked results.

## Week 4 — LLM verifier + UI + end-to-end demo

**Goal.** Local end-to-end demo works. This is the project's true checkpoint.

**Deliverables.**
- Top-5 LLM verification with JSON mode, per-call 3s timeout, `SemaphoreSlim(5)` bounding concurrency
- JSON validation + retry-on-parse-failure
- Timeout → UNCLEAR fallback
- Score fusion + drop INELIGIBLE-high-confidence
- React/TS frontend: profile create form, search input, results list with verdict badges and matched/failed criteria, demo-data banner
- Local end-to-end demo: create profile → search → see explainable ranked results

**Cut rule.** If you reach Week 4's end without this working, **pause all polish**. Spend Week 5 finishing Week 4 work. Drop deployment and demo video if necessary.

**Definition of done.** Screen-record yourself doing the demo flow locally. If the recording is convincing, you're done with the core.

## Week 5 — Deployment + telemetry

**Goal.** Demo runs from a public URL.

**Deliverables.**
- Backend deployed to Azure Container Apps via GitHub Actions
- Frontend deployed to Azure Static Web Apps
- Application Insights connected (OpenTelemetry exporter)
- Custom metrics: per-stage latency, fallback counter, verdict distribution, token usage
- Health probes wired (Container Apps liveness/readiness)
- Smoke-test the public URL end-to-end

**Cut rule.** If deployment fights you, ship with connection strings in app settings (defer managed identity to Week 6). Honesty in the README about what's left.

**Definition of done.** A public URL serves the demo. Telemetry events appear in Application Insights.

## Week 6 — Reliability + polish

**Goal.** Portfolio-ready.

**Deliverables.**
- Managed identity + Key Vault (move secrets out of app settings)
- Polly circuit breaker on the LLM verifier
- One App Insights workbook (latency, fallback rate, verdict distribution, token usage)
- `README.md` — what it is, how to run locally, architecture diagram, demo link, honest "what's left for V2"
- Architecture diagram (Excalidraw or draw.io, exported PNG)
- 3-minute demo video (Loom or QuickTime)
- Final code-review pass

**Cut rule.** Cut bottom-up: video → workbook → circuit breaker → managed identity. Keep README and architecture diagram always.

**Should-have if everything else is done.** Feedback endpoint + thumbs UI, embedding cache, filter-relaxation fallback, k6 load test (50–100 sequential requests for a real latency number), profile-only recommendations endpoint.

## Resume bullets (final, post-V1)

1. Built ScholarSense, an eligibility-aware scholarship search system on Azure (.NET 9, React/TS, Cosmos DB, Azure AI Search, Azure OpenAI), composing hybrid vector + keyword retrieval with structured filters and parallel LLM verification to produce explainable, ranked recommendations.
2. Instrumented end-to-end observability via OpenTelemetry + Application Insights with custom metrics for per-stage latency, fallback rate, verdict distribution, and LLM token usage, deliberately deferring labeled eval metrics until user-feedback data was available.
3. Built timeout-bounded LLM eligibility verification with structured JSON output and Polly circuit-breaker fallback, keeping search results available with degraded explanations when Azure OpenAI was rate-limited or unavailable.

Add a fourth bullet only after a real load test:
> Measured p95 search latency of X ms across N requests against ~250 indexed scholarships, with top-5 LLM verification fanning out in parallel.

## Weekly ritual

- **Monday (5 min).** Run `/week-checkpoint` slash command in Claude Code. Sub-agent compares your Git activity against the week's deliverables and tells you what's at risk.
- **Mid-week.** Open PRs for review by the `code-reviewer` sub-agent before merging.
- **Friday (10 min).** Run `/interview-quiz`. Five questions on the design decisions you actually shipped that week. Builds your interview defense incrementally.
- **End of week.** Update `PROGRESS.md` with what shipped and what slipped.

## When to stop iterating on the plan

You're done iterating on the plan. The next time you should change DESIGN.md is when reality (load test results, ingestion difficulty, LLM behavior) forces a change. Not on the basis of more LLM critique rounds.
