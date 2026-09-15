# Data sources

## Source policy

Production runtime data must originate from public, authoritative Berlin geodata. Synthetic geometries are used only in deterministic tests. WMS is not used for feature ingestion because it is a visualisation service; the runtime consumes vector features through WFS.

Source contracts were reviewed on **2026-09-15**. At that review, the Berlin Open Data metadata for the configured ALKIS layers reported a data state of **2026-07-31**. The separate live-source workflow exists because external service capabilities and schemas can change after a code release.

## ALKIS Berlin Flurstücke

| Field | Value |
| --- | --- |
| Provider | Senatsverwaltung für Stadtentwicklung, Bauen und Wohnen Berlin |
| Dataset | ALKIS Berlin – Flurstücke |
| Service | OGC WFS 2.0.0 |
| Endpoint | `https://gdi.berlin.de/services/wfs/alkis_flurstuecke` |
| Feature type | `alkis_flurstuecke:flurstuecke` |
| Requested CRS | EPSG:25833 (ETRS89 / UTM zone 33N) |
| Licence | Datenlizenz Deutschland – Zero – Version 2.0 (`dl-de-zero-2.0`) |
| Runtime use | Parcel geometry and source attributes |
| Retrieval | Runtime-dependent; every entity records its ingestion timestamp |

## ALKIS Berlin Gebäude

| Field | Value |
| --- | --- |
| Provider | Senatsverwaltung für Stadtentwicklung, Bauen und Wohnen Berlin |
| Dataset | ALKIS Berlin – Gebäude |
| Service | OGC WFS 2.0.0 |
| Endpoint | `https://gdi.berlin.de/services/wfs/alkis_gebaeude` |
| Feature type | `alkis_gebaeude:gebaeude` |
| Requested CRS | EPSG:25833 (ETRS89 / UTM zone 33N) |
| Licence | Datenlizenz Deutschland – Zero – Version 2.0 (`dl-de-zero-2.0`) |
| Runtime use | Building geometry and source attributes |
| Retrieval | Runtime-dependent; every entity records its ingestion timestamp |

## ALKIS Berlin Bezirke

| Field | Value |
| --- | --- |
| Provider | Senatsverwaltung für Stadtentwicklung, Bauen und Wohnen Berlin |
| Dataset | ALKIS Berlin – Bezirke |
| Service | OGC WFS 2.0.0 |
| Endpoint | `https://gdi.berlin.de/services/wfs/alkis_bezirke` |
| Feature type | `alkis_bezirke:bezirksgrenzen` |
| Requested CRS | EPSG:25833 (ETRS89 / UTM zone 33N) |
| Licence | Datenlizenz Deutschland – Zero – Version 2.0 (`dl-de-zero-2.0`) |
| Runtime use | District geometry, district naming and spatial membership |
| Retrieval | Runtime-dependent; every entity records its ingestion timestamp |

Public Berlin examples expose `namgem` as a district-name attribute; the mapper uses it when present. The parser nevertheless preserves the complete returned property set and does not require a guessed parcel/building attribute schema.

## Provenance model

Each mapped entity carries:

- provider;
- dataset title;
- source endpoint;
- service type;
- source CRS;
- licence;
- source feature identifier; and
- ingestion timestamp.

The ingestion timestamp means when this application retrieved the feature. It is not presented as the source's observation/update timestamp.

## Source identifiers

A stable WFS feature identifier is required. The parser first uses the GeoJSON feature `id`; controlled property fallbacks exist for common explicit identifier fields. If no source identifier is available, the feature is rejected. The system does not synthesize production identifiers from coordinates or row order.

## Pagination and extent

The WFS client requests WFS 2.0.0 pages using `count` and `startIndex` and stops on a short page. `MaxPages` provides a safety bound. The default parcel/building ingestion uses a configurable EPSG:25833 bounding box rather than attempting to pull the complete Berlin cadastre into memory.

District boundaries are small enough to load without the same sample bounding box and are used to derive administrative membership for the bounded parcel/building set.

## Failure policy

Transport timeouts/HTTP failures are retried a bounded number of times. Invalid or empty geometries, missing identifiers and malformed features are rejected explicitly. A live startup failure leaves the service process healthy but not ready so monitoring can distinguish infrastructure health from data readiness.
