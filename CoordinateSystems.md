

# Introduction #

Cumberland provides the ability to do coordinate transformations.

Internally, cumberland uses the [Proj.4](http://trac.osgeo.org/proj/) C Library for this, and so it uses their format for describing projections: a proj4 string.  It is recommended that you review this site for information about proj.4 strings and about coordinate systems in general.

# Configuration #

Mac and Linux users will need to install the proj.4 library.  Most Linux distributions provide this directly.  For Mac, you can use [Fink](http://www.finkproject.org/).  Cumberland has been configured to run the Fink installation.

For Window users, cumberland comes with a build of proj.4.

_Note: There have been reports that this build of proj.4 will not work out of the box on Vista because it lacks msvcr71.dll by default.  This dll can be downloaded on the internet and it will work.  XP should work fine though._

# A Coordinate System Primer #

The earth is not flat, but your computer screen is.  Therefore, it is necessary to project the earth onto a flat surface.  Note that any sort of projection will cause some distortion (as Wikipedia [says](http://en.wikipedia.org/wiki/Map_projection) "You cannot flatten an orange peel without tearing or warping it"), so cartographers attempt to find
a projection that causes the least distortion in the area they want to map.

Additionally, the Earth is not a sphere.  It is more of an ellipsoid.  When dealing with coordinate systems, it's shape must be taken into account or there is more distortion.  So, we need a coordinate frame, or datum, that describes the Earth as accurately as possible.  The latest of these is WGS84 (World Geodetic System).  It is used by the Global Positioning System (GPS) and many others.  Data may or may not be projected, but it should always have a datum.

Now, with all the cartographers in all the world projecting their data in their own local way, shouldn't there be a way to coordinate this so data can be transferred with ease?  Well there is, actually there are multiple, but we'll refer to the [EPSG](http://www.epsg.org/) which provides a set of spatial reference ids (SRIDs) that we can use to lookup via Proj.4 to acquire our coordinate system.

# Finding our SRID #

If you are not sure what coordinate system your data is, you should check with your data provider.  If your data is in a Shapefile, most times it will come with a file with extension 'prj'.  This is its projection in [Well-Known Text](http://en.wikipedia.org/wiki/Well-known_text) format.

For example, here is one from some data I have:

```
PROJCS["Albers Conical Equal Area (Florida Geographic Data Library)",
GEOGCS["GCS_North_American_1983_HARN",DATUM["D_North_American_1983_HARN",
SPHEROID["GRS_1980",6378137.0,298.257222101]],PRIMEM["Greenwich",0.0],
UNIT["Degree",0.0174532925199433]],PROJECTION["Albers"],
PARAMETER["False_Easting",400000.0],PARAMETER["False_Northing",0.0],
PARAMETER["Central_Meridian",-84.0],PARAMETER["Standard_Parallel_1",24.0],
PARAMETER["Standard_Parallel_2",31.5],PARAMETER["Central_Parallel",24.0],
UNIT["Meter",1.0]]
```

For your curiosity, the 'PROJCS' section provides detail about the projection, and 'GEOGCS' provides details about the datum.  Now, we could actually take the above information and construct a proj.4 string, but it is likely that a SRID already exists for this projection.

One way to do this is pick out a few keywords from the PROJCS definition.  From the above, I select 'Florida', and 'Albers', and 'HARN'.  I can then look it up in the 'epsg' file that gets included with proj.4.  I find:

```
# NAD83(HARN) / Florida GDL Albers
<3087> +proj=aea +lat_1=24 +lat_2=31.5 +lat_0=24 +lon_0=-84 +x_0=400000 +y_0=0 +ellps=GRS80 +units=m +no_defs  <>
```

That's my SRID at the beginning and the proj.4 string after that.

# Coordinate systems in the API #

The ProjFourWrapper class in the Cumberland.Projection namespace provides all coordinate transformation functionality in Cumberland.  You can use this class directly to transform a Point programatically:

```
using (ProjFourWrapper source = new ProjFourWrapper(ProjFourWrapper.WGS84))
{
    using (ProjFourWrapper destination = new ProjFourWrapper(3087))
    {
        Point ll = new Point(-82,32);
        Point p = source.Transform(destination, ll);
    }
}
```

This code will transform the geographic point into the Florida Albers projection mentioned above.  A couple of notes:

  * A ProjFourWrapper must be disposed of as it maintains a reference into the Proj.4 c library
  * ProjFourWrapper.WGS84 is a convenience property that holds the proj.4 coordinate system for WGS84.  There is another, 'SphericalMercatorProjection', which holds the projection used in many popular web mapping applications (Google Maps, Microsoft Virtual Earth)

# Coordinate Systems in maps #

The [map xml format](http://code.google.com/p/cumberland/wiki/mapXmlFormat) has properties for setting the projection string for both the map and the individual layers.  This provides the ability to transform your data when the map is rendered.  Thus you could have multiple layers whose data sources have different coordinate systems rendered on one map.

If no projection is specified in the map, then no coordinate transformations occur.  If the map has a projection, but a layer does not, then that layer will not be transformed.

## SRIDs in map xml ##

If you want to use a SRID in your xml, you'll need to wrap it as so to create a Proj.4  string:

```
<Projection>+init=epsg:3087</Projection>
```