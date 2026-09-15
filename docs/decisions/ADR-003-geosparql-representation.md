# ADR-003: Use GeoSPARQL for semantic geometry representation

- Status: Accepted
- Date: 2026-09-15

## Context

The project needs linked relationships and interoperable spatial metadata, not merely another JSON serialization of domain objects. Inventing a custom geometry vocabulary would reduce interoperability.

## Decision

Represent features and geometries using GeoSPARQL concepts and `geo:wktLiteral`, with an explicit EPSG:25833 CRS URI. Use PROV-O for source derivation. Keep a minimal custom `cad:` vocabulary only for cadastral classes/relationships not supplied directly by the reused vocabularies.

## Consequences

RDF consumers can identify feature/geometry/provenance semantics without project-specific parsing. The representation is GeoSPARQL-compatible, but this decision alone does not imply that a SPARQL engine with GeoSPARQL spatial functions is deployed.
