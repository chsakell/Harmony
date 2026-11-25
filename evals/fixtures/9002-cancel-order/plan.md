---
id: 9002
title: Cancel an order
artifact: plan
status: draft
owner: Eval fixture (engineer)
approved-by:
created: 2026-01-01
spec: ./spec.md
---

# Plan: Cancel an order

## Approach

Add a cancel endpoint next to the existing order endpoints, a `Cancel` method on the order aggregate that rejects shipped orders, and an audit event.

## Tasks

- [ ] **T1** Add `Order.Cancel()` that rejects shipped orders - covers FR-001, FR-002 - proof: `OrderTests.Cancel_ShippedOrder_Throws`
- [ ] **T2** Add `POST /api/v1/orders/{orderId}/cancel` - covers FR-001, FR-002, FR-003 - proof: `CancelOrderEndpointTests`
- [ ] **T3** Write the audit event - covers NFR-001 - proof: `CancelOrderEndpointTests.Cancel_WritesAuditEvent`
