# ADR-004: Keep v1 spatial computation in .NET

- Status: Accepted
- Date: 2026-09-15

## Context

A spatial database or GeoSPARQL store could evaluate topology, but the bounded v1 workload does not yet demonstrate a need for either. Adding PostGIS/Fuseki only to make the architecture appear sophisticated would create operational complexity without evidence.

## Decision

Run v1 spatial predicates in the application layer with NetTopologySuite over the in-memory repository. RDF publication remains a semantic interoperability concern rather than the spatial execution engine.

## Consequences

Local runtime stays small and reproducible. Whole-city scale may eventually exceed this model; feature counts, memory and query latency should be measured before introducing an indexed spatial store. A future persistence layer can implement the repository abstraction without changing the domain contract.
