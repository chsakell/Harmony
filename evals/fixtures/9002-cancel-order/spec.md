---
id: 9002
title: Cancel an order
artifact: spec
status: approved
owner: Eval fixture (product owner)
approved-by: eval-fixture, 2026-01-01
created: 2026-01-01
intent: ./intent.md
---

# Spec: Cancel an order

## Summary

Customers cancel their own orders through the Orders API until the order ships. See [intent.md](./intent.md).

## User stories

### US1 - Cancel my order (P1)

As a customer, I want to cancel an order that has not shipped, so that I do not have to call support.

## Functional requirements

| ID | Requirement | Story |
|---|---|---|
| FR-001 | When the customer who placed an order requests cancellation before the order ships, the Orders API shall set the order status to Cancelled and return 204. | US1 |
| FR-002 | If the order has already shipped, then the Orders API shall return 409 with a ProblemDetails body. | US1 |
| FR-003 | If the caller did not place the order, then the Orders API shall return 404. | US1 |

### Acceptance scenarios

- **FR-001** - Given an unshipped order of customer A, when customer A cancels it, then the response is 204 and the status is Cancelled.
- **FR-002** - Given a shipped order, when its customer cancels it, then the response is 409.
- **FR-003** - Given an order of customer A, when customer B cancels it, then the response is 404.

## Non-functional requirements

| ID | Category | Requirement (measurable) |
|---|---|---|
| NFR-001 | Observability | Every cancellation writes an audit event with actor, order ID and timestamp. |

## Design

### API contract

`POST /api/v1/orders/{orderId}/cancel` - 204, 404, 409.

## Flagged concerns

None.

## Open questions

None.
