# R06 independent unit test report

Validation date: 2026-11-07.

## Tested revisions

- Development PR: [#27](https://github.com/DaviJedrzejczyk/API-F1-Statistics/pull/27).
- Exact product commit: `16eb61c02563ff8f25aae08e760ad2fac56c6257`.
- Exact tested product-plus-tests commit: `99a3618ef2c4897455951166798122dd6fdd20a6`.
- Isolated branch: `DJK-Agents/r06-v2-catalogo-voltas-test`.
- PR destination: `DJK-Agents/r06-v2-catalogo-voltas-dev`.
- This report is a documentation-only follow-up to the tested commit; no product files were changed.

## Validation results

SDK: .NET `10.0.403`, targeting the existing `net8.0` projects.

| Command | Result |
| --- | --- |
| `dotnet test UnitTests/UnitTests.csproj --logger "trx;LogFileName=r06-full.trx" --results-directory TestResults` | Exit 0; 393 passed, 28 skipped, 0 failed; 421 total |
| `dotnet build "API OpenF1.sln"` | Exit 0; incremental build reported 0 warnings and 0 errors |
| `git diff --check` | Passed |

The full test command compiled the new tests and emitted existing repository warnings; neither new test file emitted a warning. Local evidence is in the ignored `TestResults/r06-full.trx`, `TestResults-r06-full.log` and `TestResults-r06-build.log` files. The focused run passed all 36 new cases before the full run; the full run includes those same cases.

## Independent coverage

`UnitTests/Service/LapCatalogIndependentTests.cs` adds 29 cases using isolated mocks and serialized storage payloads:

- Three pilots retain independent lap counts (3, 4 and 2), ordering and suggestions after reconstructing the service.
- Lap, compound and alert filters intersect before suggestion, preserve unfiltered totals, and allow manual choice when no suggestion exists.
- Equal positive durations use the lowest lap number; source order does not affect that choice.
- Null and zero durations remain distinct and are never suggested.
- Pit in/out and explicit time deletion apply to the identified driver and lap; the race's current lap is not assumed to be the deleted lap.
- Unavailable pit/race control sources remain explicit; absence of an identified alert does not imply official validity.
- Persisted complete catalogs and expired but historically complete enrichment payloads precede external calls.
- Reimporting a complete catalog performs no lap write or external request; partial catalogs reopen without fetching and retry only the incomplete pilot during import.
- Invalid persisted JSON returns a conflict without fetching; failed session, driver or lap reads never fall back to OpenF1.
- Wrong source identities, failed responses, successful responses with null payloads and cancellation do not publish a completed catalog.
- Source failure retains existing lap rows and does not publish a replacement snapshot.
- Manual selection accepts zero/null duration, pit out and explicitly deleted times, preserving the serialized choices and alerts through preview, confirmation and service reconstruction.
- A new preview does not replace the confirmed selection; confirming the same preview is idempotent, and a preview from another session is rejected.
- Confirmation acquires the session transaction before reading the preview, stages preview/latest selection, saves, commits and disposes in order; a save failure never commits the transaction or reports success.

`UnitTests/WebApi/LapsLegacyRegressionTests.cs` adds seven isolated controller cases for the previous fastest-lap and fastest-sector actions: original success payloads, cancellation forwarding, empty sectors, partial-sector observations, and 404 error contracts with exception details hidden. The full suite also runs the existing HTTP error, Swagger route, client, DAO and service regressions.

## Fixture review

Reviewed closed [PR #24](https://github.com/DaviJedrzejczyk/API-F1-Statistics/pull/24), exact head `9af9ca32c555caed97491f8a982ee187291da392`, including its six JSON fixtures and two test classes. Only its three lap JSON fixtures were copied. Their missing `is_pit_out_lap` fields remain unknown; the old race-control fixture's `LAP TIME DELETED - TRACK LIMITS` message does not identify a specific lap. The new tests explicitly cover that ambiguity. The old test classes were not copied; their nullable DTO-to-entity assignments do not match the current domain properties. The closed branch and PR were preserved.

## Limits and outstanding validation

`INTEGRATION_SQLSERVER_CONNECTION` was absent. All 28 existing SQL Server integration cases were therefore skipped with their configured prerequisite message. No real database, live OpenF1 service, hosted worker or production connection was used.

Transaction order and unsuccessful commits were checked with mocks. Actual SQL rollback, uniqueness under concurrent imports, application-lock behavior and competing confirmations remain unverified in this environment; the unit recording store does not prove those database guarantees. No new integration cases were added, respecting `.codex/tests/TESTS.md` and the instruction to keep unit tests isolated.

No product defect was reproduced by these cases. The test changes are ready for review; real SQL concurrency validation remains a separate environment-dependent check. No merge was performed.

## Automation identity

Before publication, `gh api user` returned `DJK-Agents`, `gh auth status` identified it as the active account, and `ssh -T git@github-agents` authenticated as `DJK-Agents`. Git author and committer were `DJK-Agents <contabronze62@gmail.com>`.
