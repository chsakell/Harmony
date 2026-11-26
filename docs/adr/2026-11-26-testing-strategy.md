---
title: Test with xUnit v3 on Microsoft.Testing.Platform, using NSubstitute, Shouldly and coverlet.MTP, in a separate tests/ tree
status: proposed         # proposed | accepted | rejected | superseded | deprecated
date: 2026-11-26
deciders: chsakell
work-item: "#5"
supersedes:
superseded-by:
---

# Test with xUnit v3 on Microsoft.Testing.Platform, using NSubstitute, Shouldly and coverlet.MTP, in a separate tests/ tree

## Context

Harmony has no automated tests ([overview](../architecture/overview.md), Known issues). The roadmap puts tests first: R-01 creates the first unit test project, and R-03 to R-06 add integration, API and architecture tests on top. Every later roadmap item starts with a failing test. These choices therefore hold for every test project, and changing them later means touching all of them. Raised by [spec #5](../features/5-test-foundation/spec.md).

Forces:

- **SDLC kit.** Every test carries `[Trait("Requirement", "FR-nnn")]`. The verify skill runs `dotnet test --solution <sln> --filter-trait "Requirement=..."`, which is Microsoft.Testing.Platform (MTP) syntax. Today `global.json` does not opt into MTP, so `dotnet test --solution` fails (AGENTS.md).
- **Licences.** Harmony is open source and self-hosted, so only OSI-licensed libraries can be used. FluentAssertions 8 is commercial. AutoMapper and MediatR are already pinned at their last open-source releases for the same reason.
- **Build layout.** The SDK pin is 10.0.100 with `rollForward: latestFeature` (10.0.404 installed). Central package management is in `src/Directory.Packages.props` and the target framework in `src/Directory.Build.props`. MSBuild looks for both by walking up from each project, so a project outside `src/` does not inherit them.
- **The code.** Application handlers depend only on interfaces, so they can be unit tested with substitutes. About 15 handlers run EF Core async operators directly on queries; those need a real provider (R-03).
- No `dotnet-instructions` standard is installed, so this ADR departs from none.

## Decision

1. **Runner:** opt the solution into Microsoft.Testing.Platform in `global.json` (`"test": { "runner": "Microsoft.Testing.Platform" }`). The canonical command is `dotnet test --solution Harmony.slnx`. Every test project must run on MTP.
2. **Framework:** xUnit v3 (Apache-2.0), using its MTP-native runner.
3. **Test doubles:** NSubstitute (BSD-3-Clause).
4. **Assertions:** Shouldly (BSD-3-Clause).
5. **Coverage:** coverlet.MTP (MIT), configured in the shared test props so that every run writes a Cobertura report under `TestResults/`. No coverage threshold for now.
6. **Layout and naming:**
   - Tests live in `tests/`, in a `/tests/` solution folder.
   - Projects are named `<ProjectUnderTest>.UnitTests` for tests with no I/O (for example `Harmony.Application.UnitTests`) and `<ProjectUnderTest>.IntegrationTests` for tests that need Testcontainers or a test host (R-03, R-04). Architecture rules go in a single `Harmony.ArchitectureTests` (R-06).
   - `tests/Directory.Build.props` and `tests/Directory.Packages.props` each `<Import>` their `src/` counterpart and add test-only settings. There is still one version list, in `src/Directory.Packages.props`, and no existing file moves.
7. **Conventions:**
   - Every test has a `Requirement` trait (on the method or the class) matching `FR-nnn` or `NFR-nnn`. A guard test in each test project enforces this.
   - Current behaviour that is probably a defect is pinned, not skipped. The test method name ends with `_KnownIssue` and points to where the defect is recorded. The item that fixes the defect changes that test on purpose.
   - Unit tests set the culture explicitly when output depends on it, and never read the clock.
8. **Packages in the feature PR:** the test packages are added in the R-01 pull request. This is an accepted exception to the SDLC guide's rule that central package additions go in a separate `build:` PR: they are used only by new test projects and cannot conflict with other work. Later test packages (for example Testcontainers) follow the normal rule.

## Alternatives considered

| Option | Pros | Cons | Why not chosen |
|---|---|---|---|
| VSTest runner (no `global.json` change) | Works with `dotnet test Harmony.slnx` today; coverlet.collector is the familiar choice | `dotnet test --solution` and `--filter-trait` unavailable, so the SDLC kit's commands need rewriting; VSTest is the legacy path for new .NET test work | The kit's verify commands and the platform's direction both point to MTP |
| MSTest or NUnit instead of xUnit v3 | Both run on MTP; MSTest ships with the SDK templates | No advantage for this codebase; xUnit's `Trait` matches the kit's tagging examples directly | xUnit v3 is the roadmap's choice and the most common in the .NET open-source ecosystem |
| AwesomeAssertions (Apache-2.0 fork of FluentAssertions) | Familiar `Should()` style for FluentAssertions users | Larger API surface; a younger fork, so its long-term maintenance is less proven | Shouldly is smaller, mature and has clear failure messages |
| FluentAssertions 8 | Most widely known | Commercial licence | Breaks the open-source-only constraint |
| Moq instead of NSubstitute | Very common | The 2023 SponsorLink episode damaged trust; more verbose setup | NSubstitute is equally capable with simpler syntax |
| Microsoft.Testing.Extensions.CodeCoverage | First-party, MTP-native | Ships under a Microsoft licence file rather than an OSI licence expression (NuGet metadata, 2026-11-26) | coverlet.MTP is MIT and keeps the roadmap's coverlet choice |
| Tests next to code (`src/**/Tests`) | Short paths | Mixes shipped and test code; harder to exclude from containers and packaging | A separate `tests/` tree is the common .NET layout and keeps Dockerfiles unchanged |
| Move `Directory.*.props` to the repository root | One inheritance chain for `src/` and `tests/` | Touches every project's inheritance and the solution items | More change than needed; the imports give the same single version list |

## Consequences

- **Positive:**
  - The SDLC kit's verify commands work unchanged.
  - Requirement traceability is enforced by a test, not just by convention.
  - Every later test project starts from one documented setup.
  - All test dependencies are open source: Apache-2.0, BSD-3-Clause and MIT, checked against NuGet metadata on 2026-11-26.
- **Negative:**
  - The documented test command changes to `dotnet test --solution Harmony.slnx`, and the old positional form no longer applies. Contributors and CI (R-02) must use the new one.
  - Every future test project must support MTP. That rules out tools that work only with VSTest.
  - The unmapped-member baseline guard (spec FR-016, FR-017) is specific to AutoMapper and will be removed or replaced by R-14.
- **Follow-up work:**
  - R-01 updates AGENTS.md (the Test command) and the overview's Known issues and Decision index.
  - R-02 runs the new command in CI and uploads the Cobertura and test results.
  - Exact package versions and the xUnit v3 MTP v1/v2 package choice are settled in the R-01 plan by a short trial on SDK 10.0.404. Versions available on 2026-11-26: xunit.v3 4.0.3, coverlet.MTP 10.1.1, Shouldly 4.3.0, NSubstitute 6.2.0.
  - A coverage threshold is to be decided once R-02 to R-05 have added more tests.
