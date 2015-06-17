# Introduction #

shp2sqlserver is a command line tool for loading shapefiles into Microsoft SQL Server 2008.  It is modeled after [PostGIS'](http://postgis.refractions.net/)s shp2pgsql, except that it loads directly into the database instead of writing sql to stdout.

# Details #

Here is the usage:

```
shp2sqlserver.exe -h
Usage: shp2sqlserver.exe [OPTIONS]+ "connectionString" "path to shapefile"
Loads a shapefile into Microsoft SQL Server 2008

example: shp2sqlserver.exe "Data Source=.\SQLExpress2008;Initial Catalog=spatialtest;Integrated Security=true" myshape.shp

Options:
  -s, --srid=VALUE           The Spatial Reference ID (SRID).  If not specified it defaults to -1.
  -g, --geometry_column=VALUE       The name of the geometry column
  -t, --table_name=VALUE     The table name to use
  -k, --key_column=VALUE     The name of the identity column to create for a primary key
  -i, --index                Create a spatial index
  -l, --latlong              Add spatial data as geography type
  -a, --append               Append data.  If not specified, table will be created
  -h, --help                 show this message and exit
```

# Limitations #

  * This is alpha level software. I have loaded some complex shapefiles up to 20MB with it, but it could bomb on yours.
  * The Shapefile loader only supports Point, Polyline, Polygon, and there associated `*`M types (measurement value is ignored)