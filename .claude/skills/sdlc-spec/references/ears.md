# EARS notation for requirements

EARS (Easy Approach to Requirements Syntax, Mavin et al., 2009) gives every requirement the same shape, so it is unambiguous and maps directly to a test.

General form, clauses always in this order:

**While** <precondition>, **when** <trigger>, **the** <system> **shall** <response>.

A requirement has zero or more preconditions, at most one trigger, exactly one system name and one or more responses.

## Patterns

| Pattern | Keyword | Example |
|---|---|---|
| Ubiquitous | (none) | The Orders API shall return timestamps in ISO 8601 UTC. |
| Event-driven | When | When a customer submits a valid order, the Orders API shall return 201 with a Location header. |
| State-driven | While | While the payment provider is unavailable, the Checkout service shall queue payment requests. |
| Unwanted behaviour | If ... then | If the basket is empty, then the Orders API shall return 400 with a ProblemDetails body. |
| Optional feature | Where | Where multi-currency is enabled, the Pricing service shall show prices in the customer's currency. |
| Complex | combination | While the user is signed in, when the user requests an export, the Reports API shall return a CSV with only the rows the user may see. |

## Rules

- One requirement, one behaviour. Split "and" responses that can fail independently.
- Name the system the same way every time ("the Orders API", not "the API" in one place and "the service" in another).
- The response is observable from outside: a status code, a field, a message, a stored record, an event.
- No vague words: fast, easy, user-friendly, robust, scalable, secure, appropriate. Use numbers or a named rule.
- No design inside the requirement: no class names, tables or libraries.

## From requirement to test

Every ID gets at least one automated test, tagged with the ID so it can be traced and filtered:

| EARS part | Test part |
|---|---|
| While (precondition) | Arrange / Given |
| When or If (trigger) | Act / When |
| shall (response) | Assert / Then |

```csharp
[Fact]
[Trait("Requirement", "FR-002")]
public async Task Submit_EmptyBasket_Returns400WithProblemDetails()
```

Run all tests for one requirement:

```powershell
dotnet test --solution <Solution>.slnx --filter-trait "Requirement=FR-002"
```
