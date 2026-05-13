---
description: Stress-test the current plan with the design-critic sub-agent. Run before major decisions or whenever scope feels like it's drifting.
---

Delegate to the `design-critic` sub-agent with this brief:

> Read `DESIGN.md`, `EXECUTION.md`, and `PROGRESS.md` (if it exists). Identify the three weakest claims, scope risks, or unsubstantiated assumptions in the current state of the plan. For each, give the quote, the risk, and the concrete correction. End with one sentence on what is genuinely working and should not be changed.

After the sub-agent returns, do not silently apply its suggestions. Surface them to me as a numbered list with your own assessment of whether to accept, modify, or reject each one. I'll decide.
