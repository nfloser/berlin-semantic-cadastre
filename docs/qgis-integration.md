# QGIS integration

QGIS is the primary visual analysis companion for this project. The API supplies queryable/exportable data; it does not reimplement a GIS desktop.

## 1. Start with real Berlin data

```bash
cp .env.example .env
docker compose up --build
```

The container enables live WFS ingestion. Check process health:

```bash
curl http://localhost:8080/health
```

Then check data readiness:

```bash
curl -i http://localhost:8080/ready
```

A `200` response means the bounded runtime contains parcels, buildings and districts. A `503` means the process is alive but required data has not been loaded, usually because ingestion is still running or a source request failed.

## 2. Export vector layers

The following URLs expose GeoJSON FeatureCollections:

```text
http://localhost:8080/export/geojson?type=parcels
http://localhost:8080/export/geojson?type=buildings
http://localhost:8080/export/geojson?type=districts
```

The coordinates are transformed to EPSG:4326. The response also includes a top-level metadata object documenting output and source CRS.

You can either point QGIS at the HTTP(S) resource through **Data Source Manager → Vector → Protocol**, or save a response locally first:

```bash
curl "http://localhost:8080/export/geojson?type=parcels" -o parcels.geojson
curl "http://localhost:8080/export/geojson?type=buildings" -o buildings.geojson
curl "http://localhost:8080/export/geojson?type=districts" -o districts.geojson
```

Then add the files through **Layer → Add Layer → Add Vector Layer**.

## 3. Inspect attributes

Open each layer's attribute table. The export retains:

- canonical feature identifier;
- feature type;
- derived parcel/district identifiers where available;
- source dataset;
- source feature identifier;
- source endpoint;
- retrieval timestamp; and
- original source properties returned by the WFS parser.

This lets a GIS user inspect both application relationships and upstream attributes without coupling the C# domain model to every WFS field.

## 4. Style and combine

A useful visual composition is:

- districts with transparent fill and visible boundary;
- parcels with thin outlines;
- buildings with a distinct fill; and
- labels/tooltips using canonical IDs or selected source attributes.

Because the result is standard GeoJSON, it can be combined with other Berlin layers, reprojected into a project CRS, filtered, styled and analysed with normal QGIS tooling.

## 5. Use a spatial API result

For targeted analysis, call a spatial endpoint and save its JSON response, or first identify an entity through the API and export the relevant layer. The query DTO endpoints expose internal WKT in EPSG:25833 for analytical transparency; the dedicated GeoJSON endpoint is the GIS interchange path.

Example nearby query using WGS84 input:

```bash
curl "http://localhost:8080/spatial/nearby?x=13.405&y=52.52&distanceMetres=250&srid=4326"
```

The server transforms the input before evaluating the 250-metre distance.

## 6. Semantic companion export

```bash
curl http://localhost:8080/export/turtle -o cadastre.ttl
```

Turtle is not a replacement for the vector layer. It expresses linked identity, GeoSPARQL geometry, provenance, and parcel/building/district relationships for semantic tooling. Use GeoJSON for normal QGIS geometry visualisation and Turtle when RDF interoperability is required.

## Reproducibility note

The default `.env.example` uses a bounded EPSG:25833 area. If you change `CADASTRE_INITIAL_BBOX`, record that extent with any map/screenshot so the visual result can be reproduced. Do not present a synthetic fixture map as a screenshot of live Berlin data.
