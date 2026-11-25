# Example: an API security policy skill

This is an illustration of a policy skill, based on the example in the Claude AI-native SDLC playbook and adapted to ASP.NET Core. Replace the rules with your organization's actual standard and have its owner sign off.

File: `.claude/skills/secure-api-review/SKILL.md`

```markdown
---
name: secure-api-review
description: Applies the API security standard to ASP.NET Core endpoints. Use whenever creating or changing an externally reachable endpoint (Minimal API, FastEndpoints or controller), writing the API contract section of a spec, generating an OpenAPI document, or reviewing API code. DO NOT USE FOR internal background workers with no HTTP surface.
metadata:
  version: "1.0.0"
  owner: Application Security team
  source: "API Security Standard v3 (intranet link)"
---

# Secure API review

When you design, create or change an API endpoint:

1. **Authentication:** every endpoint requires an authenticated caller through the configured scheme. Only `/health` and `/alive` may allow anonymous access.
2. **Authorization:** every endpoint names a policy (`RequireAuthorization("<policy>")`). Resource-level checks verify that the caller may access the specific resource; return 404, not 403, when revealing the resource's existence would leak information.
3. **Input validation:** validate every request body and route value. Reject unknown fields. Return ProblemDetails with status 400 for validation failures.
4. **Audit:** every state-changing endpoint writes an audit event with actor, action, entity, and timestamp.
5. **Data classification:** fields classified as personal data never appear in logs, exception messages, or ProblemDetails.

Before reporting an endpoint as done, list each rule above with how the endpoint meets it, and include that list in your summary.
```

Why it is written this way:

- The description names the moments it should trigger (designing the contract in a spec, writing code, reviewing) so it is applied while the spec is written, not discovered in review.
- Each rule is concrete and checkable.
- The last line makes the agent produce evidence, which the reviewer and the `REVIEW.md` security pass can check.
- Rules that must never be broken (for example "no anonymous endpoints") are also enforced deterministically, for example with an architecture test that scans endpoint metadata, so the skill makes violations rare and the test makes them impossible to merge.
