 # Unit Testing Guidelines

## Purpose

Use this document to guide the creation and maintenance of **unit tests only**. A unit test verifies a single function, method, or class in isolation.

## Scope

- Test behavior and business rules of individual units.
- Keep dependencies isolated with mocks, stubs, or fakes.
- Do not add integration, API, database, end-to-end, or performance tests here.

## Test cases

For each unit, cover relevant:

- Expected behavior for typical valid inputs.
- Boundary and edge cases, such as empty collections or optional values.
- Invalid inputs and expected errors or exceptions.
- Important branches and business rules.

## Guidelines

- Follow the repository's existing test framework, naming conventions, and test command.
- Keep each test focused on one behavior and independent of other tests.
- Use descriptive names that communicate the scenario and expected result.
- Structure tests clearly as Arrange, Act, Assert.
- Keep tests deterministic; avoid external services, real databases, and shared mutable state.
- Assert observable behavior rather than private implementation details.

## Done criteria

- The unit tests pass using the repository's established test command.
- New or changed behavior has appropriate unit test coverage.
- Tests do not depend on execution order or external resources.
