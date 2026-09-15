# Limitations and non-goals

The project intentionally documents what it cannot establish. A semantic representation of cadastral data does not make the application a legal cadastral authority.

## Legal and cadastral scope

- The system is **not legally authoritative** and is not a replacement for official ALKIS software or Berlin cadastral services.
- It exposes no ownership/person data model and makes no land-title or legal-boundary certification.
- It is read-oriented. There is no cadastral editing, transaction or write-back workflow.
- Only the configured parcel, building and district feature types are integrated; ALKIS contains many additional object types that v1 does not model.

## Spatial derivation

- Building→parcel and feature→district relationships can be spatially derived when a stable source relationship is not used. Representative-point assignment is practical but can be ambiguous for features crossing administrative/cadastral boundaries.
- Geometry transformation can introduce normal floating-point/CRS precision differences.
- Invalid source geometries are rejected; v1 does not attempt automated geometry repair.
- The default in-memory extent is a configurable sample bounding box, not automatically all of Berlin.

## Data freshness and availability

- External WFS services can be unavailable, rate-limited or changed independently of this repository.
- Source schemas can evolve. Normal CI therefore verifies deterministic contracts; a separate live-source workflow detects compatibility drift.
- `RetrievedAt` records application ingestion time, not the provider's observation/update time.

## Persistence and scale

- v1 uses an in-memory repository. Restarting the process requires re-ingestion when live loading is enabled.
- There is no spatial database/index beyond cheap envelope prefiltering in memory. This is deliberate until measured volume/latency justifies PostGIS or another indexed store.
- The default WFS bounding box is intentionally small to prevent an accidental whole-city in-memory import.

## Semantic scope

- RDF output is GeoSPARQL-compatible at the representation level, including explicit CRS WKT literals.
- The runtime does **not** currently host a SPARQL endpoint or an external GeoSPARQL store and therefore does not claim server-side `geof:*` spatial functions.
- SHACL validates graph structure only. Passing shapes does not prove cadastral truth, legal validity, source completeness or geometric survey accuracy.

## API/security scope

The v1 API is intended for local/research execution. It does not claim hardened public production security. A public deployment would require, at minimum, authentication/authorization where appropriate, TLS termination, rate limiting, request/body limits, network isolation, observability, abuse controls and a review of exposed source attributes.

## UI scope

There is no complex built-in dashboard. QGIS and other GIS tools are the intended visualisation environment. A lightweight viewer could be added later, but only after it serves a concrete workflow rather than becoming the project focus.
