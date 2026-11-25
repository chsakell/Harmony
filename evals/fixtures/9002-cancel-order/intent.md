---
id: 9002
title: Cancel an order
artifact: intent
status: approved
owner: Eval fixture (product owner)
approved-by: eval-fixture, 2026-01-01
created: 2026-01-01
source: idea
---

# Intent: Cancel an order

## Problem

Customers cannot cancel an order themselves before it ships; they call support, which handles about 300 such calls a week.

## Who is affected

Customers and support agents.

## Desired outcome

A customer can cancel their own order until it is handed to the carrier.

### Success measures

- Cancellation calls to support drop by 80%.

## Scope

- **In scope:** cancelling a whole order before it ships.
- **Out of scope:** partial cancellation, refunds (handled by the payments team).

## Constraints

Only the customer who placed the order may cancel it.

## Affected systems

Orders API.

## Open questions

None.
