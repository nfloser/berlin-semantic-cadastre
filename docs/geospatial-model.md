# Geospatial model

## CRS policy

The geospatial pipeline has three explicit stages:

```text
Berlin source request
    EPSG:25833
        ↓
canonical domain / spatial calculation
    EPSG:25833 (metres)
        ↓
GIS interchange export
    EPSG:4326 (GeoJSON)
```

ETRS89 / UTM zone 33N is used internally because cadastral operations such as distance need projected metric coordinates. GeoJSON is transformed to WGS84 for interoperable GIS/web consumption.

A `GeometryReference` couples a NetTopologySuite geometry with a `CoordinateReferenceSystem`. Construction fails if the geometry SRID and declared CRS disagree, if the geometry is empty, or if it is topologically invalid.

## Geometry operations

`SpatialQueryService` currently provides:

- parcel coverage of buildings;
- parcel coverage of a point, including a point exactly on the boundary (`Covers` rather than strict `Contains`);
- parcel intersection with an analysis geometry;
- building intersection with an analysis geometry;
- metric building distance from a point;
- building membership in a district; and
- parcels containing more than a selected number of buildings.

Bounding envelopes are used as cheap prefilters where useful. They never replace the final exact topological predicate.

## Relationship derivation

Parcel → district is derived from the parcel's `PointOnSurface`. Building → parcel is derived from a building representative point against candidate parcel polygons. Building → district reuses the parcel district when available and otherwise tests the building representative point against districts.

`PointOnSurface` is intentionally preferred over a naive centroid because a centroid can fall outside a concave geometry. Even so, representative-point assignment is an approximation at boundaries and is documented as such.

## Invalid geometries

Source features are validated before entering the domain. The current v1 policy rejects invalid production geometries instead of mutating them silently with automatic buffer/repair operations. Rejected feature counts are surfaced in ingestion summaries/logging.

Future geometry-repair logic would require explicit provenance explaining both source and repaired geometry.

## Multipolygons and collections

NetTopologySuite operations are polymorphic across supported OGC geometry types. The source parser does not flatten MultiPolygon data into Polygon. Empty and invalid geometries are rejected. GeometryCollections can be parsed by the library, but downstream source compatibility is still covered by the live-source smoke workflow.

## Accepted API CRS

Spatial point and WKT intersection inputs accept:

- EPSG:25833 directly; or
- EPSG:4326, transformed into the internal projected CRS.

Other SRIDs are rejected rather than guessed. Metre distances are never computed directly over raw longitude/latitude coordinates.

## Transformation implementation

CRS transformation is isolated in `CoordinateTransformer` using ProjNET. The projected source definition is constructed explicitly as **ETRS89 / UTM zone 33N** with the ETRS89/ETRF89 datum, GRS80 ellipsoid, Transverse Mercator central meridian 15°, scale factor 0.9996, false easting 500000 m and false northing 0 m. This avoids mislabelling a WGS84/UTM geometry as EPSG:25833.

ProjNET models the predefined ETRS89 datum with zero Bursa-Wolf parameters to WGS84. That static approximation is appropriate for the project's visual/interchange GeoJSON path, but it is not presented as a time-dependent survey-grade transformation. Cadastral topology and metric calculations remain in the official EPSG:25833 source space and therefore do not depend on this export transformation.

Tests cover plausible Berlin output, wrong-SRID rejection and centimetre-level forward/reverse round trips for representative coordinates.
