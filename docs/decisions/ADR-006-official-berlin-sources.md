# ADR-006: Restrict runtime cadastral data to official Berlin sources

- Status: Accepted
- Date: 2026-09-15

## Context

The value of the project depends on representing real cadastral/geospatial entities. Generated polygons or unofficial mirrors could make demonstrations look functional while invalidating provenance and research usefulness.

## Decision

Runtime ingestion is configured for official Berlin ALKIS parcel, building and district WFS services. Synthetic features are permitted only in deterministic tests. If an official endpoint changes, inspect and adapt to the current authoritative contract rather than inventing a response shape.

## Consequences

The project remains grounded in real public data and retains clear provenance. Live execution inherits external service availability and schema-change risk, which is documented and isolated from deterministic CI.
