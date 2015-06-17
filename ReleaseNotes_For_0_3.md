0.3 is mainly a bugfix release but also contains a few new features



# New Features #

## Polygon fill textures ##

A new FillStyle option has been added.  'Texture' allows the use of an external image file which will be repeated within the interior of a polygon.

## Point symbols on line features ##

It is now possible to draw a point symbol on top of a line feature.  This is useful for road symbology.

Here is an example of both of these:

![http://cumberland.googlecode.com/files/cumberland_0_3_new.png](http://cumberland.googlecode.com/files/cumberland_0_3_new.png)

## Proj.4 Custom search path ##

Cumberland now allows a custom search path for looking up SRIDs and includes an EPSG file by default.  For Windows users, this means that Proj.4 coordinate systems transformations using spatial reference IDs (SRIDs) will work out of the box (no configuration required).

# Fixes #

Several bugs were fixed in this release:

  * [#1 -   	 shp2sqlserver: loading a polygon as geography fails](http://code.google.com/p/cumberland/issues/detail?id=1&can=1).  This was a result of Sql Server 2008 requiring geography polygon's exterior rings to be counter-clockwise ([details](http://blogs.msdn.com/edkatibah/archive/2008/08/19/working-with-invalid-data-and-the-sql-server-2008-geography-data-type-part-1b.aspx)).  By specification, Shapefiles use the opposite ring orientation.  [shp2sqlserver](shp2sqlserver.md) now flips all polygon rings to conform to SQL Server.  This has the added benefit of not forcing the db to flip ring orientation on geometries during spatial operations.
  * [#2 - EndOfStreamException](http://code.google.com/p/cumberland/issues/detail?id=2&can=1).  Cumberland now supports float columns in DBF files.
  * [#3 - Deserialization of map xml fails on german locale](http://code.google.com/p/cumberland/issues/detail?id=3&can=1).  This manifested itself on locales that use a comma for a decimal place.  Cumberland now forces the map xml format to be in the InvariantCulture (which is the default, en-US).
  * [#4 Export kml does not handle german characters](http://code.google.com/p/cumberland/issues/detail?id=4&can=1).  map2kml now properly creates utf-8 encoded xml.  Instead of piping to console out, the output path is now a required second argument.

## Other fixes ##

  * TileProvider now clips requested extents to within world extents (this prevents tiles from being created that will never be requested by tile consumers).
  * KeyholeMarkupLanguage now sniffs out polygon ring orientation based on the orientation of the first ring in a polygon.  This allows counter-clockwise exterior rings (e.g. SQL Server 2008).

_A big thank you to [Neil Young](http://foreverneilyoung.blogspot.com/) for his assistance in discovering and helping test solutions to all locale issues (several other issues as well)._

# Miscellaneous #

  * All command line tools now have a version option for getting the current version
  * map2kml now provides a help option