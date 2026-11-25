---
id: 9001
title: Export orders as CSV
artifact: intent
status: approved
owner: Eval fixture (product owner)
approved-by: eval-fixture, 2026-01-01
created: 2026-01-01
source: idea
---

# Intent: Export orders as CSV

## Problem

Account managers rebuild order lists by hand in spreadsheets every month to reconcile invoices. It takes each of them about half a day per month and the copies are often out of date.

## Who is affected

Account managers (about 40 people) and the finance team that receives the reconciled lists.

## Desired outcome

An account manager can download the orders of one customer for a chosen period as a file that opens in a spreadsheet.

### Success measures

- Manual reconciliation time drops from half a day to under 30 minutes per month.

## Scope

- **In scope:** orders of one customer, a date range, the fields already shown on the order list.
- **Out of scope:** scheduled exports, exports across customers, invoice data.

## Constraints

- Account managers may only export customers they are assigned to.
- No new personal data leaves the system: customer contact details are not included.

## Affected systems

Orders API, account manager portal.

## Open questions

- [ ] What is the largest date range we need to support? - finance team
