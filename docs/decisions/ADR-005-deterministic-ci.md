# ADR-005: Keep normal CI independent of live WFS availability

- Status: Accepted
- Date: 2026-09-15

## Context

Third-party/public WFS availability is outside repository control. Making every pull request call live Berlin services would turn network outages or maintenance into unrelated CI failures, while never testing live sources would allow schema drift to go unnoticed.

## Decision

Normal CI uses deterministic fixtures and controlled HTTP handlers. A separate scheduled/manually triggered `live-source-smoke` workflow requests real features from the three configured Berlin WFS endpoints and verifies parser compatibility.

## Consequences

Pull-request signal remains deterministic while external compatibility is still monitored. A live-source failure is an integration signal to investigate, not automatically evidence that an application change is defective.
