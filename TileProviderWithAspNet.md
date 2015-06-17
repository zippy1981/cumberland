# Introduction #

_Note: This article uses Google Maps as the tile consumer, but it could easily be translated to Virtual Earth (or likely even Open Layers)_

The TileProvider class provides the ability to generate image tiles for use in Google Maps.  You can overlay these tiles to add your own spatial data on top of Google's base data.  The [tilepyramider](tilepyramider.md) tools provides the ability to pre-generate the tiles, but this tutorial will show how to use this class directly within Asp.Net to create tiles on-the-fly.

# Step 1: Our map page #

The page that will show our google map must be configured, I will omit the details here as they are provided in the [tilepyramider](tilepyramider.md) page.  However, you will need to set up your tile overlay differently because we want to use a URL to acquire tiles instead of the images:

```
// Set up the copyright information
// Each image used should indicate its copyright permissions
var myCopyright = new GCopyrightCollection("©");
myCopyright.addCopyright(new GCopyright('Demo',
	new GLatLngBounds(new GLatLng(-90,-180), new GLatLng(90,180)),
	0,'©'));
				
var tilelayers = new GTileLayer(myCopyright, 0, 19);
tilelayers.getTileUrl = CustomGetTileUrl;
			
myTileLayer = new GTileLayerOverlay(tilelayers);	
map.addOverlay(myTileLayer);
```

This simply tells google maps to use the CustomGetTileUrl prototype method to get the URL for each tile.  This method must have the correct parameters that specify the tile and zoomlevel.  Ours is:

```
function CustomGetTileUrl(a,b) 
{
	return "TileProvider.aspx?x=" + a.x + "&y=" + a.y + "&zl=" + b;
}			
```

So, TileProvider.aspx will get each tile request.  Let's define that.

# Step 2: the tile provider web page #

TileProvider.aspx is a simple page (it needs no designer controls).  All that it does is pipe out a PNG image, and this will be done in the code-behind.  First, we need to clear out the Response and set the content-type to png:

```
public partial class TileProvider : System.Web.UI.Page
{
	protected override void OnLoad (EventArgs e)
	{
		base.OnLoad (e);
		
		// clear out response
		Response.Clear();
		Response.ContentType = "image/png";
```

Next, we pull our tile info out of the query string:

```
		// acquire the tile index and zoom level
		int x = Convert.ToInt32(Request.QueryString["x"]);
		int y = Convert.ToInt32(Request.QueryString["y"]);
		int zoomLevel = Convert.ToInt32(Request.QueryString["zl"]);
```

At this point, if we wanted to store cached tiles in a database, we could check here to see if the tile exists.  If so, return it.  If not, continue and render the tile.  We are not caching, so we must now get our map (we use the serialization API to do this) and instantiate our tile provider:

```
		MapSerializer ms = new MapSerializer();
		ms.AddDatabaseFeatureSourceType(typeof(PostGISFeatureSource));
		Map map = ms.Deserialize("/path/to/map.xml");
		Cumberland.Web.TileProvider tp = new Cumberland.Web.TileProvider(TileConsumer.GoogleMaps);
```

_Note: We could also store the Map object in session for fast access._

Finally, we render our map, and flush it into the response output stream:

```
		using (Bitmap b = tp.DrawTile(map, x, y, zoomLevel))
		{
			// stream out the image
			b.Save(Response.OutputStream, ImageFormat.Png);
		}
		
		Response.End();
```

_Note: If we wanted to cache in a database, we'd store Bitmap b, along with the x,y and zoom level in a new record._

And that's it: on-the-fly rendered tiles.