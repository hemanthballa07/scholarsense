---
description: Compare this week's actual Git activity against the planned deliverables in EXECUTION.md. Run every Monday.
---

Determine the current ISO week and which project week we're in (Week 1–6). Then:

1. Read `EXECUTION.md` and locate the deliverables and definition-of-done for the current project week.
2. Run `git log --since="last week" --pretty=format:"%h %s"` and summarize what was actually shipped.
3. Compare. For each planned deliverable, mark:
   - ✅ Shipped
   - 🟡 In progress (commits exist but not done)
   - 🔴 Not started

4. Surface risks: if the week is more than 50% through and 🔴 items exist on the critical path, say so explicitly and recommend either re-prioritization or invoking the week's **cut rule** from `EXECUTION.md`.

5. End with: "Recommendation for this week: [focus on X, defer Y, cut Z if needed]."

Do not modify any files. This is a read-only checkpoint.
