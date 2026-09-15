# ADR-001: Use NetTopologySuite for geometry operations

- Status: Accepted
- Date: 2026-09-15

## Context

The domain needs correct OGC-style topology for containment, intersection, distance, representative points, envelopes and heterogeneous geometry types. Hand-written coordinate comparisons would be incorrect for real cadastral polygons.

## Decision

Use NetTopologySuite as the canonical geometry library in the C# domain/application layers. Domain geometry carries an explicit SRID/CRS contract. Envelope checks may prefilter candidates, but exact predicates remain NetTopologySuite operations.

## Consequences

Geometry behaviour is based on a mature spatial model and can handle Polygon/MultiPolygon semantics without custom algorithms. Tests must still define boundary semantics explicitly (`Covers` versus `Contains`). CRS transformation is kept in infrastructure rather than delegated implicitly to the geometry library.
