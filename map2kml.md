# Introduction #

map2kml is a command line tool that takes a [map xml document](mapXmlFormat.md) and converts it into a Keyhole Markup Language (KML) document that can be loaded into Google Earth or Google Maps.  The kml document will preserve symbology (including thematic mapping) and export data from your feature source.  Each layer in your map will be a folder in the kml Document.

# Usage #

As of 0.3, map2kml takes two arguments: a [map xml file](mapXmlFormat.md) and the path to the new kml file.  It can be called as so:

```
map2kml mymap.xml  mymap.kml
```

Previous versions print out the kml directly to the console.

# Notes #

  * All styles used in the map must have a unique Id property or they will be ignored.

# Screenshot #

In Google Earth:

![http://cumberland.googlecode.com/files/screenshot-google-earth.jpg](http://cumberland.googlecode.com/files/screenshot-google-earth.jpg)