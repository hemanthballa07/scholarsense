# ScholarSense — Design

Eligibility-aware scholarship search. Hybrid retrieval (vector + BM25 + structured filters) composed with LLM eligibility verification and explainable ranking. Built independently on public/synthetic data, Azure-native stack, .NET 9 backend, React/TS frontend.

## Architecture

**Frontend** — React/TS on Azure Static Web Apps.
**Backend** — .NET 9 minimal API in a container on Azure Container Apps.
**Search** — Azure AI Search (Basic tier) holds the hybrid index: vector field + BM25 fields + structured filterable fields.
**AI** — Azure OpenAI: `text-embedding-3-small` for embeddings, `gpt-4o-mini` for eligibility verification. No `gpt-4o` in V1.
**System of record** — Cosmos DB (NoSQL API, serverless). Profiles, scholarships, search sessions, optional feedback.
**Raw artifacts** — Azure Blob Storage. Source HTML/JSON, synthetic-data fixtures, ingestion run logs.
**Observability** — Application Insights via OpenTelemetry exporter.
**Secrets** — Azure Key Vault, accessed via managed identity. No connection strings in app settings.

### Request flow: search

1. Client `POST /api/v1/search` with `{ profileId, query, limit }`.
2. Backend fires two parallel calls: Cosmos point-read for profile, Azure OpenAI for query embedding.
3. Single hybrid query to AI Search: BM25 + vector kNN + hard structured filters (`deadline > now`, country/state, GPA minimum) all in one round trip. RRF fusion + semantic re-ranking on top 50. Returns top 20.
4. **Top 5** candidates fan out in parallel to `gpt-4o-mini` eligibility verifier. JSON mode, per-call timeout 3s, bounded by `SemaphoreSlim`. Returns `{verdict, confidence, matched_criteria, failed_criteria, unclear_criteria}` per candidate.
5. Score fusion: `0.4 * search_score + 0.5 * eligibility_score + 0.1 * freshness_score`. INELIGIBLE-high-confidence is dropped, UNCLEAR survives with a flag.
6. Response includes per-stage timings and any `fallbacksTriggered`.

### Request flow: profile

1. `POST /api/v1/profiles` with validated profile JSON.
2. Server-generated GUID `profileId`. Write to Cosmos with partition key `/profileId`.
3. (Optional in V1) Compute and cache a profile embedding from free-text fields.

## Data model

**Cosmos containers and partition keys.** All hot-path reads are point reads by ID, which is exactly Cosmos's sweet spot.

| Container | Partition key | Purpose |
|---|---|---|
| `profiles` | `/profileId` | Student profiles |
| `scholarships` | `/scholarshipId` | Source of truth for scholarship records |
| `searchSessions` | `/profileId` | Query log; append-only |
| `feedback` (optional V1) | `/profileId` | Thumbs ratings |

**Why Cosmos over Postgres.** Every hot path is a point read by partition key. Eligibility data is schema-flexible per scholarship (variable demographic tags, eligibility criteria, custom rules). Postgres with JSONB would also work, but Cosmos is the native fit and the Azure-native choice is a coherent story for Microsoft interviews. Reporting/analytics work goes through App Insights, not the OLTP store, so relational features are unused.

**Azure AI Search index fields.**

- `id` (key)
- `title`, `description`, `eligibility_text` (searchable, en.lucene analyzer)
- `description_vector` (Collection<Single>, dim=1536, HNSW)
- `sponsor`, `amount_usd`, `deadline`, `min_gpa` (filterable/sortable)
- `eligible_countries`, `eligible_states`, `eligible_majors`, `demographic_tags` (filterable Collection<string>)
- `source_url`

One combined embedding over `title + description + eligibility_text`. Semantic configuration enabled for L2 re-ranking. Single embedding keeps ingestion and storage simple; multi-field embeddings are deferred to V2.

**Blob layout.** `scholarsense/{raw-sources, synthetic, ingestion-artifacts}/...`. Lifecycle to Cool tier after 30 days for raw sources.

## Retrieval pipeline

**Stage 1 — Query understanding (cheap).** No LLM query rewriting. Optional regex pass extracts obvious filter intent ("Florida", "graduate") as additional filters. Eligibility lives in the profile, not the query.

**Stage 2 — Hybrid search with hard prefilter (1 AI Search call).** BM25 + vector kNN + structured filter clause derived from profile. *Hypothesis: hard filters on hard facts (deadline, country, state, GPA minimum) cut the candidate set 50–90% on our corpus before any LLM cost. Validated in Week 2 against the 250-scholarship corpus across 10 representative profiles. If measured selectivity falls outside 40–95%, retract the range and replace with the measured median + IQR.* Returns top 20.

**Stage 3 — LLM verification (parallel, top 5).** Tight system prompt, strict JSON output. Per-call 3s timeout. Parallel via `Task.WhenAll` with `SemaphoreSlim(5)`. On JSON parse failure, one retry at temperature=0; second failure → UNCLEAR + log `LLMParseFailure`. Top 5, not 10 — lower cost, fewer rate-limit issues, easier debugging. `MaxVerificationCandidates` is a config value, not a constant.

**Stage 4 — Score fusion and explanations.** `eligibility_score` mapping: ELIGIBLE-high = 1.0, ELIGIBLE-mid = 0.7, UNCLEAR = 0.4, INELIGIBLE = drop. `freshness_score` is a sigmoid peaking ~30 days before deadline. Explanations come directly from `matched_criteria` / `failed_criteria` arrays — no second LLM call for explanation generation.

## API surface

| Method | Path | Purpose |
|---|---|---|
| `POST` | `/api/v1/profiles` | Create profile, returns `{profileId, ...}` |
| `GET`  | `/api/v1/profiles/{profileId}` | Fetch profile |
| `POST` | `/api/v1/search` | Profile-aware ranked search with explanations |
| `GET`  | `/api/v1/scholarships/{id}` | Full scholarship record |
| `GET`  | `/api/v1/health` | Liveness + shallow dependency probe |

Optional (should-have): `PATCH /api/v1/profiles/{id}`, `GET /api/v1/recommendations/{profileId}`, `POST /api/v1/feedback`. Errors use RFC 7807 Problem Details.

Search response shape (abridged):

```json
{
  "results": [{
    "scholarshipId": "...", "title": "...", "amount": 15000, "deadline": "...",
    "fitScore": 0.87, "sourceUrl": "...",
    "eligibility": {
      "verdict": "ELIGIBLE",
      "confidence": 0.92,
      "matchedCriteria": ["graduate student", "US resident"],
      "failedCriteria": [],
      "unclearCriteria": [],
      "reason": null
    },
    "matchReason": "..."
  }],
  "stageTimings": { "embedMs": 0, "searchMs": 0, "verifyMs": 0, "totalMs": 0 },
  "fallbacksTriggered": []
}
```

`eligibility.reason` is `null` for ELIGIBLE and INELIGIBLE verdicts; on UNCLEAR it carries one of `timeout`, `rate_limit_429`, `transport_error`, or `parse_failure` (set by the verifier per §Failure handling).

No auth in V1. UI displays a "Demo data only — do not enter real personal information" banner on the search page (the only V1 page; profile-creation UI is out of V1, see EXECUTION.md Week 4).

## Failure handling

**Azure OpenAI rate-limit / timeout.**
- Embeddings: Polly retry (3 attempts, exponential backoff with jitter, 2s budget) + 5-min LRU cache. Total failure → degrade to BM25-only, tag response with `fallbacksTriggered: ["embedding_unavailable"]`.
- Verifier: per-call 3s timeout, enforced by both `HttpClient.Timeout` and a `CancellationToken` on the SDK call (the dual enforcement guards against TCP-level hangs that exceed the SDK's own timeout during regional incidents). Failure → candidate marked UNCLEAR with `reason ∈ {timeout, rate_limit_429, transport_error, parse_failure}`. Content-filter rejections and other "200 with unusable body" responses fall through the Stage 3 JSON-parse retry path and end as UNCLEAR with `reason: "parse_failure"`. No circuit breaker in V1 — see *Why no circuit breaker in V1* below.

**Empty results.** Re-issue with relaxed filters (drop `eligible_majors` first, then `demographic_tags`). Tag response.

**Unparseable eligibility text** is an ingestion-time problem. Try deterministic parser → LLM extractor → ingest with `eligibility_status: "unstructured"`. Query-time verifier handles unstructured records as free text.

**Cosmos down.** In-memory profile cache (5-min TTL) serves reads. Writes return 503 with Retry-After. No in-memory write queue — data loss risk.

**AI Search down.** Out of V1 scope. V2 fallback would be Cosmos direct query with structured filters.

**Why no circuit breaker in V1.** A circuit breaker on the verifier would amortize wasted LLM calls when Azure OpenAI is persistently degraded — skipping ~5 calls per request times request rate times outage duration. At V1's scale (portfolio demo, <1 RPS, single-region), the saved calls are negligible against daily token budget, and per-call 3s timeout already bounds user-visible latency on each request. The breaker's three preconditions (unreliable downstream, calling-worsens-it, meaningful fallback) all hold *in principle* — but the *economic* precondition (sustained traffic that makes amortization matter) does not at this scale. Per-call timeout-to-UNCLEAR is the V1 failure-handling path. Reconsider in V2 if sustained RPS rises or token costs become a binding constraint.

Other dependencies — Cosmos, AI Search, Embeddings — already have appropriate handling: Cosmos profile cache on read, embedding-failure → BM25-only, and Polly retries on embeddings (all documented above). Circuit breakers there would be cargo-culted complexity.

## Observability

Wired from day one as an OpenTelemetry exporter to Application Insights. Custom metrics include per-stage latency (`embed`, `search`, `verify`, `total`), fallback counters by reason, eligibility verdict distribution, parse-failure rate, OpenAI token usage and estimated cost, Cosmos and AI Search RU consumption.

**Not in V1**: recall@k, MRR, nDCG. No labeled eval set exists; faking labels would be self-deceptive. Build the eval set later from user-feedback data.

One App Insights workbook in V1 covering latency, error rate, fallback rate, verdict distribution, token usage, dependency health.

## Out of V1 (explicitly)

Essay generation. User auth. Saved searches and deadline alerts. Admin UI. A/B testing. Multi-region. Recall@k / MRR / nDCG. Fine-tuned embeddings. Top-10 verification with sophisticated concurrency. AI Search outage fallback. Second App Insights workbook. Real scraping pipeline (synthetic-first). Polly circuit breaker on the verifier (deferred — see *Failure handling* for the V1 reasoning).

## Data sourcing

~250 scholarships total: ~50 hand-curated from public sources (Federal Student Aid, one state program like Florida Bright Futures, one university public scholarship page) + ~200 synthetic generated by `gpt-4o-mini` against a structured template with provenance tagging. CareerOneStop API only if access is smooth within 30 minutes of investigation; otherwise skip.

**IP safety.** No code, data, schema, or prompts from prior employer. Different stack. Different problem framing. `DATA_PROVENANCE.md` in the repo tracks source per record. Project README explicitly states independent build with public/synthetic data only.

## Interview defense (one-liners)

- **Cosmos over Postgres** — every hot path is a point read by partition key; that's Cosmos's wheelhouse. JSONB in Postgres would also work; the Azure-native choice is a coherent story.
- **Container Apps over App Service or AKS** — Kubernetes-style scale without operating a cluster; right point on the simplicity/power curve for one backend.
- **Top-5 verification, parallel** — bounded fan-out isolates per-candidate failure; wall-clock is bounded by the slowest call. Config value, not a constant.
- **Hard prefilter + LLM judgment** — hard filters on hard facts (deadline, GPA, geography); LLM on the judgment calls only. Don't pay GPT tokens to reject "deadline last year."
- **No latency claim in resume** — measured before claimed. Real numbers from real load tests or none.
- **No circuit breaker in V1** — three preconditions for a breaker (unreliable downstream, calling-worsens-it, meaningful fallback) hold for the verifier *in principle*, but the economic precondition — sustained traffic that justifies amortization — does not at portfolio scale. Per-call 3s timeout-to-UNCLEAR is the V1 path; reconsidered for V2 if traffic or token cost warrants it. Not cargo-culted anywhere.
- **No recall@k in V1** — no labeled eval set means honest metrics only. Feedback data builds the eval set later.
