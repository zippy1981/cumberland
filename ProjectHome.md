Cumberland is a mapping framework for .Net.  It also includes a set of mapping tools that may be useful for people besides .NET developers.


---


<img src='http://cumberland.googlecode.com/files/9.png' align='right' />

# News #

  * July 28, 2009 - [0.4.2 released!](http://groups.google.com/group/cumberland-users/browse_thread/thread/ffdd20cfdd484db0)
  * Mar. 5, 2009 - 0.4.1 released to fix issue [#8](http://code.google.com/p/cumberland/issues/detail?id=8&can=1)
  * Feb. 28, 2009 - 0.4 has been released ([notes](http://code.google.com/p/cumberland/wiki/ReleaseNotes_For_0_4))
  * Feb. 10, 2009 - I created a [google group](http://groups.google.com/group/cumberland-users) if you have questions, ideas, etc.
  * Feb. 6, 2009 - I have [blogged](http://www.salmonsalvo.net/blog/?p=199) about simple map creation with xml schema and Visual Studio.
  * January 22, 2009 - Cumberland 0.3 is released!  Read the [release notes](ReleaseNotes_For_0_3.md) for details on what's new.
  * December 9, 2008 - Cumberland 0.2 released!  New in this release are significant improvements to symbology, including labelling.  Support for creating tiles for TMS (OpenLayers) and Microsoft VirtualEarth.  And export to Keyhole Markup Language (kml) functionality.

# Features #

  * a geometry/shape API
  * a [map xml format](mapXmlFormat.md) and serialization API
  * a map rendering engine
  * supports the following data sources:
    * Shapefiles (include reading .DBFs)
    * PostGIS
    * SQL Server 2008
  * a pluggable architecture for adding third-party support for other data sources
  * [coordinate system transformation / Projection](CoordinateSystems.md) (via [Proj.4](http://proj.maptools.org/))
  * [basic symbology](BasicSymbology.md), including labelling and thematic mapping
  * a [tile provider class](TileProviderWithAspNet.md) for generating tiles for popular web mapping applications:
    * Google Maps
    * VirtualEarth
    * Tile Map Service (TMS) - for use in OpenLayers
  * runs on Windows, Linux, and Mac OSX.
  * export map to KML API
  * OGC Standards
    * subset WellKnownText (WKT) parser/writer
    * subset WellKnownBinary (WKB) parser

> ## Tools ##

  * [shp2sqlserver](shp2sqlserver.md) - loads a Shapefile into SQL Server 2008
  * [drawmap](drawmap.md) - draws a map to an image
  * [tilepyramider](tilepyramider.md) - generates a set of tiles for use in the aforementioned web mapping applications
    * [Google Maps demo](http://www.salmonsalvo.net/Cumberland:GoogleMapsTest)
    * [OpenLayers demo](http://salmonsalvo.net/Cumberland:_OpenLayers_Test) ([another](http://salmonsalvo.net/Cumberland:_BaseMap_Test))
    * [VirtualEarth demo](http://salmonsalvo.net/Cumberland:_VirtualEarth_Test)
  * [map2kml](map2kml.md) - converts a map into a kml document

# Additional Resources #

  * API Documentation:
    * [0.4](http://salmonsalvo.net/cumberland/docs/0.4/Cumberland)
    * Older: ([0.3](http://salmonsalvo.net/cumberland/docs/0.3/Cumberland), [0.2](http://salmonsalvo.net/cumberland/docs/0.2/Cumberland), [0.1](http://salmonsalvo.net/cumberland/docs/0.1/Cumberland))
  * [Roadmap](Roadmap.md)
  * ["Create a simple application" tutorial](CreateASimpleApp.md)
