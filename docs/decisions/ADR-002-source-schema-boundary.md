# ADR-002: Separate source schemas from the canonical domain

- Status: Accepted
- Date: 2026-09-15

## Context

Berlin WFS response schemas are provider contracts and can evolve. Letting GeoJSON/GML/WFS DTOs leak into application logic would make every source change a domain change and would encourage guessed field dependencies.

## Decision

Use infrastructure boundary records (`WfsFeature`) and a dedicated Berlin mapper. Canonical entities expose stable identity, geometry, derived relationships, provenance and a preserved attribute dictionary rather than source-specific transport classes.

## Consequences

Application/semantic layers are isolated from HTTP and WFS serialization details. Source attributes remain available for GIS inspection without becoming domain invariants. Any attribute promoted into the domain in the future must be justified and based on a verified source contract.
