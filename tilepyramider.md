

# Introduction #

Most modern web mapping interfaces use tiles.  This has the advantage of allowing the tiles to be pre-generated instead of created per user as traditional web mapping has done (think ArcIMS).  Tilepyramider generates a pyramid of tile images into a directory structure that you can use with these interfaces.  The output can be copied to your web server.

Supported tile consumers:

  * Google Maps
  * TileMapService (TMS) - can be used in OpenLayers (new to 0.2)
  * Microsoft VirtualEarth (new to 0.2)

# Usage #

Tilepyramider is a command line utility, its usage is as follows:

```
Usage: [mono] tilepyramider.exe [OPTIONS]+ "path to map file" 
Generates a pyramid of tile images for use for popular web mapping interfaces

example: mono tilepyramider.exe  /path/to/map 

Options:
  -e, --extents=VALUE        comma-delimited extents for clipping tile generation (e.g. -180,-90,180,90).  Overrides the map file.  (Must be in map's coordinate system)
  -h, --help                 show this message and exit
  -o, --output=VALUE         the path of the where to create.  Defaults to current directory
  -x, --maxzoom=VALUE        the maximum zoom level
  -n, --minzoom=VALUE        the minimum zoom level
  -t, --test                 Test - only calculate the total and return
  -c, --consumer=VALUE       The consumer.  Valid values are 'googlemaps', 'tms', and 've'.
  -w, --worldextents=VALUE   comma-delimited extents for defining world (e.g. -180,-90,180,90). Valid only for TMS
  -b, --bleed=VALUE          the bleed in pixels for tiles (useful for catching overrunning symbols/labels from other tiles
```

The "map file" is defined by the [map xml format](mapXmlFormat.md) which holds data sources and style information.  This generates a set of images in the output folder specified.  If no output folder is specified, the tiles are generated in the current directory.  The maxzoom and minzoom are used for restricting the zoom levels in which you want to generate tiles (as all tiles at all levels will be many tiles).  The worldextents is only for customizing TMS output.

## Testing the tile count ##

You may want to test the number of tiles that are generated.  Apply the test option and it will return before generating tiles:

```
>tilepyramider -t -x=6 cumberland\cumberland\Cumberland.Tests\maps\mexico.xml
0  of 64
>
```

## Projection properties ##

Google maps and Virtual Earth use as spherical mercator projection for tiles, so unless your data is in this coordinate system (which it probably is not), you'll need to set some properties in your map xml to let cumberland know what coordinate system to transform from:

  1. Map.Projection - set this to the coordinate system of the extents of your map and those defined in the extents option.
  1. Layer.Projection - each layer will need this property set which defines the coordinate system of the underlying data.

For more details on coordinate systems and setting this property, see the [CoordinateSystems](CoordinateSystems.md) page.

## Setting a bleed ##

The bleed option provides the ability to have the tool render a larger map and crop it to size.  This way it can catch features whose symbology may extend across a tile.  The below diagram shows before and after a bleed is set in a Virtual Earth application.  Notice the vertical line in the left image where point data is not rendered which extends over the neighboring tile.

![http://cumberland.googlecode.com/files/bleed-comparison.jpg](http://cumberland.googlecode.com/files/bleed-comparison.jpg)

_Note: A bleed of 5 works for this data because the point symbols are of size 5._

# Deployment: Google Maps #

Check out the [tilepyramider and google maps page](TilePyramiderAndGoogleMaps.md) for details.

# Deployment: Virtual Earth #

Your page will need to reference the VE script:

```
<script charset="UTF-8" type="text/javascript" src="http://dev.virtualearth.net/mapcontrol/mapcontrol.ashx?v=6.2"></script>
```

Add a method for adding your tile overlay:

```
function init()
{
    var map = new VEMap('myMap');
    map.LoadMap(new VELatLong(21,-100), 3, null, null, null, null, null, null);

    var bounds = [
        new VELatLongRectangle(new VELatLong(34,-120),
                new VELatLong(12,-82))];
    var layerID = "test";
    var tileSource =
        "ve/%4.png";
    var tileSourceSpec =
        new VETileSourceSpecification(layerID, tileSource);
    tileSourceSpec.NumServers = 1;
    tileSourceSpec.Bounds     = bounds;
    tileSourceSpec.MinZoom    = 1;
    tileSourceSpec.MaxZoom    = 5;
    tileSourceSpec.Opacity    = 1;
    tileSourceSpec.ZIndex     = 99;

    map.AddTileLayer(tileSourceSpec, true);
}
```

tileSource should point to the directory that tilepyramider created.  Finally, call that method and create the map div:

```
<body onload="init();">
<div id='myMap' style="position:absolute; width:400px; height:400px;"></div>
...
```

An example is available here: http://salmonsalvo.net/Cumberland:_VirtualEarth_Test

# Deployment: TMS to Open Layers #

Check out the [tilepyramider and OpenLayers page](TilePyramiderAndOpenLayers.md) for details.