---
description: Run a five-question Microsoft-style design interview on this week's shipped work. Use Friday end-of-week or before a real interview.
---

Delegate to the `interview-coach` sub-agent.

Before delegating, run `git log --since="1 week ago" --pretty=format:"%h %s"` and pass the summary to the sub-agent so the questions focus on shipped work, not aspirational features.

After the session, save a short "weak spots" note to `docs/interview-notes/week-N.md` (create the directory if it doesn't exist) so the gaps surface as commits you can address in the following week.
