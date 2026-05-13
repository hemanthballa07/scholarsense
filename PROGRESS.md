# PROGRESS.md — ScholarSense live ledger

This is the live ledger for ScholarSense. Updated **before** any implementation, decision, or scope change. A new Claude Code session in this repo should read this file immediately after CLAUDE.md to know where the project is right now and what's in flight.

---

## Snapshot

*Overwritten each update. Last updated: 2026-05-12.*

**Current state:** Plan-only. `DESIGN.md`, `EXECUTION.md`, `CLAUDE.md`, `README.md`, `PROGRESS.md` are in place. No `src/`, no `tests/`, no `data/`, no `infra/`. The plan has been stress-tested across three passes: (1) `/critique-plan` resolution — Q1 (prefilter hypothesis with Week 2 validation), Q2 (Week 4 UI scope reduced — hardcoded `profileId`, profile-creation form deferred; Week 3 integration tests cover profiles + search), Q3 (Polly circuit breaker removed from V1, per-call timeout-to-UNCLEAR with `reason` codes is V1, breaker deferred to V2). (2) Self-review cleanup — profile-endpoint test coverage, response-shape `eligibility.reason` field, "Why no circuit breaker" closing sentence, README.md PROGRESS.md template. (3) Parallel-agent audit — orphan references in upstream summary sections (build-order list, weekly ritual, CLAUDE.md description, banner locked decision), grammar artifacts ("sustainedly degraded", nested parens). All findings applied. `.claude/` folder (3 agents + 3 commands) independently verified clean.

**Next concrete step:** Begin Week 1 implementation. Sequence per `EXECUTION.md` Week 1: (a) confirm Phase 0 (.NET warm-up, ~2 hours of Microsoft Learn minimal-API tutorial) is done — Week 1 assumes it; (b) confirm Azure subscription is active and create one empty resource group; (c) `dotnet new` the `ScholarSense.Api` minimal-API project and `ScholarSense.Core` class library; (d) create repo folder structure (`src/`, `tests/`, `infra/`, `data/synthetic/`, `docs/`); (e) wire `ILogger` structured logging; (f) implement `/api/v1/health` and `/api/v1/scholarships/sample`; (g) define domain records `Profile`, `Scholarship`, `EligibilityVerdict` in `ScholarSense.Core`. Definition of done: push to GitHub; `curl localhost:5000/api/v1/scholarships/sample` returns valid JSON.

**Before Week 1 starts:** the session's doc changes are *uncommitted* — `git status` will still show modified DESIGN.md/EXECUTION.md/CLAUDE.md/README.md and untracked PROGRESS.md/.claude/. A commit is needed before any code work begins, or a fresh clone won't reflect any of the planning work done here.

**Next concrete step:** Begin Week 1 deliverables per `EXECUTION.md`. In order: (1) create repo folder structure (`src/`, `tests/`, `infra/`, `data/synthetic/`, `docs/`); (2) `dotnet new` the `ScholarSense.Api` minimal-API project and `ScholarSense.Core` class library; (3) wire `ILogger` structured logging; (4) implement `/api/v1/health` returning 200; (5) implement `/api/v1/scholarships/sample` returning a hardcoded `Scholarship` record; (6) define domain records `Profile`, `Scholarship`, `EligibilityVerdict` in `ScholarSense.Core`; (7) confirm Azure subscription + create one empty resource group. Before any of this, complete Phase 0 (.NET warm-up per EXECUTION.md line 22) if not already done. Definition of done: push to GitHub and `curl localhost:5000/api/v1/scholarships/sample` returns valid JSON.

**In-progress work:** None — no code in flight, no half-edited files.

**Open questions:** None — all three critique items resolved.

---

## Log

*Append-only, newest first.*

### 2026-05-12 — Parallel-agent audit: 10 coherence findings; 8 fixes applied; `.claude/` verified clean
- **Type:** doc-update
- **What:** Ran two parallel agents to verify pre-Week-1 readiness — (a) end-to-end doc-coherence audit across DESIGN.md / EXECUTION.md / CLAUDE.md / README.md / PROGRESS.md; (b) `.claude/` folder verification (3 agents + 3 commands). Verification clean: all 6 files exist, valid YAML, substantive bodies, slash commands reference correct agents, no extras. Audit surfaced 10 findings — the cleanup pass missed orphan references in upstream summary sections (build-order list, weekly ritual, CLAUDE.md description, banner location). Eight fixes applied: EXECUTION.md build-order item 7 (drop "Polly circuit breaker"), build-order item 3 (drop "no circuit breaker yet" parenthetical), weekly-ritual "End of week" bullet (rewrite to live-ledger model); CLAUDE.md PROGRESS.md description (live ledger, updated before each change), demo-banner locked-decision (relocate from profile-creation page to V1 search page); DESIGN.md demo-banner sentence (same relocation), "Why no circuit breaker" paragraph (replace "sustainedly" → "persistently"; split nested-parens closing into a separate paragraph). PROGRESS.md Snapshot's self-falsifying "internally consistent" claim is revised. Line-number drift in older Log entries deferred — historical entries stay; new entries prefer section anchors.
- **Why:** A previous Snapshot claimed "stress-tested and internally consistent" while the build-order top-of-EXECUTION.md still listed the breaker as Week-6+ work and CLAUDE.md still framed PROGRESS.md as a weekly recap. Self-falsifying Snapshots are the worst-case failure mode of a live ledger — they erode the doc's trust as a source of truth across sessions. Catching this now (before commit, before Week 1) prevents a future Claude Code session from acting on a contradicted plan. `.claude/` verification was preventative — if any agent or command file were broken, that surfaces mid-Week-4 when `code-reviewer` is needed.
- **Files:** `EXECUTION.md` (build-order items 3 and 7; weekly-ritual End-of-week bullet), `CLAUDE.md` (PROGRESS.md description; demo-banner locked decision), `DESIGN.md` (demo-banner sentence; "Why no circuit breaker" paragraph — two spots), `PROGRESS.md` (this entry + Snapshot), and the persistent memory file `progress-ledger-workflow.md` (added note: prefer section anchors over line numbers).
- **Status:** done
- **Refs:** prior log entries "2026-05-12 — Cleanup pass" and "2026-05-12 — Resolve Q1/Q2/Q3".

### 2026-05-12 — Cleanup pass: tighten doc consistency after Q1–Q3 resolution
- **Type:** doc-update
- **What:** Self-review after Q1–Q3 surfaced four loose ends. Fixes applied: (1) `EXECUTION.md` Week 3 integration-tests bullet expanded to cover `POST /profiles`, `GET /profiles/{id}`, and `POST /search` — so the Q2 defense rationale ("POST /profiles stands on its `WebApplicationFactory` integration tests") is actually supported by the plan; (2) `DESIGN.md` line 116 — closing sentence of "Why no circuit breaker in V1" paragraph rewritten to accurately describe other dependencies' fallback paths (Cosmos profile cache, embedding-failed → BM25-only, Polly retry on embeddings) instead of the inaccurate "intelligent SDK retries" catch-all; (3) `DESIGN.md` response shape — added `eligibility.reason` field (null on ELIGIBLE/INELIGIBLE; one of `timeout|rate_limit_429|transport_error|parse_failure` on UNCLEAR) so the verifier `reason` introduced at DESIGN.md line 106 actually surfaces in the client response; (4) `README.md` "End of week" section — replaced the stale weekly-recap PROGRESS.md template with a pointer to the live-ledger format established earlier this session. Loose end #4 (Stage 3 JSON schema validation for content-filter responses returning valid-JSON-but-wrong-shape) intentionally deferred to Week 4 implementation discovery.
- **Why:** Q1–Q3 resolutions introduced doc inconsistencies that would have surfaced in interview defense or in a future Claude Code session reading the docs. #1 is the most load-bearing — the Q2 rationale referenced integration tests the plan didn't deliver. #2–#4 are smaller consistency fixes; ignoring them would gradually erode confidence in the docs as a source of truth.
- **Files:** `EXECUTION.md` (Week 3 integration-tests bullet), `DESIGN.md` (line 116 closing sentence; response shape JSON + explanatory sentence after the JSON), `README.md` ("End of week" section), `PROGRESS.md` (this entry + Snapshot).
- **Status:** done
- **Refs:** prior log entries "2026-05-12 — Resolve Q1/Q2/Q3".

### 2026-05-12 — Resolve Q3: defer Polly circuit breaker to V2; per-call timeout-to-UNCLEAR is the V1 failure-handling path
- **Type:** decision + doc-update
- **What:** After stress-testing options (a) pin contract + implement Week 6, (b) defer with V2 stub, (c) cut entirely, via the `design-critic` agent, chose option (b) with (c)-flavored framing. Removing the Polly circuit breaker from V1 entirely. `DESIGN.md` Failure handling section rewritten: verifier bullet (line 106) now specifies per-call 3s timeout enforced by both `HttpClient.Timeout` and `CancellationToken` (guards TCP-level hangs during regional incidents), with `reason`-tagged UNCLEAR (timeout, rate-limit-429, transport-error, parse-failure); the old "circuit breaker only on the LLM verifier" paragraph (line 116) replaced with a "Why no circuit breaker in V1" paragraph that states the *why* (economic precondition — sustained traffic that justifies amortization — does not hold at portfolio scale). "Out of V1" list (line 128) gains the breaker. Interview-defense one-liner (line 143) flipped to "No circuit breaker in V1." `EXECUTION.md` Week 6 deliverables + cut rule strip the breaker; resume bullet #3 rewritten to describe per-call timeout-to-UNCLEAR. `CLAUDE.md` Polly stack-conventions line dropped the circuit-breaker reference.
- **Why:** Per-call timeout-to-UNCLEAR genuinely covers V1's failure modes — Azure OpenAI 429s return fast from the gateway (with `Retry-After`); outages present as fast 5xx or connection errors; content-filter and bad-body responses flow through the existing JSON-parse retry path (Stage 3). Breaker amortization (skip ~5 wasted calls per request when service is degraded) is negligible at <1 RPS portfolio scale; pays off around ~10 RPS. A half-implemented breaker is worse than no breaker — interviewer probes ("what failure rate threshold? what window? did you load-test it tripping?") have no answer without a real load test. Locking the decision into DESIGN.md *and* removing from Week 6 deliverables prevents re-litigating under time pressure five weeks from now. Interview defense becomes: *"Considered the breaker; preconditions hold in principle (unreliable downstream, calling-worsens-it, meaningful fallback) but the economic precondition — sustained traffic justifying amortization — doesn't hold at demo scale; per-call timeout suffices; would reconsider at production traffic."* Stronger senior-engineer signal than implementing every resilience pattern reflexively.
- **Files:** `DESIGN.md` (lines 106, 116, 128, 143), `EXECUTION.md` (Week 6 deliverables + cut rule, resume bullet #3), `CLAUDE.md` (Polly stack-conventions line), `PROGRESS.md` (this entry + Snapshot).
- **Status:** done
- **Refs:** prior log entry "2026-05-12 — /critique-plan stress-test (design-critic)".

### 2026-05-12 — Resolve Q2: reduce Week 4 UI scope upfront (hardcoded profileId is the plan)
- **Type:** decision + doc-update
- **What:** After stress-testing the original "split Week 4" proposal (option α) against four alternatives via the `design-critic` agent, switched recommendation to option β: make hardcoded `profileId` the *planned* Week 4 UI scope, not a fallback. Week 4 frontend deliverables drop the profile creation form. The `POST /profiles` endpoint from Week 3 stands on its `WebApplicationFactory` integration tests. Wednesday-EOD mid-week cut trigger from option α survives as a secondary safety net inside the new cut rule.
- **Why:** Option α only reduces Week 4 hours *if the cut trigger fires under mid-week sunk-cost pressure*. Option β actually subtracts work upfront (~3–4 hrs of UI budget: profile form state + validation + POST + success-state UI). The profile form is plumbing, not the thesis — ScholarSense's thesis is eligibility verification *rendered* (search → results with verdicts and matched/failed criteria). Interview defense holds via integration tests for the profiles endpoint. Options γ (move UI to Week 5), δ (defer score fusion), and ε (β+δ) were rejected: γ just moves the Trojan horse one week right; δ weakens the governing rule (line 7) by breaking the ranking story for trivial savings; ε inherits δ's flaws.
- **Files:** `EXECUTION.md` (Week 4 section), `PROGRESS.md` (this entry + Snapshot). DESIGN.md unchanged — API surface still has `POST /profiles`; only UI scope shrinks.
- **Status:** done
- **Refs:** prior log entry "2026-05-12 — /critique-plan stress-test (design-critic)".

### 2026-05-12 — Resolve Q1: prefilter selectivity is a hypothesis, not a measured fact
- **Type:** decision + doc-update
- **What:** Per `/critique-plan` + a counter-critique pass via the `design-critic` agent, switched from "downgrade to qualitative" (option A) to "label as hypothesis with validation plan + retraction rule" (option B). `DESIGN.md` line 61 rewritten to call the 50–90% range a hypothesis validated in Week 2 against 10 representative profiles, with a retraction rule if measured selectivity falls outside 40–95%. `EXECUTION.md` Week 2 deliverables gained a selectivity-measurement bullet.
- **Why:** Top-5 verification caps LLM cost regardless of prefilter selectivity, so the "fabricated cost claim" framing was wrong — the prefilter's actual role is candidate quality, not cost. 50–90% is a defensible prior (citizenship alone is plausibly 60%+ selective against a global corpus; compose with deadline + GPA-min + level-of-study and 50–90% is reasonable). Labeling beats deletion: an interview answer of "hypothesis with validation plan and retraction rule" shows priors + measurement discipline + falsifiability; "I removed the number" reads as having no model. Keeps DESIGN.md consistent with other unmeasured-but-stated targets (3s timeout, top-5 cutoff).
- **Files:** `DESIGN.md` (line 61), `EXECUTION.md` (Week 2 deliverables), `PROGRESS.md` (this entry + Snapshot).
- **Status:** done
- **Refs:** prior log entry "2026-05-12 — /critique-plan stress-test (design-critic)".

### 2026-05-12 — /critique-plan stress-test (design-critic)
- **Type:** decision
- **What:** Ran `/critique-plan`. Three weaknesses surfaced; resolution captured per-question in subsequent log entries.
- **Why:** Stress-test the plan before any Week 1 implementation, to catch unsubstantiated claims and scope-overload before they get baked into code.
- **Files:** none — review only.
- **Status:** done
- **Refs:** —

### 2026-05-12 — Adopt document-first workflow
- **Type:** decision
- **What:** PROGRESS.md is now a live, append-only ledger. Every change (design tweak, scope edit, file create/edit, dep add, decision) gets a Log entry *before* the change lands. Snapshot section is overwritten each update with current state + next step + in-progress + open questions. Supersedes the EXECUTION.md framing of PROGRESS.md as a weekly recap.
- **Why:** Single source of truth so a fresh session can pick up coherently without re-discovering decisions or re-litigating cut items.
- **Files:** none — workflow rule (also captured in Claude's persistent project memory at `progress-ledger-workflow.md`).
- **Status:** done
- **Refs:** —

### 2026-05-12 — Create PROGRESS.md
- **Type:** doc-update
- **What:** Initial creation of PROGRESS.md with Snapshot + append-only Log structure.
- **Why:** Bootstrap the live ledger so subsequent work has a place to land.
- **Files:** `PROGRESS.md` (new).
- **Status:** done
- **Refs:** —
