# SKILLS.md

Project preferences for AI agents (and humans) working on FodmapLog / HealthyGutLog.

## Simplicity first

- **Do not overengineer.** Prefer the simplest approach that solves the problem.
- **Write for humans.** Code must stay readable when the owner revisits it later — clear names, short functions, obvious control flow.
- **Choose the simpler option** whenever two solutions would both work (fewer layers, fewer helpers, less indirection).
- **Avoid** premature abstractions, extra design patterns, clever generics, or large refactors “for cleanliness” unless asked.
- **Small diffs:** change only what the task needs; match existing style in the files you touch.
- **Comments:** only where intent is non-obvious; do not narrate obvious code.

This complements `AGENTS.md` (architecture and stack rules). When in doubt, favor simple and readable.

## Frontend build hygiene

- Before finishing Angular work, clear template compiler warnings such as **NG8107** (unnecessary `?.` when the type is already non-nullable). Use `.` when the type is a plain `string` / non-optional object property.
- Do not leave known `ng serve` / `ng build` warnings that your change introduced or that you can fix in the same files you touched.
