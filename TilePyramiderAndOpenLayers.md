

# Introduction #

TMS is an [OSGeo specification](http://wiki.osgeo.org/wiki/Tile_Map_Service_Specification), but our use is mainly for [OpenLayers](http://openlayers.org/), which is an open source web mapping interface comparable to Google Maps or Microsoft Virtual Earth.

Unlike these other interfaces, you will need to supply your own tiles for base data (states, roads, cities, etc.).  Cumberland's [tilepyramider](tilepyramider.md) allows you to build these tiles and the following article will help you to configure and deploy them with OpenLayers.

# Building the tile pyramid #

We first build our tile pyramid, which is a directory structure of tile images based on zoom level and x and y coordinates.  The tilepyramider command requires an argument that is the path to a cumberland map xml file.  This is a [simple xml format](mapXmlFormat.md) which stores data source info (shapefile, database) and symbology (colors, line widths, etc.).

We call the tool as so:

```
tilepyramider.exe -e=-180,-90,180,90 -o=/home/scottell/Desktop/tms -c=tms -x=6 ../../../Cumberland.Tests/maps/mexico.xml
```

The 'c' option is what tells the tool that we want our output to be for TMS.  The 'x' option prevents defines that max  zoom level will be 6.  The default minimum zoom level is 0, thus this will generate 7 levels of tiles. The 'e' option allow us to override the extents as defined in the map file.

In our example we are outputting our tiles in geographic coordinates (lat/longs), which is common in OpenLayers, but there is nothing stopping you from using your own coordinate system in a TMS (though I have not tried this in OpenLayers).  However, you will need to set the 'w' option to the world extents of your map as the default is geographic.

# Deployment #

The above command will create a 'tms' directory.  You can copy this to your server, and then all that is necessary is to set up your web page.  First, add a script reference to OpenLayers:

```
<script src="http://www.openlayers.org/api/OpenLayers.js" type="text/javascript"></script>
```

And write a javascript method to create your map and load your TMS:

```
function initialize()
{
    var options = {
        maxExtent: new OpenLayers.Bounds(-180, -90, 180, 90),
            numZoomLevels: 6,
    };

    var map = new OpenLayers.Map('map', options);
    var layer = new OpenLayers.Layer.TMS( "Mexico", ["http://localhost/~scottell/"],
    {
        layername: 'tms',
        serviceVersion: '',
        type:'png',
    } );
   
    map.addLayer(layer);
    map.setCenter(new OpenLayers.LonLat(-105, 24), 4);
}
```

You'll need to edit to fit your site and setup.  Finally, call the above method on load and add your div which will hold the map:

```
<body onload="initialize()">
    <div id="map" style="width: 500px; height: 300px"></div>
```

# TMS that are a subset of the map boundaries #

Currently, a TMS has to have an EPSG of 4326 and worldwide extents to work in OpenLayers ([source](http://openlayers.org/pipermail/users/2008-April/005449.html)). This means you can't clip to a certain region by default (you'll get broken image links in outlying areas).

However, we can workaround this.  In the above example, the call to tilepyramider passes extents that are worldwide.  If we changed this to just a subset of the world, we would get dramatically fewer images.  If we want to use this, we need to override the 'getURL' method of a TMS and handle it ourselves:

```
var layer = new OpenLayers.Layer.TMS("Name", "http://localhost/~scottell", { 'type':'png', 'getURL':getTileUrl });
```

We then define that method and check if the tile's boundaries are within our extents and zoom levels.  If not, we simple pass a URL to a blank image tile:

```
  function getTileUrl (bounds)
  {
  		if (bounds.left > -82 ||
  			bounds.right <  -120 ||
  			bounds.top < 12 ||
  			bounds.bottom > 34 ||
  			map.getZoom() > 8)
  		{
  			return 'blanktile.png';
  		}
  
        var res = this.map.getResolution();
        var x = Math.round ((bounds.left - this.maxExtent.left) / (res * this.tileSize.w));
        var y = Math.round ((bounds.bottom - this.maxExtent.bottom) / (res * this.tileSize.h));
        var z = this.map.getZoom();
        
        return "tms/" + z + "/" + x + "/" + y + "." + this.type; 
   }    
```

# Example #

A example site is available here: http://salmonsalvo.net/Cumberland:_OpenLayers_Test