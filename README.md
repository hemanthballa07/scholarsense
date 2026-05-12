# ScholarSense — Workspace Package

This is the **start-here package**. Copy the contents into a fresh GitHub repo, install Claude Code, and you have a working agentic engineering setup for ScholarSense.

## What's in this package

```
scholarsense/
├── DESIGN.md                       # the technical reference. Read first.
├── EXECUTION.md                    # week-by-week plan, build order, cut rules.
├── CLAUDE.md                       # project context for Claude Code.
├── README.md                       # this file. The workflow.
└── .claude/
    ├── agents/
    │   ├── design-critic.md        # adversarial planner
    │   ├── code-reviewer.md        # Microsoft-bar PR review
    │   └── interview-coach.md      # design-interview quizzer
    └── commands/
        ├── critique-plan.md        # /critique-plan
        ├── week-checkpoint.md      # /week-checkpoint
        └── interview-quiz.md       # /interview-quiz
```

## The intended workflow

You will not just code this project. You will run it as a small engineering team where Claude plays several roles, and you make the calls.

### One-time setup (~30 minutes)

1. Create a new GitHub repo: `scholarsense`.
2. Copy these files into the repo root.
3. Install Claude Code: `npm install -g @anthropic-ai/claude-code` (or whichever installer is current — check `docs.claude.com`).
4. In your repo directory, run `claude` to start a session. It will read `CLAUDE.md` automatically.
5. **Before any coding**, run the self-critique step below.

### The self-critique step (do this before Week 1)

This is the part you asked about — running the plan through one more adversarial pass *inside* Claude Code instead of in a chat. From your repo root, in a Claude Code session:

```
/critique-plan
```

The `design-critic` sub-agent will read DESIGN.md, EXECUTION.md, and any progress notes, and produce three corrections. You then decide for each: accept, modify, reject. **Apply the accepted ones to the docs and commit.** This becomes your locked plan.

After this round, **do not run `/critique-plan` again before each week**. Run it only when scope is genuinely drifting or before a major architecture decision. Otherwise you're just procrastinating.

### Phase 0 — .NET warm-up (before Week 1, ~2 hours)

Microsoft Learn: "Build a web API with ASP.NET Core minimal APIs." Build `/health` locally. No Cosmos, no AI Search. Just confirm tooling clicks. If at hour 2 you're drowning, message me and we'll re-plan Week 1 with more learning time.

### Weekly cycle (Weeks 1–6)

**Monday (5 min).**
```
/week-checkpoint
```
The slash command reads EXECUTION.md, looks at your Git log, and tells you what's at risk this week.

**Mid-week.** When you finish a feature branch, ask Claude Code to invoke the `code-reviewer` sub-agent on your diff. The standard way:
> "Have the code-reviewer sub-agent review my latest commits."

Blockers must be addressed before merge. Strong suggestions are usually right. Nits are taste.

**Friday (10 min).**
```
/interview-quiz
```
Five questions on this week's shipped work. The session notes go to `docs/interview-notes/week-N.md`. Don't skip this. The interview prep compounds.

**End of week.** Update `PROGRESS.md`:
```markdown
## Week N
Shipped:
- ...
Slipped:
- ...
Risks:
- ...
```

### When to deviate from the plan

- Load test produces real latency numbers → update `DESIGN.md` with measurements, add the resume bullet.
- Cosmos or AI Search behaves differently than expected → update `DESIGN.md`, never silently work around it.
- An external dependency (CareerOneStop, a public scholarship source) blocks → trigger that week's cut rule and move on.

## The trends this setup leans on

- **Sub-agents** (specialized roles in `.claude/agents/`) for separation of concerns. Each agent has a narrow job and its own context window. Critic ≠ reviewer ≠ coach.
- **Slash commands** (`/critique-plan` etc.) for repeatable rituals. Encoding the *workflow* in the repo, not in your memory.
- **CLAUDE.md** as the project's voice. Locked decisions are written down once and re-read every session, so you stop re-explaining the same context.
- **MCP servers (later).** Once you're past Week 1, consider adding a GitHub MCP server for PR management from inside Claude Code, and an Azure MCP server (check `docs.claude.com` for current availability) for resource queries without leaving the terminal.
- **Skills (optional, Week 3+).** If you find yourself repeating a multi-step workflow (e.g., "add a new endpoint" → create handler + validator + integration test + telemetry), encode it as a skill in `.claude/skills/`. Don't pre-build skills; let them emerge from real repetition.

## What this package deliberately does *not* include

- A pre-generated repo scaffold. You build the scaffold yourself in Week 1; that's how `.NET` muscle memory forms.
- Specific Azure CLI / `az` commands. They drift fast — pull current ones from `learn.microsoft.com` when you provision in Week 2.
- A pre-written architecture diagram. You sketch it Week 6 once the system actually exists.
- Resume bullets with specific numbers. Measure first, then claim.

## When you are done iterating

You're done iterating on the plan. Do `/critique-plan` once, accept what fits, commit, and start Phase 0. The plan does not get better by being argued about more.
