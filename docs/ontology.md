# Ontology and semantic publication

## Design goal

The semantic layer links cadastral concepts where RDF adds interoperability: stable feature identity, geometry, provenance and cross-entity relationships. It is not a second application object model and does not serialise every source attribute into invented predicates.

## Namespaces

```text
cad:  https://nfloser.github.io/berlin-semantic-cadastre/ontology#
geo:  http://www.opengis.net/ont/geosparql#
prov: http://www.w3.org/ns/prov#
```

The custom vocabulary is defined in `ontology/cadastre.ttl`.

## Classes

`cad:CadastralParcel`, `cad:Building`, and `cad:AdministrativeDistrict` are subclasses of `geo:Feature`. Geometry resources are typed as `geo:Geometry`.

## Relationships

The graph can contain:

```text
cad:Building --cad:locatedOnParcel--> cad:CadastralParcel
cad:Building --cad:locatedIn--------> cad:AdministrativeDistrict
cad:CadastralParcel --cad:locatedIn-> cad:AdministrativeDistrict
```

These relationships are written only when the application domain has derived them. They are not asserted merely because two unrelated source features exist.

## Geometry

Every published feature receives a dedicated geometry IRI and `geo:hasGeometry`. Geometry is represented as an explicit GeoSPARQL WKT literal, for example:

```ttl
<.../parcel/example/geometry>
    a geo:Geometry ;
    geo:asWKT "<http://www.opengis.net/def/crs/EPSG/0/25833> POLYGON (...)"^^geo:wktLiteral .
```

The CRS prefix is important: semantic consumers are not forced to infer what the coordinates mean.

## Provenance

Every feature links to a source entity through `prov:wasDerivedFrom`. Source nodes record the dataset title, endpoint and retrieval timestamp. Runtime provenance distinguishes application ingestion time from upstream dataset update time.

## SHACL

`ontology/shapes.ttl` defines structural publication requirements. Parcels/buildings/districts must have an identifier, a geometry IRI and provenance. Geometry resources must have a GeoSPARQL WKT literal.

Turtle export validates the graph before serialisation and fails publication when the shape constraints do not conform. This protects the API from publishing structurally incomplete graph state.

SHACL does **not** establish legal cadastral truth, geometry accuracy or source freshness; it validates graph shape only.

## GeoSPARQL query scope

The RDF is GeoSPARQL-compatible at the representation level. The current v1 runtime does not configure an external graph store and therefore does not claim support for SPARQL functions such as `geof:distance` or `geof:sfIntersects` in a remote endpoint.

Spatial predicates are evaluated with NetTopologySuite in the application layer. This decision keeps current functionality verifiable and avoids pretending a generic RDF library is a spatial database. A real GeoSPARQL-capable store can be added later if measured semantic query workloads require one.
