<%@ Page Language="C#" Inherits="Cumberland.TileViewerTest.Default" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head>
	<title>Default</title>
 	<script src="http://maps.google.com/maps?file=api&amp;v=2&amp;key=ABQIAAAAa5sosIiO91cWE45WLzdVYRRi_j0U6kJrkFvY4-OX2XYmEAa76BQVIUdpeziAPVqrtBLTyQRXi9jh7w"
            type="text/javascript"></script>
    <script type="text/javascript">

		function CustomGetTileUrl(a,b) 
		{
			return "TileProvider.aspx?x=" + a.x + "&y=" + a.y + "&zl=" + b;
		}			

	    function initialize() 
	    {
	      if (GBrowserIsCompatible()) 
	      {
	        var map = new GMap2(document.getElementById("map_canvas"));
	        map.setCenter(new GLatLng(29.5, -100.1419), 6);
	        map.addControl(new GSmallMapControl());
	        map.addControl(new GMapTypeControl());
	        
			// Set up the copyright information
			// Each image used should indicate its copyright permissions
			var myCopyright = new GCopyrightCollection("©");
			myCopyright.addCopyright(new GCopyright('Demo',
			  new GLatLngBounds(new GLatLng(-90,-180), new GLatLng(90,180)),
			  0,'©'));
				
			var tilelayers = new GTileLayer(myCopyright, 0, 19);
			tilelayers.getTileUrl = CustomGetTileUrl;
			
			var myTileLayer = new GTileLayerOverlay(tilelayers);	
			map.addOverlay(myTileLayer);
	      }
	    }
	    
    </script>

</head>
<body onload="initialize()" onunload="GUnload()">
	<form id="form1" runat="server">
		<asp:button id="button1" runat="server" Text="Click me!" OnClick="button1Clicked" />
		
		<div id="map_canvas" style="width: 500px; height: 300px"></div>

		
		
	</form>
</body>
</html>