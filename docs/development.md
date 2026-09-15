# Development guide

## Prerequisites

- .NET 10 SDK
- Docker with Compose v2
- Git

No database, Java runtime or RDF server is required for the v1 architecture.

## Build and deterministic test

```bash
dotnet restore BerlinCadastre.slnx
dotnet format BerlinCadastre.slnx --verify-no-changes --no-restore
dotnet build BerlinCadastre.slnx -c Release --no-restore
dotnet test BerlinCadastre.slnx -c Release --no-build
```

Normal tests must remain independent of external Berlin services.

## TDD workflow

Behavioural changes should follow Red → Green → Refactor:

1. Add a test expressing the externally meaningful behaviour or contract.
2. Verify it fails for the expected reason.
3. Commit the red contract separately when that history is useful.
4. Implement the smallest correct behaviour.
5. Run the focused test and then the complete deterministic suite.
6. Refactor only while protected by tests.
7. Update documentation when the observable contract, architecture, source assumptions or limitations changed.

Tests should assert contracts, geometry semantics and output behaviour rather than private method structure.

## Test layers

`BerlinCadastre.Domain.Tests` covers IDs, geometry invariants and CRS contracts.

`BerlinCadastre.Application.Tests` covers deterministic spatial predicates and relationship derivation.

`BerlinCadastre.Infrastructure.Tests` uses controlled HTTP responses and fixed geometries for WFS parsing/pagination, source mapping and CRS transformation.

`BerlinCadastre.Semantics.Tests` verifies RDF classes/relationships, GeoSPARQL WKT/provenance and SHACL failure behaviour.

`BerlinCadastre.Api.Tests` hosts the ASP.NET Core application with deterministic repository fixtures and verifies public API/export contracts.

`BerlinCadastre.LiveTests` is deliberately excluded from the solution used by normal CI. It checks real Berlin WFS compatibility and can fail because an external service is unavailable; this is why it has its own workflow. The live workflow runs on schedule, on manual dispatch, and when its own contract/test files change; ordinary application pushes and pull requests therefore remain independent of third-party WFS availability.

## Performance diagnostics

Performance is measured before optimisation rather than inferred from dataset size. Structured application logs record:

- accepted and rejected source-feature counts;
- wall-clock time spent waiting for the three concurrent WFS ingestion streams;
- cumulative WFS request time across paginated source calls;
- GeoJSON parse and geometry-validation time;
- domain mapping time;
- parcel/building/district spatial-relationship derivation time;
- total ingestion time and managed-memory delta; and
- RDF triple count, SHACL validation time, Turtle serialisation time, UTF-8 output size and managed-memory delta.

These measurements are diagnostics, not service-level objectives. They are intended to show when bounding-box filtering, indexing, streaming, persistence or other scalability work becomes justified by evidence.

## Source changes

Never change a mapper because a guessed WFS field name sounds plausible. When a Berlin schema changes:

1. inspect the current `GetCapabilities` and actual `GetFeature` response;
2. update a deterministic fixture/test to capture the observed contract;
3. adjust the boundary parser/mapper;
4. update `docs/data-sources.md` with review date and relevant change; and
5. run the live-source smoke test separately.

## Adding geometry behaviour

Every new metric operation must establish the CRS it expects. Tests should include edge cases such as touching boundaries, disjoint geometry, invalid input, empty geometry where accepted at a boundary, MultiPolygon behaviour, and an explicit wrong-SRID rejection where relevant.

Do not compare coordinates with naive equality for topological decisions.

## Semantic changes

Prefer GeoSPARQL, PROV-O, RDFS and established terms before adding custom vocabulary. When a new required semantic property is introduced, update both the graph builder and SHACL shape, then add valid and invalid graph tests.

Do not use SHACL as evidence of real-world correctness.

## Container verification

```bash
docker compose config --quiet
docker build -f docker/Dockerfile -t berlin-semantic-cadastre:local .
docker compose up --build
```

After startup, `/health` should respond independently of source state; `/ready` should become healthy only after the real ingestion has populated all required feature categories.

## Commit discipline

Commits should represent comprehensible engineering increments (contract, implementation, infrastructure, documentation) and use imperative messages. Do not rewrite history merely to simulate a development sequence that did not occur.
